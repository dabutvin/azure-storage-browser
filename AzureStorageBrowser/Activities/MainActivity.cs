using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using Newtonsoft.Json;
using Android.Content;
using Akavache;

namespace AzureStorageBrowser.Activities
{
    [Activity(Label = "Azure Storage Browser", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : BaseActivity
    {
        Button loginButton;
        Button blobButton;
        Button queueButton;
        Button tableButton;

        ListView accountsListView;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            BlobCache.ApplicationName = nameof(AzureStorageBrowser);

            loginButton = FindViewById<Button>(Resource.Id.login);
            blobButton = FindViewById<Button>(Resource.Id.goto_blobs);
            queueButton = FindViewById<Button>(Resource.Id.goto_queues);
            tableButton = FindViewById<Button>(Resource.Id.goto_tables);

            accountsListView = FindViewById<ListView>(Resource.Id.accounts);

            loginButton.Click += async delegate
            {
                var token = await this.GetTokenAsync();

                if(token != null)
                {
                    loginButton.Visibility = ViewStates.Gone;
                    blobButton.Visibility = ViewStates.Visible;
                    tableButton.Visibility = ViewStates.Visible;
                    queueButton.Visibility = ViewStates.Visible;

                    try
                    {
                        var cachedAccounts = await BlobCache.LocalMachine.GetObject<Account[]>("accounts");
                        if (cachedAccounts != null)
                        {
                            accountsListView.Adapter = new AccountsListAdapter(this, cachedAccounts);
                        }
                    }
                    catch (KeyNotFoundException) { }

                    var accounts = await FetchAccounts(token);

                    await BlobCache.LocalMachine.InsertObject("accounts", accounts);

                    accountsListView.Adapter = new AccountsListAdapter(this, accounts);
                }
            };

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

            accountsListView.ItemClick += async delegate
            {
                var cachedAccounts = await BlobCache.LocalMachine.GetObject<Account[]>("accounts");

                await BlobCache.LocalMachine.InsertObject(
                    "selectedAccount",
                    cachedAccounts[accountsListView.CheckedItemPosition]);
            };
        }

        private async Task<Account[]> FetchAccounts(string token)
        {
            var httpClient = new HttpClient();

            var subscriptions = await httpClient.GetSubscriptions(token);

            var resources = new List<AzureResource>();

            foreach (var subscription in subscriptions)
            {
                resources.AddRange(await httpClient.GetAzureResources(token, subscription.SubscriptionId));
            }

            return resources
                .Select(x => new Account { Name = x.Name, Id = x.Id })
                .ToArray();
        }
    }
}
