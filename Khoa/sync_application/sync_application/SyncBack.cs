using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sync_application
{
    class SyncBack
    {
        bool isDone = false;
        string fileTalbe;

        public void DoSyncBack(string fileTable)
        {
            this.fileTalbe = fileTable;

            Thread thr1 = new Thread(new ThreadStart(SyncBackThread));
            thr1.IsBackground = true;
            thr1.Start();
        }

        public void SyncBackThread()
        {
            List<FileInfoJson> listfileInfo = new List<FileInfoJson>();
            using (FileStream fs = new FileStream(fileTalbe, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                string json;
                using (StreamReader reader = new StreamReader(fs))
                {
                    json = reader.ReadToEnd();
                }
                listfileInfo = JsonConvert.DeserializeObject<List<FileInfoJson>>(json);
            }

            foreach (FileInfoJson fileInfoJson in listfileInfo)
            {
                Global._download.FileName.filename = fileInfoJson.filename;
                Global._download.FileName.mode = "DOWNLOAD";
                Global._download.FileName.status = "PROCESSING";

                Global._download.DoDownload();

                isDone = false;
                while (!isDone) ;
            }


        }

        public void setDone()
        {
            isDone = true;
        }
    }
}
