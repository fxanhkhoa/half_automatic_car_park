using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace sync_application
{
    class Global
    {
        public static string location;
        public static upload _upload = new upload();
        public static download _download = new download();
        public static SyncBack syncBack = new SyncBack();
        public static Byte[] data = new byte[2048];
        public static Watcher watcher = new Watcher();
        public static MainWindow mw = (MainWindow)Application.Current.MainWindow;
    }
}
