using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
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

        public static async Task<AzureResource[]> GetStorageResources(this HttpClient httpClient, string token, string subscriptionId)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resourcesJson = await httpClient.GetStringAsync(
                    $"https://management.azure.com/subscriptions/{subscriptionId}/resources?api-version=2017-05-10&$filter=resourceType eq 'Microsoft.Storage/storageAccounts'");

            var resources = JsonConvert.DeserializeObject<ResourceContract>(resourcesJson);

            return resources.Value;
        }

        public static async Task<string> GetStorageKey(this HttpClient httpClient, string token, string id)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var keysResponse = await httpClient.PostAsync(
                    $"https://management.azure.com/{id}/listKeys?api-version=2017-06-01",
                    new StringContent(""));

                var keysJson = await keysResponse.Content.ReadAsStringAsync();

                var keys = JsonConvert.DeserializeObject<StorageKeyContract>(keysJson);

                return keys.Keys?.FirstOrDefault()?.Value;
            }
            catch(Exception ex)
            {
                Crashes.TrackError(ex);
                return null;
            }
        }
    }

    public class SusbcriptionContract
    {
        public AzureSubscription[] Value { get; set; }
    }

    public class AzureSubscription
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
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

    public class StorageKeyContract
    {
        public StorageKey[] Keys { get; set; }
    }

    public class StorageKey
    {
        public string Value { get; set; }
    }
}
