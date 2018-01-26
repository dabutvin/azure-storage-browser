
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
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorageBrowser.Activities
{
    [Activity]
    public class QueueActivity : BaseActivity
    {
        CloudQueueClient queueClient;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Queue);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");

            Title = $"{account.Name} > queues";

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            queueClient = storageAccount.CreateCloudQueueClient();

            await BindQueuesAsync($"{account.Id}/queues");
            await RefreshQueuesAsync($"{account.Id}/queues");
            await BindQueuesAsync($"{account.Id}/queues");

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

            await BlobCache.LocalMachine.InsertObject(id, queues.ToArray());
        }

        private async Task BindQueuesAsync(string id)
        {
            try
            {
                var queues = await BlobCache.LocalMachine.GetObject<string[]>(id);

                if (queues != null)
                {
                    var queuesListView = FindViewById<ListView>(Resource.Id.queues);

                    queuesListView.Adapter = new ArrayAdapter<string>(
                        this,
                        Android.Resource.Layout.SimpleListItem1,
                        queues.ToArray());

                    queuesListView.ItemClick += async delegate (object sender, AdapterView.ItemClickEventArgs e)
                    {
                        if (e.Position > -1)
                        {
                            await BlobCache.LocalMachine.InsertObject("selectedQueue", queues[e.Position]);
                            StartActivity(typeof(QueueDetailActivity));
                        }
                    };
                }
            }
            catch (KeyNotFoundException) { }
        }
    }
}
