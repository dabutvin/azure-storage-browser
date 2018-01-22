
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Akavache;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorageBrowser.Activities
{
    [Activity(Label = "BlobActivity")]
    public class BlobActivity : BaseActivity
    {
        CloudBlobClient blobClient;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Blob);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");

            var accountLabel = FindViewById<TextView>(Resource.Id.account_label);
            accountLabel.Text = account.Name;

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            blobClient = storageAccount.CreateCloudBlobClient();

            await BindContainersAsync($"{account.Id}/containers");
            await RefreshContainersAsync($"{account.Id}/containers");
            await BindContainersAsync($"{account.Id}/containers");

        }

        private async Task RefreshContainersAsync(string id)
        {
            var containers = new List<string>();
            BlobContinuationToken continuationToken = null;
            do
            {
                var containerSegment = await blobClient.ListContainersSegmentedAsync(continuationToken);

                containers.AddRange(containerSegment.Results.Select(x => x.Name));
                continuationToken = containerSegment.ContinuationToken;

            } while (continuationToken != null);

            await BlobCache.LocalMachine.InsertObject(id, containers.ToArray());
        }

        private async Task BindContainersAsync(string id)
        {
            try
            {
                var containers = await BlobCache.LocalMachine.GetObject<string[]>(id);

                if (containers != null)
                {
                    var containersListView = FindViewById<ListView>(Resource.Id.containers);

                    containersListView.Adapter = new ArrayAdapter<string>(
                        this,
                        Android.Resource.Layout.SimpleListItem1,
                        containers.ToArray());
                }
            }
            catch(KeyNotFoundException){}
        }
    }
}
