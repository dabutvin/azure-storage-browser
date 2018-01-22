
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Akavache;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AzureStorageBrowser.Activities
{
    [Activity(Label = "BlobActivity")]
    public class BlobActivity : BaseActivity
    {
        Account account;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Blob);

            account = await BlobCache.LocalMachine.GetObject<Account>("selectedAccount");

            var accountLabel = FindViewById<TextView>(Resource.Id.account_label);
            accountLabel.Text = account.Name;
        }
    }
}
