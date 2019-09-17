using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sync_application
{
    class Ethernet
    {
        private static string ip;
        private static string port;
        private static Socket sender;
        private static Ethernet _ethernet = new Ethernet();
        public const int BufferSize = 2048;
        public static byte[] buffer = new byte[BufferSize];
        public static int _bytesRead;
        public Ethernet(string ip, string port)
        {
            Ip = ip;
            Port = port;
        }

        public Ethernet()
        {

        }

        public static string Ip { get => ip; set => ip = value; }
        public static string Port { get => port; set => port = value; }

        public static void Connect()
        {
            Global.mw.appendReportText("== Connecting to " + Ip + ":" + Port + " ==");
            if ((ip != "") && (port != ""))
            {
                try
                {
                    IPAddress ipAddress = IPAddress.Parse(ip);
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, Convert.ToInt16(port));

                    sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    // Connect to the remote endpoint.  
                    sender.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), sender);

                    Receive(sender);
                }
                catch (Exception ex)
                {
                    Global.mw.appendReportText("== Connect Fail to {0}:{1} ==", Ip, Port);
                    Console.WriteLine(ex);
                }
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());
                Global.mw.Dispatcher.Invoke(new Action(() => Global.mw.appendReportText("Socket connected to {0}", client.RemoteEndPoint.ToString())));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void Receive(Socket sender)
        {
            try
            {
                // Begin receiving the data from the remote device.  
                sender.BeginReceive(buffer, 0, BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), sender);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);
                Global.data = buffer;
                if (bytesRead > 0)
                {
                    _bytesRead = bytesRead;
                    Console.WriteLine("Receive {0} bytes", bytesRead);
                    //Global.mw.appendReportText("Received {0}", bytesRead);
                    //string message = Encoding.UTF8.GetString(buffer);
                    //Console.WriteLine(message);
                    try
                    {
                        Global.watcher.processEthernetData();
                    }
                    catch (Exception ex)
                    {

                    }

                }

                client.BeginReceive(buffer, 0, BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), client);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in recive callback {0}", ex.ToString());
            }
            
        }

        private static void Send(Socket client, Byte[] data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            //byte[] byteData = Encoding.ASCII.GetBytes();

            // Begin sending the data to the remote device.  
            client.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);
            }
            catch (Exception ex)
            {

            }
        }

        public static void SendData(Byte[] data)
        {
            Send(sender, data);
        }

        public static void SendData(String data)
        {
            Byte[] arraybyte = Encoding.UTF8.GetBytes(data);
            Send(sender, arraybyte);
        }

        public static bool IsConnected()
        {
            bool part1, part2;
            try
            {
                part1 = sender.Poll(1000, SelectMode.SelectRead);
                part2 = (sender.Available == 0);
            }
            catch (Exception e)
            {
                return true;
            }

            if (part1 && part2)
                return false;
            else
                return true;
        }

        public static void Disconnect()
        {
            sender.Shutdown(SocketShutdown.Both);
            sender.Disconnect(true);
        }
    }
}
