using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.IO;

namespace SpyTools
{
    public class LibraryFragment : ListFragment
    {
        private FileListAdapter _adapter;
        private DirectoryInfo _directory;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _adapter = new FileListAdapter(Activity, new FileSystemInfo[0]);
            ListAdapter = _adapter;
        }

        public override void OnResume()
        {
            base.OnResume();
            RefreshFilesList(MediaService.GetToolsFolder());
        }

        public void RefreshFilesList(string directory)
        {
            IList<FileSystemInfo> visibleThings = new List<FileSystemInfo>();
            var dir = new DirectoryInfo(directory);
            try
            {
                foreach (var item in dir.GetFileSystemInfos().Where(item => item.IsVisible()))
                {
                    visibleThings.Add(item);
                }
            }
            catch (Exception ex)
            {
                Log.Error("FileListFragment", "Couldn't access the directory " + _directory.FullName + "; " + ex);
                Toast.MakeText(Activity, "Problem retrieving contents of " + directory, ToastLength.Long).Show();
                return;
            }
            _directory = dir;
            // Empty out the adapter and add in the FileSystemInfo objects
            // for the current directory.
            _adapter.Clear();
            _adapter.AddDirectoryContents(visibleThings);
            // If we don't do this, then the ListView will not update itself when then data set
            // in the adapter changes. It will appear to the user that nothing has happened.
            ListView.RefreshDrawableState();
            Log.Verbose("FileListFragment", "Displaying the contents of directory {0}.", directory);
        }

        public override void OnListItemClick(ListView l, View v, int position, long id)
        {
            var fileSystemInfo = _adapter.GetItem(position);
            if (fileSystemInfo.IsFile())
            {
                // Do something with the file.  In this case we just pop some toast.
                Log.Verbose("FileListFragment", "The file {0} was clicked.", fileSystemInfo.FullName);
                Toast.MakeText(Activity, "You selected file " + fileSystemInfo.FullName, ToastLength.Short).Show();

                OpenFile(fileSystemInfo.FullName);
            }
            base.OnListItemClick(l, v, position, id);
        }

        private void OpenFile(string path)
        {
            var fileToPlay = new Java.IO.File(path);
            var intent = new Intent();
            intent.SetDataAndType(Android.Net.Uri.FromFile(fileToPlay), "*/*");
            StartActivity(intent);
        }
    }
}