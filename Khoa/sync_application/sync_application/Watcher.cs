using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace sync_application
{
    class Watcher
    {
        const int minute = 0;
        const int second = 10;
        public string location = "";
        FileSystemWatcher watcher;
        Boolean let = false;
        DispatcherTimer dispatcherTimer;

        List<EditingFile> listEditingFile = new List<EditingFile>();

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void start()
        {
            if (location == "")
            {
                MessageBox.Show("Select location");
                return;
            }

            TimerInitialize();

            watcher = new FileSystemWatcher();
            watcher.Path = Global.location;

            watcher.NotifyFilter = NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            watcher.Filter = "*.*";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Deleted += new FileSystemEventHandler(OnDeleted);
            watcher.Renamed += new RenamedEventHandler(OnRename);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
            dispatcherTimer.Start();
        }

        private void TimerInitialize()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, minute, second);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) // use to check file is still edit
        {
            try
            {
                foreach (EditingFile edt in listEditingFile)
                {
                    string newMD5 = EditingFile.getMD5Hash(edt.filepath);

                    // Different => Edited
                    if (newMD5.CompareTo(edt.md5_hash) != 0)
                    {
                        Global._upload.FileName.filename = edt.filepath;
                        Global._upload.FileName.mode = "UPLOAD";
                        Global._upload.FileName.status = "PROCESSING";
                        Global._upload.Do_Upload();

                        listEditingFile.Remove(edt);
                    }
                }
            }
            catch
            {

            }
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Deleted: " + e.Name);
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("created: " + e.Name);
            Console.WriteLine("created: " + e.FullPath);
        }

        private void OnRename(object sender, RenamedEventArgs e)
        {
            Console.WriteLine("Rename: " + e.Name);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Change: " + e.Name);
            if (let == false)
            {
                FileAttributes attr = File.GetAttributes(e.FullPath);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    Console.WriteLine(e.FullPath + " is a DIRECTORY");
                else
                {
                    Console.WriteLine(e.FullPath + " is a FILE");
                    listEditingFile.Add(new EditingFile(e.FullPath, EditingFile.getMD5Hash(e.FullPath)));
                    Console.WriteLine(listEditingFile.ElementAt(listEditingFile.Count - 1).md5_hash);
                }
                let = true;
            }
            else
                let = false;
        }
    
    }

    class EditingFile
    {
        public string filepath;
        public string md5_hash;

        public EditingFile(string path, string md5)
        {
            filepath = path;
            md5_hash = md5;
        }

        public static string getMD5Hash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] buffer = new byte[stream.Length];
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }
    }
}