using System;
using System.Threading.Tasks;
using Android.App;
using Microsoft.AppCenter.Analytics;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureStorageBrowser.Activities
{
    public class BaseActivity : Activity
    {
        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.Settings, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    break;
                case Resource.Id.logout:
                    Analytics.TrackEvent("global-clicked-logout");
                    Task.Run(async () => { await AuthToken.LogoutAsync(); }).Wait();
                    Finish();
                    StartActivity(typeof(MainActivity));
                    break;
                default:
                    return base.OnOptionsItemSelected(item);

            }

            return true;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode, data);
        }
    }
}
