using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace AzureStorageBrowser
{
    [Activity(Label = "Azure Storage Browser", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : BaseActivity
    {
        Button loginButton;
        Button blobButton;
        Button queueButton;
        Button tableButton;

        ListView accountsListView;

        Account[] accounts;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

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
                    await RefreshAccounts(token);

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

            accountsListView.Adapter = new AccountsListAdapter(this, accounts);
        }

        private Account SelectedAccount()
        {
            if (accountsListView.CheckedItemPosition > -1)
            {
                return accounts[accountsListView.CheckedItemPosition];
            }

            return null;
        }

        private async Task RefreshAccounts(string token)
        {
            accounts = await Task.FromResult(new[]
            {
                new Account
                {
                    Name = "abc",
                },
                new Account
                {
                    Name = "def",
                },
            });
        }
    }
}
