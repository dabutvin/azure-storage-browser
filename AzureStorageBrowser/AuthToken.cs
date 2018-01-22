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
                resource: "https://management.azure.com/",
                clientId: "a8c2e660-92bf-4905-89bf-2b8fbc685186",
                redirectUri: new Uri("https://azurestoragebrowser.com"),
                parameters: new PlatformParameters(activity));

            return authResult?.AccessToken;
        }

        /*
         * 
         * 
         *   "requiredResourceAccess": [
                {
                  "resourceAppId": "797f4846-ba00-4fd7-ba43-dac1f8f63013",
                  "resourceAccess": [
                    {
                      "id": "41094075-9dad-400e-a0bd-54e686782033",
                      "type": "Scope"
                    }
                  ]
                },
                {
                  "resourceAppId": "00000002-0000-0000-c000-000000000000",
                  "resourceAccess": [
                    {
                      "id": "311a71cc-e848-46a1-bdf8-97ff7156d8e6",
                      "type": "Scope"
                    }
                  ]
                }
              ],
         *
         */

    }
}
