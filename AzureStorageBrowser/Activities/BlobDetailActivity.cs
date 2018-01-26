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
using Android.Graphics;
using System.Threading.Tasks;
using System.Net.Http;
using Android.Views;
using Android.Util;

namespace AzureStorageBrowser.Activities
{
    [Activity(Label = "BlobDetailActivity", NoHistory = true)]
    public class BlobDetailActivity : BaseActivity
    {
        ImageView imageView;
        TextView textView;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.BlobDetail);

            imageView = FindViewById<ImageView>(Resource.Id.blobImageView);
            textView = FindViewById<TextView>(Resource.Id.blobTextView);

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


            blobsListView.ItemClick += async delegate(object sender, AdapterView.ItemClickEventArgs e)
            {
                var blob = blobs.Results.OfType<CloudBlockBlob>().ElementAt(e.Position);

                if (blob.IsImage())
                {
                    var bitmap = await GetBitmap(blob);

                    imageView.SetImageBitmap(bitmap);
                    imageView.Visibility = ViewStates.Visible;
                }
                else
                {
                    textView.Text = await blob.DownloadTextAsync();
                    textView.Visibility = ViewStates.Visible;
                }
            };

            imageView.Click += delegate
            {
                imageView.Visibility = ViewStates.Gone;
                imageView.SetImageBitmap(null);
            };

            textView.Click += delegate
            {
                textView.Visibility = ViewStates.Gone;
                textView.Text = null;
            };
        }

        private async Task<Bitmap> GetBitmap(CloudBlockBlob blob)
        {
            Bitmap imageBitmap = null;

            try
            {
                var imageBytes = new byte[blob.Properties.Length];

                await blob.DownloadToByteArrayAsync(imageBytes, 0);

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }
            catch{}

            return imageBitmap;
        }
    }
}
