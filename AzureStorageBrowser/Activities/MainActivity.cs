﻿using System;
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
        TextView loggedInAsText;
        ProgressBar progressBar;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            BlobCache.ApplicationName = nameof(AzureStorageBrowser);

            AppCenter.Start("3c0813dd-30a8-4215-987d-68fc759340e0", typeof(Analytics), typeof(Crashes));

            var splash = FindViewById<ImageView>(Resource.Id.splash);

            try
            {
                splash.SetImageResource(Resource.Drawable.azure_storage_browser1);
            }
            catch(Exception e)
            {
                Crashes.TrackError(e);
            }

            loginButton = FindViewById<Button>(Resource.Id.login);
            loggedInAsText = FindViewById<TextView>(Resource.Id.loggedinas);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progress);

            try
            {
                var loggedInUser = await BlobCache.LocalMachine.GetObject<string>("loggedInUser");
                if (!string.IsNullOrEmpty(loggedInUser))
                {
                    loginButton.Text = $"Continue";
                    loggedInAsText.Text = $"Logged in as {loggedInUser}";
                    loggedInAsText.Visibility = ViewStates.Visible;
                }
            }
            catch(KeyNotFoundException){}
            finally{ loginButton.Visibility = ViewStates.Visible; }

            loginButton.Click += async delegate
            {
                progressBar.Visibility = ViewStates.Visible;
                Analytics.TrackEvent("main-login-clicked");

                var token = await this.GetTokenAsync();

                progressBar.Visibility = ViewStates.Invisible;

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
