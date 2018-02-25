using System;
using System.Threading.Tasks;

namespace AzureStorageBrowser
{
    public class Account
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
    }

    public class Subscription
    {
        public string Id { get; set; }
        public Account[] Accounts { get; set; }
        public string Name { get; set; }
    }
}
