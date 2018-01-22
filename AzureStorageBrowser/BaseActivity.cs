using System;
using Android.App;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureStorageBrowser
{
    public class BaseActivity : Activity
    {
        public BaseActivity()
        {
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode, data);
        }
    }
}
