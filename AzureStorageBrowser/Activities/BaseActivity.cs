using System;
using System.Threading.Tasks;
using Android.App;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureStorageBrowser.Activities
{
    public class BaseActivity : Activity
    {
        public BaseActivity()
        {
        }

        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.Settings, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
        {
            Task.Run(async () => { await AuthToken.LogoutAsync(); }).Wait();

            // restart
            Finish();
            StartActivity(typeof(MainActivity));

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode, data);
        }
    }
}
