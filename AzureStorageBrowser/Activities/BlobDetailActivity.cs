using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter.Analytics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace AzureStorageBrowser.Activities
{
    [Activity]
    public class BlobDetailActivity : BaseActivity
    {
        SwipeRefreshLayout refresher;
        ImageView imageView;
        TextView textView;
        ProgressBar progressBar;
        ListView blobsListView;
        CloudBlobContainer container;
        CloudBlockBlob[] blobs;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            SetContentView(Resource.Layout.BlobDetail);

            refresher = FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);
            imageView = FindViewById<ImageView>(Resource.Id.blobImageView);
            textView = FindViewById<TextView>(Resource.Id.blobTextView);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progress);
            blobsListView = FindViewById<ListView>(Resource.Id.blobs);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");
            var containerName = await BlobCache.LocalMachine.GetObject<string>("selectedContainer");

            Title = $"{account.Name} > {containerName}";

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry();

            container = blobClient.GetContainerReference(containerName);

            blobsListView.ItemClick += async delegate (object sender, AdapterView.ItemClickEventArgs e)
            {
                if (blobs.Any() == false) { return; }

                Analytics.TrackEvent("blobdetail-blob-clicked");

                var blob = blobs.ElementAt(e.Position);

                if (blob.IsImage())
                {
                    var bitmap = await GetBitmap(blob);

                    imageView.SetImageBitmap(bitmap);
                    imageView.Visibility = ViewStates.Visible;
                }
                else
                {
                    var text = await blob.DownloadTextAsync();
                    var prettyText = ShittyPrettyPrint(text);
                    textView.Text = prettyText;
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

            await LoadBlobs();

            refresher.Refresh += async delegate
            {
                Analytics.TrackEvent("blobdetail-blobs-refreshed");
                await LoadBlobs();
                refresher.Refreshing = false;
            };
        }

        private async Task LoadBlobs()
        {
            progressBar.Visibility = ViewStates.Visible;

            blobs = (await container.ListBlobsSegmentedAsync(null))
                .Results.OfType<CloudBlockBlob>()
                .ToArray();

            var displayBlobNames = blobs.Select(x => x.Name).ToArray();

            progressBar.Visibility = ViewStates.Gone;

            if (blobs.Any() == false)
            {
                displayBlobNames = new[] { "    ~~  No blobs  ~~    " };
            }

            blobsListView.Adapter = new ArrayAdapter<string>(this,
                                                             Android.Resource.Layout.SimpleListItem1,
                                                             displayBlobNames);
        }

        private string ShittyPrettyPrint(string text)
        {
            try
            {
                return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(text), Formatting.Indented);    
            }
            catch
            {
                return text;
            }
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
