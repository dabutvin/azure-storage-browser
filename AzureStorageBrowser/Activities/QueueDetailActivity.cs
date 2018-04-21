using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Widget;
using Microsoft.AppCenter.Analytics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorageBrowser.Activities
{
    [Activity]
    public class QueueDetailActivity : BaseActivity
    {
        SwipeRefreshLayout refresher;
        ProgressBar progressBar;
        TextView pageCount;
        ListView messagesListView;
        TextView emptyMessage;
        CloudQueue queue;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            SetContentView(Resource.Layout.QueueDetail);

            refresher = FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progress);
            pageCount = FindViewById<TextView>(Resource.Id.pagecount);
            messagesListView = FindViewById<ListView>(Resource.Id.messages);
            emptyMessage = FindViewById<TextView>(Resource.Id.empty);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");
            var queueName = await BlobCache.LocalMachine.GetObject<string>("selectedQueue");

            Title = $"{account.Name} > {queueName}";

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            var queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry();

            queue = queueClient.GetQueueReference(queueName);

            await LoadQueueDetails();

            refresher.Refresh += async delegate
            {
                Analytics.TrackEvent("queuedetail-messages-refreshed");
                await LoadQueueDetails();
                refresher.Refreshing = false;
            };
        }

        private async Task LoadQueueDetails()
        {
            emptyMessage.Visibility = Android.Views.ViewStates.Gone;
            progressBar.Visibility = Android.Views.ViewStates.Visible;

            await queue.FetchAttributesAsync();
            var messages = await queue.PeekMessagesAsync(32);

            progressBar.Visibility = Android.Views.ViewStates.Gone;

            if (messages.Any() == false)
            {
                emptyMessage.Visibility = Android.Views.ViewStates.Visible;
                pageCount.Text = "0 / 0";
            }
            else
            {
                pageCount.Text = $"{messages.Count()} / {queue.ApproximateMessageCount}";
            }

            Analytics.TrackEvent(
                "queuedetail-messages-fetched",
                new Dictionary<string, string> { ["count"] = messages.Count().ToString() });

            var displayMessages = messages.Select(x =>
            {
                try
                {
                    return x.AsString;
                }
                catch
                {
                    return "[ERROR] Unable to read message";
                }
            }).ToArray();

            messagesListView.Adapter = new ArrayAdapter<string>(this,
                                                                Android.Resource.Layout.SimpleListItem1,
                                                                displayMessages);
        }
    }
}
