using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using Android.App;
using Android.OS;
using Android.Widget;
using Microsoft.AppCenter.Analytics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorageBrowser.Activities
{
    [Activity]
    public class QueueActivity : BaseActivity
    {
        CloudQueueClient queueClient;
        ListView queuesListView;
        ProgressBar progressBar;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            SetContentView(Resource.Layout.Queue);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");

            Title = $"{account.Name} > queues";

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry();

            queuesListView = FindViewById<ListView>(Resource.Id.queues);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progress);

            BindQueueClick($"{account.Id}/queues");
            await BindQueuesAsync($"{account.Id}/queues");
            await RefreshQueuesAsync($"{account.Id}/queues");
            await BindQueuesAsync($"{account.Id}/queues");
        }

        private void BindQueueClick(string id)
        {
            queuesListView.ItemClick += async delegate (object sender, AdapterView.ItemClickEventArgs e)
            {
                if (e.Position > -1)
                {
                    Analytics.TrackEvent("queue-queue-clicked");
                    var queues = await BlobCache.LocalMachine.GetObject<string[]>(id);
                    await BlobCache.LocalMachine.InsertObject("selectedQueue", queues[e.Position]);
                    StartActivity(typeof(QueueDetailActivity));
                }
            };
        }

        private async Task RefreshQueuesAsync(string id)
        {
            var queues = new List<string>();
            QueueContinuationToken continuationToken = null;
            do
            {
                var queueSegment = await queueClient.ListQueuesSegmentedAsync(continuationToken);

                queues.AddRange(queueSegment.Results.Select(x => x.Name));
                continuationToken = queueSegment.ContinuationToken;

            } while (continuationToken != null);

            Analytics.TrackEvent(
                "queue-queues-fetched",
                new Dictionary<string, string> { ["count"] = queues.Count().ToString() });

            await BlobCache.LocalMachine.InsertObject(id, queues.ToArray());
            progressBar.Visibility = Android.Views.ViewStates.Gone;

            if (queues.Any() == false)
            {
                var emptyMessage = FindViewById<TextView>(Resource.Id.empty);
                emptyMessage.Visibility = Android.Views.ViewStates.Visible;
            }
        }

        private async Task BindQueuesAsync(string id)
        {
            try
            {
                var queues = await BlobCache.LocalMachine.GetObject<string[]>(id);

                if (queues != null)
                {
                    queuesListView.Adapter = new ArrayAdapter<string>(
                        this,
                        Android.Resource.Layout.SimpleListItem1,
                        queues.ToArray());
                }
            }
            catch (KeyNotFoundException)
            {
                progressBar.Visibility = Android.Views.ViewStates.Visible;
            }
        }
    }
}
