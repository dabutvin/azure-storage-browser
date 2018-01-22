
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
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorageBrowser.Activities
{
    [Activity(Label = "TableActivity")]
    public class TableActivity : BaseActivity
    {
        CloudTableClient tableClient;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Table);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");

            var accountLabel = FindViewById<TextView>(Resource.Id.account_label);
            accountLabel.Text = account.Name;

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            tableClient = storageAccount.CreateCloudTableClient();

            await BindTablesAsync($"{account.Id}/tables");
            await RefreshTablesAsync($"{account.Id}/tables");
            await BindTablesAsync($"{account.Id}/tables");

        }

        private async Task RefreshTablesAsync(string id)
        {
            var tables = new List<string>();
            TableContinuationToken continuationToken = null;
            do
            {
                var tablesSegment = await tableClient.ListTablesSegmentedAsync(continuationToken);

                tables.AddRange(tablesSegment.Results.Select(x => x.Name));
                continuationToken = tablesSegment.ContinuationToken;

            } while (continuationToken != null);

            await BlobCache.LocalMachine.InsertObject(id, tables.ToArray());
        }

        private async Task BindTablesAsync(string id)
        {
            try
            {
                var tables = await BlobCache.LocalMachine.GetObject<string[]>(id);

                if (tables != null)
                {
                    var tablesListView = FindViewById<ListView>(Resource.Id.tables);

                    tablesListView.Adapter = new ArrayAdapter<string>(
                        this,
                        Android.Resource.Layout.SimpleListItem1,
                        tables.ToArray());
                }
            }
            catch (KeyNotFoundException) { }
        }
    }
}
