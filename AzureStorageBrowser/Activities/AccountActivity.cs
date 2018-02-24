using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using Android.App;
using Android.Widget;
using static Android.Widget.AdapterView;

namespace AzureStorageBrowser.Activities
{
    [Activity(Label = "Accounts")]
    public class AccountActivity : BaseActivity
    {
        ListView accountsListView;
        ProgressBar progressBar;

        protected override async void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Account);

            accountsListView = FindViewById<ListView>(Resource.Id.accounts);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progress);

            try
            {
                var cachedAccounts = await BlobCache.LocalMachine.GetObject<Account[]>("accounts");
                if (cachedAccounts != null)
                {
                    accountsListView.Adapter = new AccountsListAdapter(this, cachedAccounts);
                }
            }
            catch (KeyNotFoundException)
            {
                progressBar.Visibility = Android.Views.ViewStates.Visible;
            }

            accountsListView.ItemClick += async delegate(object sender, ItemClickEventArgs e)
            {
                var cachedAccounts = await BlobCache.LocalMachine.GetObject<Account[]>("accounts");

                await BlobCache.LocalMachine.InsertObject(
                    "selectedAccount",
                    cachedAccounts[e.Position]);

                StartActivity(typeof(ServiceActivity));
            };

            var token = await BlobCache.LocalMachine.GetObject<string>("token");
            var accounts = await FetchAccounts(token);

            await BlobCache.LocalMachine.InsertObject("accounts", accounts);

            progressBar.Visibility = Android.Views.ViewStates.Gone;
            accountsListView.Adapter = new AccountsListAdapter(this, accounts);
        }

        private async Task<Account[]> FetchAccounts(string token)
        {
            var httpClient = new HttpClient();

            var subscriptions = await httpClient.GetSubscriptions(token);

            var accounts = new List<Account>();

            foreach (var subscription in subscriptions)
            {
                var resources = await httpClient.GetStorageResources(token, subscription.SubscriptionId);

                accounts.AddRange(await resources.ForEachAsync(async resource =>
                {
                    var key = await httpClient.GetStorageKey(token, resource.Id);
                    return new Account
                    {
                        Name = resource.Name,
                        Id = resource.Id,
                        Key = key,
                    };
                }));
            }

            return accounts.ToArray();
        }
    }
}
