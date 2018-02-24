using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using Android.App;
using Android.OS;
using Android.Widget;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorageBrowser.Activities
{
    [Activity]
    public class BlobActivity : BaseActivity
    {
        CloudBlobClient blobClient;
        ListView containersListView;
        ProgressBar progressBar;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            SetContentView(Resource.Layout.Blob);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");

            Title = $"{account.Name} > containers";

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry();

            containersListView = FindViewById<ListView>(Resource.Id.containers);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progress);

            BindContainerClick($"{account.Id}/containers");
            await BindContainersAsync($"{account.Id}/containers");
            await RefreshContainersAsync($"{account.Id}/containers");
            await BindContainersAsync($"{account.Id}/containers");
        }

        private void BindContainerClick(string id)
        {
            containersListView.ItemClick += async delegate (object sender, AdapterView.ItemClickEventArgs e)
            {
                if (e.Position > -1)
                {
                    var containers = await BlobCache.LocalMachine.GetObject<string[]>(id);
                    await BlobCache.LocalMachine.InsertObject("selectedContainer", containers[e.Position]);
                    StartActivity(typeof(BlobDetailActivity));
                }
            };
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
            progressBar.Visibility = Android.Views.ViewStates.Gone;
        }

        private async Task BindContainersAsync(string id)
        {
            try
            {
                var containers = await BlobCache.LocalMachine.GetObject<string[]>(id);

                if (containers != null)
                {
                    containersListView.Adapter = new ArrayAdapter<string>(
                        this,
                        Android.Resource.Layout.SimpleListItem1,
                        containers.ToArray());
                }
            }
            catch(KeyNotFoundException)
            {
                progressBar.Visibility = Android.Views.ViewStates.Visible;
            }
        }
    }
}
