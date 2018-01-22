using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureStorageBrowser
{
    public static class AuthToken
    {
        private static AuthenticationResult authResult = null;

        public static async Task<string> GetTokenAsync(this Activity activity)
        {
            var authContext = new AuthenticationContext("https://login.windows.net/common");
            if (authContext.TokenCache.ReadItems().Any())
            {
                authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);
            }

            authResult = await authContext.AcquireTokenAsync(
                resource: "https://graph.windows.net",
                clientId: "a8c2e660-92bf-4905-89bf-2b8fbc685186",
                redirectUri: new Uri("https://azurestoragebrowser.com"),
                parameters: new PlatformParameters(activity));

            return authResult?.AccessToken;
        }
    }
}
