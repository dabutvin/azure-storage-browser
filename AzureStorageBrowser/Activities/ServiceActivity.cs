using System;
using System.Reactive.Linq;
using Akavache;
using Android.App;
using Android.Widget;
using Microsoft.AppCenter.Analytics;

namespace AzureStorageBrowser.Activities
{
    [Activity]
    public class ServiceActivity : BaseActivity
    {
        Button blobButton;
        Button queueButton;
        Button tableButton;

        protected override async void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            SetContentView(Resource.Layout.Service);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");
            Title = $"{account.Name}";

            blobButton = FindViewById<Button>(Resource.Id.goto_blobs);
            queueButton = FindViewById<Button>(Resource.Id.goto_queues);
            tableButton = FindViewById<Button>(Resource.Id.goto_tables);

            blobButton.Click += delegate
            {
                Analytics.TrackEvent("service-blob-clicked");
                StartActivity(typeof(BlobActivity));
            };

            queueButton.Click += delegate
            {
                Analytics.TrackEvent("service-queue-clicked");
                StartActivity(typeof(QueueActivity));
            };

            tableButton.Click += delegate
            {
                Analytics.TrackEvent("service-table-clicked");
                StartActivity(typeof(TableActivity));
            };
        }
    }
}
