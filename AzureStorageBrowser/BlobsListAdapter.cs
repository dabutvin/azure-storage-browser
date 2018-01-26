using System;
using Android.App;
using Android.Views;
using Android.Widget;

namespace AzureStorageBrowser
{
    public class BlobsListAdapter : BaseAdapter<Blob>
    {
        private readonly Activity _activity;
        private readonly Blob[] _blobs;

        public BlobsListAdapter(Activity activity, Blob[] blobs)
        {
            _activity = activity;
            _blobs = blobs;
        }

        public override Blob this[int position] => _blobs[position];

        public override int Count => _blobs.Length;

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

            var blob = _blobs[position];

            TextView text1 = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            text1.Text = blob.Name;

            return view;
        }
    }
}
