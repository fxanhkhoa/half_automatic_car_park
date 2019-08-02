﻿using System;
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
        private FileObject fileName;
        private byte isJson;
        private byte isReceive;

        public upload()
        {
            FileName = new FileObject();
        }

        public FileObject FileName { get => fileName; set => fileName = value; }

        public void Do_Upload()
        {
            string json = JsonConvert.SerializeObject(FileName);
            Ethernet.SendData(json);
            Thread thr1 = new Thread(new ThreadStart(UploadThread));
            thr1.IsBackground = true;
            thr1.Start();
        }

        public void process(Byte[] data)
        {
            try
            {
                string json = Encoding.UTF8.GetString(data);
                Console.WriteLine(json);
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
                isJson = 0;
            }
        }

        public void UploadThread()
        {
            using (FileStream fs = new FileStream(fileName.filename, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[1024];
                while (fs.Read(buffer, 0, buffer.Length) > 0)
                {   
                    var name = Encoding.UTF8.GetBytes(fileName.filename);
                    byte[] len = BitConverter.GetBytes(fileName.filename.Length);
                    byte a = Convert.ToByte(fileName.filename.Length);
                    Array.Reverse(len);
                    //Console.WriteLine(len);
                    Console.WriteLine(name.ToString());

                    byte[] rv = new byte[buffer.Length + name.Length + 1];
                    System.Buffer.BlockCopy(buffer, 0, rv, 0, buffer.Length);
                    System.Buffer.BlockCopy(name, 0, rv, buffer.Length, name.Length);

                    rv[buffer.Length + name.Length] = a;

                    Console.WriteLine(rv[1024 + fileName.filename.Length]);
                    isReceive = 0;
                    Ethernet.SendData(rv);
                    while (isReceive == 0) ;
                    if (isReceive == 2) return;

                    Array.Clear(buffer, 0, 1024);
                }
            }
        }
    }
}
