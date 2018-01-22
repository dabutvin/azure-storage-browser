using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureStorageBrowser
{
    public static class AzureApi
    {
        public static async Task<AzureSubscription[]> GetSubscriptions(this HttpClient httpClient, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var subscriptionsJson = await httpClient.GetStringAsync(
                "https://management.azure.com/subscriptions?api-version=2015-01-01");

            var subscriptions = JsonConvert.DeserializeObject<SusbcriptionContract>(subscriptionsJson);

            return subscriptions.Value;
        }

        public static async Task<AzureResource[]> GetAzureResources(this HttpClient httpClient, string token, string subscriptionId)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resourcesJson = await httpClient.GetStringAsync(
                    $"https://management.azure.com/subscriptions/{subscriptionId}/resources?api-version=2017-05-10&$filter=resourceType eq 'Microsoft.Storage/storageAccounts'");

            var resources = JsonConvert.DeserializeObject<ResourceContract>(resourcesJson);

            return resources.Value;
        }
    }

    public class SusbcriptionContract
    {
        public AzureSubscription[] Value { get; set; }
    }

    public class AzureSubscription
    {
        public string Id { get; set; }
        public string SubscriptionId { get; set; }
    }

    public class ResourceContract
    {
        public AzureResource[] Value { get; set; }
    }

    public class AzureResource
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
