using System;
using System.Reactive.Linq;
using System.Linq;
using Akavache;
using Android.App;
using Android.OS;
using Android.Widget;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorageBrowser.Activities
{
    [Activity(Label = "QueueDetailActivity", NoHistory = true)]
    public class QueueDetailActivity : BaseActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.QueueDetail);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");
            var queueName = await BlobCache.LocalMachine.GetObject<string>("selectedQueue");

            var accountLabel = FindViewById<TextView>(Resource.Id.account_label);
            accountLabel.Text = account.Name;

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            var queueClient = storageAccount.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference(queueName);

            var messagesListView = FindViewById<ListView>(Resource.Id.messages);

            var messages = await queue.PeekMessagesAsync(32);

            messagesListView.Adapter = new ArrayAdapter<string>(this,
                                                                Android.Resource.Layout.SimpleListItem1,
                                                                messages.Select(x => x.AsString).ToArray());
        }
    }
}
