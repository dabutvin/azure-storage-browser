using System;
using System.Reactive.Linq;
using System.Linq;
using Akavache;
using Android.App;
using Android.OS;
using Android.Widget;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorageBrowser.Activities
{
    [Activity(Label = "BlobDetailActivity", NoHistory = true)]
    public class BlobDetailActivity : BaseActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.BlobDetail);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");
            var containerName = await BlobCache.LocalMachine.GetObject<string>("selectedContainer");

            var accountLabel = FindViewById<TextView>(Resource.Id.account_label);
            accountLabel.Text = account.Name;

            var containerLabel = FindViewById<TextView>(Resource.Id.container_label);
            containerLabel.Text = containerName;

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(containerName);

            var blobsListView = FindViewById<ListView>(Resource.Id.blobs);

            var blobs = await container.ListBlobsSegmentedAsync(null);

            blobsListView.Adapter = new ArrayAdapter<string>(
                this,
                Android.Resource.Layout.SimpleListItem1,
                blobs.Results.OfType<CloudBlockBlob>().Select(x => x.Name).ToArray());
        }
    }
}
