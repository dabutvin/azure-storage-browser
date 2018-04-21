using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akavache;
using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
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
        CloudQueueMessage[] messages;
        CloudQueueClient queueClient;

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

            queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry();

            queue = queueClient.GetQueueReference(queueName);

            await LoadQueueDetails();

            refresher.Refresh += async delegate
            {
                Analytics.TrackEvent("queuedetail-messages-refreshed");
                await LoadQueueDetails();
                refresher.Refreshing = false;
            };

            RegisterForContextMenu(messagesListView);
        }

        private async Task LoadQueueDetails()
        {
            emptyMessage.Visibility = Android.Views.ViewStates.Gone;
            progressBar.Visibility = Android.Views.ViewStates.Visible;

            await queue.FetchAttributesAsync();
            messages = (await queue.PeekMessagesAsync(32)).ToArray();

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

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if(v.Id == messagesListView.Id)
            {
                var info = (AdapterView.AdapterContextMenuInfo)menuInfo;

                if (info.Position == 0) // Can only delete the first message without throwing off DQ count
                {
                    menu.Add(Menu.None, 1, 0, "Delete");

                    if (queue.Name.EndsWith("-poison", StringComparison.Ordinal))
                    {
                        menu.Add(Menu.None, 2, 0, "Reprocess");
                    }
                }
            }
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;

            var message = messages[info.Position];

            if (item.ItemId == 1)
            {
                Analytics.TrackEvent("queuedetail-message-deleted");
                DeleteMessageAsync(message);
            }

            if (item.ItemId == 2)
            {
                Analytics.TrackEvent("queuedetail-message-reprocessed");
                ReprocessMessageAsync(message);
            }

            return true;
        }

        private async Task DeleteMessageAsync(CloudQueueMessage message)
        {
            try
            {
                var topMessage = await queue.GetMessageAsync(TimeSpan.FromSeconds(1), new QueueRequestOptions(), new OperationContext());

                if(topMessage != null)
                {
                    if (topMessage.Id == message.Id)
                    {
                        await queue.DeleteMessageAsync(topMessage.Id, topMessage.PopReceipt);
                    }
                    else
                    {
                        // dequeued the wrong message :( someone else DQ'd it before we got a chance to
                        // Wait 1 second for the DQ'd message to come back
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception e)
            {
                // ignore
            }

            await LoadQueueDetails();
        }

        private async Task ReprocessMessageAsync(CloudQueueMessage message)
        {
            var destQueue = queueClient.GetQueueReference(queue.Name.Replace("-poison", ""));
            if(await destQueue.ExistsAsync())
            {
                await destQueue.AddMessageAsync(new CloudQueueMessage(message.AsString));
                await DeleteMessageAsync(message);
            }
        }
    }
}
