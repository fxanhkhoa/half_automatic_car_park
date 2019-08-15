using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
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
        public static string drive;
        int isJson = 0;

        List<EditingFile> listEditingFile = new List<EditingFile>();

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void start()
        {
            Global.mw.Dispatcher.Invoke(new Action(() => Global.mw.appendReportText("== Starting Watcher ==")));
            if (location == "")
            {
                MessageBox.Show("Select location");
                return;
            }

            try
            {
                TimerInitialize();

                watcher = new FileSystemWatcher();
                watcher.Path = location;

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
                //dispatcherTimer.Start();

                Thread checkThr = new Thread(new ThreadStart(checkAllFile));
                checkThr.IsBackground = true;
                checkThr.Start();

            }
            catch (Exception e){
                Global.mw.appendReportText("== Starting Watcher Fail {0} ==", e);
            }
        }

        private void TimerInitialize()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, minute, second);
        }

        public void ProcessData(byte[] data, int bytesRead)
        {
            try
            {
                Console.WriteLine("===== GO TO WATCHER PROCESS DATA =====");
                byte[] true_data = new Byte[bytesRead];
                Array.Copy(data, 0, true_data, 0, bytesRead);
                string json = Encoding.UTF8.GetString(true_data);
                Console.WriteLine("====== WATCHER PROCESSING DATA ====== " + json);
                FileObject new_file = JsonConvert.DeserializeObject<FileObject>(json);
                isJson = 1;

                if (new_file.mode == "CHECKMD5")
                {
                    if (new_file.status == "DIFF")
                    {
                        Global._upload.FileName.filename = new_file.filename; // Set only file name because status and mode 
                                                                                              //is set in function
                        Global._upload.Do_Upload();

                        // Signal done
                        upload.isDone = 0;
                        //while (upload.isDone == 0);
                        Console.WriteLine("============= START UPLOAD =============== " + Global._upload.FileName.filename);
                    }
                    else if (new_file.status == "SAME")
                    {
                        Console.WriteLine("============= SAME MD5 =============== ");
                        upload.isDone = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("======== Is Not Json In Watcher {0} =========", ex);
                isJson = 0;
            }
        }

        private void checkAllFile()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        FileStream fs;
                        List<FileInfoJson> listfileInfo = new List<FileInfoJson>();

                        // Create file
                        if (!File.Exists(Global.location + "/FileTable.json"))
                        {
                            fs = new FileStream(Global.location + "/FileTable.json", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                            fs.Close();
                        }

                        //// Open File
                        //fs = new FileStream("FileTable.json", FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                        //fs.Write(new byte[1], 0, 0);
                        //fs.Close();

                        // Append file
                        //fs = new FileStream("FileTable.json", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        string[] allfiles = Directory.GetFiles(location, "*.*", SearchOption.AllDirectories);
                        //Console.WriteLine("===== WATCHER LOCATION ===== " + Global.location.Substring(Global.location.IndexOf(":") + 2,
                        //                                                    Global.location.Length - 3));
                        //listfileInfo.Add(new FileInfoJson(Global.location.Substring(Global.location.IndexOf(":") + 2,
                        //                                                    Global.location.Length - 3)));

                        /////// For File Table ///////
                        foreach (var file in allfiles)
                        {
                            if (upload.isDone == 0) break;
                            FileInfo info = new FileInfo(file);
                            FileObject fo = new FileObject();
                            Console.WriteLine("====== TIMER ====== : " + file);
                            Console.WriteLine("====== TIMER ====== : " + info.DirectoryName);
                            fo.filename = file;

                            //// Append to List

                            string temp_file_name = file;

                            if (info.Name != "FileTable.json")
                            {
                                listfileInfo.Add(new FileInfoJson(temp_file_name));
                            }

                            //fo.mode = "CHECKMD5";
                            //fo.status = EditingFile.getMD5Hash(info.FullName);

                            //upload.isDone = 0;

                            //string json_to_send = JsonConvert.SerializeObject(fo);
                            //Console.WriteLine("======== WATCHER JSON TO SEND========= " + json_to_send);
                            //Ethernet.SendData(json_to_send);

                            //while (upload.isDone == 0) ;
                        }
                        string json = JsonConvert.SerializeObject(listfileInfo.ToArray());
                        File.WriteAllText(Global.location + "/FileTable.json", json);

                        /////// For All File ///////
                        foreach (var file in allfiles)
                        {
                            if (upload.isDone == 0) break;
                            FileInfo info = new FileInfo(file);
                            FileObject fo = new FileObject();
                            Console.WriteLine("====== TIMER ====== : " + file);
                            Console.WriteLine("====== TIMER ====== : " + info.DirectoryName);
                            fo.filename = file;

                            //// Append to List

                            string temp_file_name = file;

                            if (info.Name != "FileTable.json")
                            {
                                listfileInfo.Add(new FileInfoJson(temp_file_name));
                            }

                            fo.mode = "CHECKMD5";
                            fo.status = EditingFile.getMD5Hash(info.FullName);

                            upload.isDone = 0;

                            string json_to_send = JsonConvert.SerializeObject(fo);
                            Console.WriteLine("======== WATCHER JSON TO SEND========= " + json_to_send);
                            Ethernet.SendData(json_to_send);

                            while (upload.isDone == 0) ;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("===== Watcher Tick Error {0} =====", ex.ToString());
                    }

                    Thread.Sleep((minute * 60 + second) * 1000);
                }
            }
            catch
            {

            }
            
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) // use to check file is still edit
        {
            try
            {
                FileStream fs;
                List<FileInfoJson> listfileInfo = new List<FileInfoJson>();
                
                // Create file
                if (!File.Exists(Global.location + "/FileTable.json"))
                {
                    fs = new FileStream(Global.location + "/FileTable.json", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    fs.Close();
                }

                //// Open File
                //fs = new FileStream("FileTable.json", FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                //fs.Write(new byte[1], 0, 0);
                //fs.Close();

                // Append file
                //fs = new FileStream("FileTable.json", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                string[] allfiles = Directory.GetFiles(location, "*.*", SearchOption.AllDirectories);
                //Console.WriteLine("===== WATCHER LOCATION ===== " + Global.location.Substring(Global.location.IndexOf(":") + 2,
                //                                                    Global.location.Length - 3));
                //listfileInfo.Add(new FileInfoJson(Global.location.Substring(Global.location.IndexOf(":") + 2,
                //                                                    Global.location.Length - 3)));
                foreach (var file in allfiles)
                {
                    if (upload.isDone == 0) break;
                    FileInfo info = new FileInfo(file);
                    FileObject fo = new FileObject();
                    Console.WriteLine("====== TIMER ====== : " + file);
                    Console.WriteLine("====== TIMER ====== : " + info.DirectoryName);
                    fo.filename = file;

                    //// Append to List

                    string temp_file_name = file;

                    if (info.Name != "FileTable.json")
                    {
                        listfileInfo.Add(new FileInfoJson(temp_file_name));
                        string json = JsonConvert.SerializeObject(listfileInfo.ToArray());
                        File.WriteAllText(Global.location + "/FileTable.json", json);
                    }

                    fo.mode = "CHECKMD5";
                    fo.status = EditingFile.getMD5Hash(info.FullName);

                    string json_to_send = JsonConvert.SerializeObject(fo);
                    Console.WriteLine("======== WATCHER JSON TO SEND========= " + json_to_send);
                    Ethernet.SendData(json_to_send);

                    Thread.Sleep(500);
                    upload.isDone = 0;
                    //while (upload.isDone == 0) ;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== Watcher Tick Error {0} =====", ex.ToString());
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
                //FileAttributes attr = File.GetAttributes(e.FullPath);
                //if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                //    Console.WriteLine(e.FullPath + " is a DIRECTORY");
                //else
                //{
                //    Console.WriteLine(e.FullPath + " is a FILE");
                //    listEditingFile.Add(new EditingFile(e.FullPath, EditingFile.getMD5Hash(e.FullPath)));
                //    Console.WriteLine(listEditingFile.ElementAt(listEditingFile.Count - 1).md5_hash);
                //}
                let = true;
            }
            else
                let = false;
        }

        public void processEthernetData()
        {
            Thread thr =  new Thread(new ThreadStart(ProcessThread));
            thr.IsBackground = true;
            thr.Start();            
        }

        public void ProcessThread()
        {
            Global.watcher.ProcessData(Ethernet.buffer, Ethernet._bytesRead);
            Global._upload.process(Ethernet.buffer, Ethernet._bytesRead);
            Global._download.process(Ethernet.buffer, Ethernet._bytesRead);
        }
    }

    class CheckingFile
    {
        public string filepath;
        public EditingFile edtFile;
    }

    class EditingFile
    {
        public string filepath;
        public string md5_hash;
        public DateTime last_time_edited;

        public EditingFile(string path, string md5)
        {
            filepath = path;
            md5_hash = md5;
        }

        public EditingFile(string path, string md5, DateTime lasttime)
        {
            filepath = path;
            md5_hash = md5;
            last_time_edited = lasttime;
        }

        public static string getMD5Hash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] buffer = new byte[stream.Length];
                    string output = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", ""); ;
                    Console.WriteLine("MD5 of " + filename + " : " + output);
                    return output;
                }
            }
        }
    }

    class FileInfoJson
    {
        public string filename;

        public FileInfoJson() { }
        
        public FileInfoJson(string filename)
        {
            this.filename = filename;
        }
    }
}