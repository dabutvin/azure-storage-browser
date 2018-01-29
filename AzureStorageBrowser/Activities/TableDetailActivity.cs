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
    [Activity]
    public class TableDetailActivity : BaseActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            SetContentView(Resource.Layout.TableDetail);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");
            var tableName = await BlobCache.LocalMachine.GetObject<string>("selectedTable");

            Title = $"{account.Name} > {tableName}";

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            var tableClient = storageAccount.CreateCloudTableClient();
            tableClient.DefaultRequestOptions.RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry();

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
