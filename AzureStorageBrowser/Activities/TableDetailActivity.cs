using System;
using System.Linq;
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
        ProgressBar progressBar;
        TableLayout tableLayout;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            SetContentView(Resource.Layout.TableDetail);

            progressBar = FindViewById<ProgressBar>(Resource.Id.progress);
            tableLayout = FindViewById<TableLayout>(Resource.Id.tablelayout);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");
            var tableName = await BlobCache.LocalMachine.GetObject<string>("selectedTable");

            Title = $"{account.Name} > {tableName}";

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            var tableClient = storageAccount.CreateCloudTableClient();
            tableClient.DefaultRequestOptions.RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry();

            var table = tableClient.GetTableReference(tableName);

            var rows = await table.ExecuteQuerySegmentedAsync(new TableQuery(), null);

            progressBar.Visibility = Android.Views.ViewStates.Gone;

            if (rows.Any() == false)
            {
                var emptyMessage = FindViewById<TextView>(Resource.Id.empty);
                emptyMessage.Visibility = Android.Views.ViewStates.Visible;
            }
            else
            {
                var header = new TableRow(this);

                var partitionKey = new TextView(this);
                partitionKey.Text = "PARTITION KEY";

                var rowKey = new TextView(this);
                rowKey.Text = "ROW KEY";

                header.AddView(partitionKey);
                header.AddView(rowKey);

                tableLayout.AddView(header);

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
}
