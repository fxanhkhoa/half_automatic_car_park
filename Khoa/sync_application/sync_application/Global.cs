using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync_application
{
    class Global
    {
        public static string location;
        public static upload _upload = new upload();
        public static download _download = new download();
        public static Byte[] data = new byte[2048];
    }
}
