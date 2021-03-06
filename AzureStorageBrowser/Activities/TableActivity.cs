﻿using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using Android.App;
using Android.OS;
using Android.Widget;
using Microsoft.AppCenter.Analytics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorageBrowser.Activities
{
    [Activity]
    public class TableActivity : BaseActivity
    {
        CloudTableClient tableClient;
        ListView tablesListView;
        ProgressBar progressBar;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            SetContentView(Resource.Layout.Table);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");

            Title = $"{account.Name} > tables";

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={account.Name};AccountKey={account.Key}");

            tableClient = storageAccount.CreateCloudTableClient();
            tableClient.DefaultRequestOptions.RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry();

            tablesListView = FindViewById<ListView>(Resource.Id.tables);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progress);

            BindTableClick($"{account.Id}/tables");
            await BindTablesAsync($"{account.Id}/tables");
            await RefreshTablesAsync($"{account.Id}/tables");
            await BindTablesAsync($"{account.Id}/tables");
        }

        private void BindTableClick(string id)
        {
            tablesListView.ItemClick += async delegate (object sender, AdapterView.ItemClickEventArgs e)
            {
                if (e.Position > -1)
                {
                    Analytics.TrackEvent("table-table-clicked");
                    var tables = await BlobCache.LocalMachine.GetObject<string[]>(id);
                    await BlobCache.LocalMachine.InsertObject("selectedTable", tables[e.Position]);
                    StartActivity(typeof(TableDetailActivity));
                }
            };
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

            Analytics.TrackEvent(
                "table-tables-fetched",
                new Dictionary<string, string> { ["count"] = tables.Count().ToString() });

            await BlobCache.LocalMachine.InsertObject(id, tables.ToArray());
            progressBar.Visibility = Android.Views.ViewStates.Gone;

            if (tables.Any() == false)
            {
                var emptyMessage = FindViewById<TextView>(Resource.Id.empty);
                emptyMessage.Visibility = Android.Views.ViewStates.Visible;
            }
        }

        private async Task BindTablesAsync(string id)
        {
            try
            {
                var tables = await BlobCache.LocalMachine.GetObject<string[]>(id);

                if (tables != null)
                {
                    tablesListView.Adapter = new ArrayAdapter<string>(
                        this,
                        Android.Resource.Layout.SimpleListItem1,
                        tables.ToArray());
                }
            }
            catch (KeyNotFoundException)
            {
                progressBar.Visibility = Android.Views.ViewStates.Visible;
            }
        }
    }
}
