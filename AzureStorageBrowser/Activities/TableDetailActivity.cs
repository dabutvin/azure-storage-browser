using System;
using System.Reactive.Linq;
using Akavache;
using Android.App;
using Android.OS;
using Android.Widget;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorageBrowser.Activities
{
    [Activity(Label = "TableDetailActivity")]
    public class TableDetailActivity : BaseActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.TableDetail);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");
            var tableName = await BlobCache.LocalMachine.GetObject<string>("selectedTable");

            var accountLabel = FindViewById<TextView>(Resource.Id.account_label);
            accountLabel.Text = account.Name;

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            var tableClient = storageAccount.CreateCloudTableClient();

            var table = tableClient.GetTableReference(tableName);

            var tableLayout = FindViewById<TableLayout>(Resource.Id.tablelayout);

            var header = new TableRow(this);

            var partitionKey = new TextView(this);
            partitionKey.Text = "PARTITION KEY";

            var rowKey = new TextView(this);
            rowKey.Text = "ROW KEY";

            header.AddView(partitionKey);
            header.AddView(rowKey);

            tableLayout.AddView(header);

            var rows = await table.ExecuteQuerySegmentedAsync(new TableQuery(), null);
            foreach (var row in rows)
            {
                var tableRow = new TableRow(this);

                var partitionKeyValue = new TextView(this);
                partitionKeyValue.Text = row.PartitionKey;

                var rowKeyValue = new TextView(this);
                rowKeyValue.Text = row.RowKey;

                tableRow.AddView(partitionKeyValue);
                tableRow.AddView(rowKeyValue);

                tableLayout.AddView(tableRow);
            }
        }
    }
}
