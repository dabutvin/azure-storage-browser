using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;

namespace AzureStorageBrowser
{
    [Activity(Label = "Azure Storage Browser", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : BaseActivity
    {
        Button loginButton;
        Button blobButton;
        Button queueButton;
        Button tableButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            loginButton = FindViewById<Button>(Resource.Id.login);
            blobButton = FindViewById<Button>(Resource.Id.goto_blobs);
            queueButton = FindViewById<Button>(Resource.Id.goto_queues);
            tableButton = FindViewById<Button>(Resource.Id.goto_tables);

            loginButton.Click += async delegate
            {
                var token = await this.GetTokenAsync();
                if(token != null)
                {
                    loginButton.Visibility = ViewStates.Gone;
                    blobButton.Visibility = ViewStates.Visible;
                    tableButton.Visibility = ViewStates.Visible;
                    queueButton.Visibility = ViewStates.Visible;
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
        }
    }
}
