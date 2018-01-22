using System;
using Android.App;
using Android.Views;
using Android.Widget;

namespace AzureStorageBrowser
{
    public class AccountsListAdapter : BaseAdapter<Account>
    {
        private readonly Activity _activity;
        private readonly Account[] _accounts;

        public AccountsListAdapter(Activity activity, Account[] accounts)
        {
            _activity = activity;
            _accounts = accounts;
        }

        public override Account this[int position] => _accounts[position];

        public override int Count => _accounts.Length;

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            if (view == null)
            {
                view = _activity.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItemSingleChoice, null);
            }

            var account = _accounts[position];

            TextView text1 = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            text1.Text = account.Name;

            return view;
        }
    }
}
