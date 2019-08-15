using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace sync_application
{
    class upload
    {
        private byte isJson;
        private byte isReceive;
        public static string drive;
        public static byte isDone = 1;

        public upload()
        {
            FileName = new FileObject();
        }

        public FileObject FileName;

        public void Do_Upload()
        {
            Global.mw.Dispatcher.Invoke(new Action(() => Global.mw.appendReportText("== START UPLOADING {0}==", FileName.filename)));
            //drive = string.Copy(FileName.filename);
            //drive = drive.Substring(0, drive.IndexOf(":") + 2);
            //FileName.filename = FileName.filename.Substring(FileName.filename.IndexOf(":") + 2,
            //                                                        FileName.filename.Length - 3); // Remove *:/
            FileName.mode = "UPLOAD";
            FileName.status = "PROCESSING";
            Console.WriteLine(FileName.filename);

            string json = JsonConvert.SerializeObject(FileName);
            Ethernet.SendData(json);

            Thread thr1 = new Thread(new ThreadStart(UploadThread));
            thr1.IsBackground = true;
            thr1.Start();

            isDone = 0;
        }

        public void process(Byte[] data, int bytesRead)
        {
            Console.WriteLine("==========  UPLOAD PROCESSING ==========");
            Global.mw.Dispatcher.Invoke(new Action(() => Global.mw.appendReportText("== UPLOAD PROCESSING ==")));
            try
            {
                byte[] true_data = new Byte[bytesRead];
                Array.Copy(data, 0, true_data, 0, bytesRead);
                string json = Encoding.UTF8.GetString(true_data);
                Console.WriteLine("========= UPLOAD DATA: " + json + "============");
                FileObject new_file = JsonConvert.DeserializeObject<FileObject>(json);
                isJson = 1;
                // check if this is the file of own upload
                if (new_file.mode != "UPLOAD") return; // Wrong mode
                if (new_file.filename.CompareTo(FileName.filename) != 0) return; // wrong filename pf this process
                if (new_file.status.CompareTo("OK") == 0) // Server send ok after receive part of file
                {
                    isReceive = 1;
                }
                else if (new_file.status.CompareTo("FAIL") == 0)
                {
                    isReceive = 2;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("========== UPLOAD: NOT JSON ==========");
                isJson = 0;
            }
        }

        public void UploadThread()
        {
            //Console.WriteLine(drive + FileName.filename);
            int count = 0;
            using (FileStream fs = new FileStream(FileName.filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[2048];
                int num_bytes_read = fs.Read(buffer, 0, buffer.Length);
                while (num_bytes_read > 0)
                {
                    buffer = buffer.Where(c => c != null).ToArray();
                    Console.WriteLine(FileName.filename);
                    var name = Encoding.UTF8.GetBytes(FileName.filename);
                    byte[] len = BitConverter.GetBytes(FileName.filename.Length);
                    //byte a = Convert.ToByte(FileName.filename.Length);
                    byte a = Convert.ToByte(name.Length);
                    Array.Reverse(len);
                    //Console.WriteLine(len);
                    Console.WriteLine(Encoding.UTF8.GetString(name));
                    Console.WriteLine(a);

                    byte[] rv = new byte[num_bytes_read + name.Length + 1]; // 1 + 1 is name length and num_byte reads
                    System.Buffer.BlockCopy(buffer, 0, rv, 0, num_bytes_read);
                    System.Buffer.BlockCopy(name, 0, rv, num_bytes_read, name.Length);

                    rv[num_bytes_read + name.Length] = a;

                    Console.WriteLine(rv[num_bytes_read + FileName.filename.Length]);
                    isReceive = 0;
                    Ethernet.SendData(rv);
                    count++;
                    while (isReceive == 0) ;
                    if (isReceive == 2) return;

                    Array.Clear(buffer, 0, 1024);
                    num_bytes_read = fs.Read(buffer, 0, buffer.Length);
                }

                FileName.status = "DONE";
                FileName.mode = "UPLOAD";
                string json = JsonConvert.SerializeObject(FileName);
                Ethernet.SendData(json);

                Console.WriteLine("Done");
                Console.WriteLine("======== COUNT = {0} ==========", count);
                isDone = 1;
            }
        }
    }
}
