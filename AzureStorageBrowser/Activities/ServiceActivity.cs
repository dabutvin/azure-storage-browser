using System;
using System.Reactive.Linq;
using Akavache;
using Android.App;
using Android.Widget;

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

            SetContentView(Resource.Layout.Service);

            var account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");
            Title = $"{account.Name} Services";

            blobButton = FindViewById<Button>(Resource.Id.goto_blobs);
            queueButton = FindViewById<Button>(Resource.Id.goto_queues);
            tableButton = FindViewById<Button>(Resource.Id.goto_tables);

            blobButton.Click += delegate
            {
                StartActivity(typeof(BlobActivity));
            };

            queueButton.Click += delegate
            {
                StartActivity(typeof(QueueActivity));
            };

            tableButton.Click += delegate
            {
                StartActivity(typeof(TableActivity));
            };
        }
    }
}
