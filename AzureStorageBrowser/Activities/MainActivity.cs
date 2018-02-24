using System.Collections.Generic;
using System.Reactive.Linq;
using Akavache;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace AzureStorageBrowser.Activities
{
    [Activity(Label = "Azure Storage Browser", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : BaseActivity
    {
        Button loginButton;
        ImageView homeImage;
        TextView homeTitle;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            BlobCache.ApplicationName = nameof(AzureStorageBrowser);

            AppCenter.Start("3c0813dd-30a8-4215-987d-68fc759340e0", typeof(Analytics), typeof(Crashes));

            loginButton = FindViewById<Button>(Resource.Id.login);
            homeImage = FindViewById<ImageView>(Resource.Id.homeimage);
            homeTitle = FindViewById<TextView>(Resource.Id.hometitle);

            try
            {
                var loggedInUser = await BlobCache.LocalMachine.GetObject<string>("loggedInUser");
                if (!string.IsNullOrEmpty(loggedInUser))
                {
                    loginButton.Text = $"Continue as {loggedInUser}";
                }
            }
            catch(KeyNotFoundException){}
            finally{ loginButton.Visibility = ViewStates.Visible; }

            loginButton.Click += async delegate
            {
                homeImage.Visibility = ViewStates.Gone;
                loginButton.Visibility = ViewStates.Gone;
                homeTitle.Visibility = ViewStates.Gone;

                var token = await this.GetTokenAsync();

                if (token == null)
                {
                    // Reset to try to get a token again
                    Finish();
                    StartActivity(Intent);
                }
                else
                {
                    await BlobCache.LocalMachine.InsertObject<string>("token", token);
                    StartActivity(typeof(AccountActivity));
                }
            };
        }
    }
}
