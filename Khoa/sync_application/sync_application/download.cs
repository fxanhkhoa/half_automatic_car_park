using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sync_application
{
    class download
    {
        private byte isDone;
        private byte isJson;
        private Byte[] data;
        private int bytesRead;
        private int count = 0;
        public static string drive;

        public download()
        {
            FileName = new FileObject();
        }

        public FileObject FileName;

        public void process(Byte[] data, int bytesRead)
        {
            try
            {
                Console.WriteLine("===== GO TO DOWNLOAD ======");
                this.data = new Byte[bytesRead];
                Array.Copy(data, 0, this.data, 0, bytesRead);
                this.bytesRead = bytesRead;
                string json = Encoding.UTF8.GetString(this.data);
                Console.WriteLine(json);
                FileObject new_file = JsonConvert.DeserializeObject<FileObject>(json);
                isJson = 1;

                // Check if getting FileTable.json
                if (new_file.mode == "SYNCBACK")
                {
                    Console.WriteLine("===== MODE SYNC BACK =====");
                    // Create directory and Download FileTable.json
                    Console.WriteLine(Path.GetFileName(new_file.filename));
                    Console.WriteLine(Path.GetDirectoryName(new_file.filename));
                    Console.WriteLine("===== CREATED DIRECTORY {0} =====", 
                                             Path.GetDirectoryName(new_file.filename));
                    Directory.CreateDirectory(
                                             Path.GetDirectoryName(new_file.filename));
                    Global._download.FileName.filename = new_file.filename;
                    Global._download.DoDownload();
                }

                // check if this is the file of own download process
                if (new_file.mode != "DOWNLOAD") return; // Wrong mode
                if (new_file.filename.CompareTo(FileName.filename) != 0) return; // Wrong file of process
                if (new_file.status.CompareTo("DONE") == 0) // Server send ok after receive part of file
                {
                    isDone = 1;
                    Global.syncBack.setDone();
                    if (Path.GetFileName(new_file.filename) == "FileTable.json")
                    {
                        Global.syncBack.DoSyncBack(new_file.filename);
                    }
                    Console.WriteLine("Count = " + count);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Not Json");
                isJson = 0;
            }
        }

        public void DoDownload()
        {
            Console.WriteLine("=== DO DOWNLOAD ===");
            // Create file if do not have
            using (FileStream fs = new FileStream(FileName.filename, FileMode.Create, FileAccess.Write))
            {
                fs.Write(new Byte[1], 0, 0);
            }

            FileName.status = "PROCESSING";
            FileName.mode = "DOWNLOAD";
            string json = JsonConvert.SerializeObject(FileName);
            Ethernet.SendData(json);
            
            // Initialize 
            isJson = 1;

            Thread thr = new Thread(new ThreadStart(DownloadThread));
            thr.IsBackground = true;
            thr.Start();
        }

        private void DownloadThread()
        {
            isDone = 0;
            while (isDone == 0)
            {
                // Byte and now Get part of File
                if (isJson == 0)
                {
                    //count++;
                    int len = Convert.ToInt32(data[bytesRead-1]);
                    //Console.WriteLine("LENGTH: " + len);
                    Byte[] fileNameArray = new Byte[len];
                    Array.Copy(data, bytesRead - len - 1, fileNameArray, 0, len);
                    string fileNameStr = Encoding.UTF8.GetString(fileNameArray);
                    //Console.WriteLine(fileNameStr);

                    Byte[] newData = new Byte[bytesRead - len - 1];
                    Array.Copy(data, 0, newData, 0, bytesRead - len - 1);

                    // Append file
                    using (FileStream fs = new FileStream(fileNameStr, FileMode.Append, FileAccess.Write))
                    {
                        Console.WriteLine("Wrote: " + newData.Length);
                        fs.Write(newData, 0, newData.Length);
                    }

                    FileName.status = "OK";

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    string json = JsonConvert.SerializeObject(FileName);
                    Ethernet.SendData(json);
                    isJson = 1;
                }
            }
        }
    }
}
