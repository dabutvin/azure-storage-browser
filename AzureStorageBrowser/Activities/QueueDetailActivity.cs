using System.Reactive.Linq;
using System.Linq;
using Akavache;
using Android.App;
using Android.OS;
using Android.Widget;
using Microsoft.WindowsAzure.Storage;

namespace AzureStorageBrowser.Activities
{
    [Activity]
    public class QueueDetailActivity : BaseActivity
    {
        ProgressBar progressBar;
        TextView pageCount;
        ListView messagesListView;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            SetContentView(Resource.Layout.QueueDetail);

            progressBar = FindViewById<ProgressBar>(Resource.Id.progress);
            pageCount = FindViewById<TextView>(Resource.Id.pagecount);
            messagesListView = FindViewById<ListView>(Resource.Id.messages);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");
            var queueName = await BlobCache.LocalMachine.GetObject<string>("selectedQueue");

            Title = $"{account.Name} > {queueName}";

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            var queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry();

            var queue = queueClient.GetQueueReference(queueName);
            await queue.FetchAttributesAsync();

            var messages = await queue.PeekMessagesAsync(32);

            progressBar.Visibility = Android.Views.ViewStates.Gone;

            if (messages.Any() == false)
            {
                var emptyMessage = FindViewById<TextView>(Resource.Id.empty);
                emptyMessage.Visibility = Android.Views.ViewStates.Visible;
                pageCount.Text = "0 / 0";
            }
            else
            {
                pageCount.Text = $"{messages.Count()} / {queue.ApproximateMessageCount}";
            }

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
