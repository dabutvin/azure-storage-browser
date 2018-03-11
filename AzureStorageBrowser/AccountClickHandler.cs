using System;
using System.Reactive.Linq;
using Akavache;
using Android.Views;
using Android.Widget;
using AzureStorageBrowser.Activities;
using Microsoft.AppCenter.Analytics;
using static Android.Widget.ExpandableListView;

namespace AzureStorageBrowser
{
    public class AccountClickHandler : Java.Lang.Object, IOnChildClickListener
    {
        public bool OnChildClick(ExpandableListView parent, View clickedView, int groupPosition, int childPosition, long id)
        {
            Analytics.TrackEvent("account-account-clicked");

            BlobCache.LocalMachine.GetObject<Subscription[]>("subscriptions")
                     .Subscribe(subscriptions =>
            {
                var account = subscriptions[groupPosition].Accounts[childPosition];
                BlobCache.LocalMachine.InsertObject("selectedAccount", account);
                parent.Context.StartActivity(typeof(ServiceActivity));
            });

            return true;
        }
    }
}
