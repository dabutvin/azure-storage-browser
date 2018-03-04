using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using Android.App;
using Android.Views;
using Android.Widget;
using AzureStorageBrowser.Activities;

namespace AzureStorageBrowser
{
    public class SubscriptionsListAdapter : BaseExpandableListAdapter
    {
        private readonly Activity _activity;
        private readonly Subscription[] _subscriptions;

        public SubscriptionsListAdapter(Activity activity, Subscription[] subscriptions)
        {
            _activity = activity;
            _subscriptions = subscriptions;
        }

        public override int GroupCount => _subscriptions.Length;
        public override bool HasStableIds => true;
        public override long GetChildId(int groupPosition, int childPosition) => childPosition;
        public override int GetChildrenCount(int groupPosition) => _subscriptions[groupPosition].Accounts.Length;
        public override long GetGroupId(int groupPosition) => groupPosition;

        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
            {
                view = _activity.LayoutInflater.Inflate(Resource.Layout.SubscriptionListViewItem, null);
            }

            var subscription = _subscriptions[groupPosition];

            view.FindViewById<TextView>(Resource.Id.subscriptionname).Text = subscription.Name;

            return view;
        }

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
            {
                view = _activity.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
            }

            var account = _subscriptions[groupPosition].Accounts[childPosition];

            view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = account.Name;

            return view;
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            if(_subscriptions[groupPosition].Accounts[childPosition].Id == "empty")
            {
                return false;    
            }

            return true;   
        }


        public override Java.Lang.Object GetGroup(int groupPosition)
        {
            throw new NotImplementedException();
        }

        public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
        {
            throw new NotImplementedException();
        }
    }
}
