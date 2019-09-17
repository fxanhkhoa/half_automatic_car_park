using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;


namespace sync_application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SyncFolder_Btn_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                Console.WriteLine(result);
                if (dialog.SelectedPath == "") return;
                string current_path = dialog.SelectedPath.Substring(dialog.SelectedPath.IndexOf(":") + 1, 
                                                                    dialog.SelectedPath.Length - 2);
                Console.WriteLine(current_path);
                FolderName.Text = dialog.SelectedPath.ToString();
                Global.location = dialog.SelectedPath.ToString();
            }
        }

        private void IP_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Port_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void IP_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void IP_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void IP_GotFocus(object sender, RoutedEventArgs e)
        {
            if (IP.Text == "Input IP")
            {
                IP.Text = "";
            }
        }

        private void IP_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IP.Text == "")
            {
                IP.Text = "Input IP";
            }
        }

        private void Port_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Port.Text == "")
            {
                Port.Text = "Input PORT";
            }
        }

        private void Port_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Port.Text == "Input PORT")
            {
                Port.Text = "";
            }
        }

        public void appendReportText(string text, params object[] parameters)
        {    
            reportField.Text += string.Format(text, parameters) + Environment.NewLine;
            scrollReport.ScrollToBottom();
        }

        public void WriteLine(string text, params object[] args)
        {
            var message = args.Length == 0 ? text : string.Format(text, args);
            string.Format(text, args);
        }

        private void SaveSetting()
        {
            Properties.Settings.Default.IP = IP.Text;
            Properties.Settings.Default.Port = Port.Text;
            Properties.Settings.Default.Location = FolderName.Text;
            Properties.Settings.Default.Save();
        }

        private void LoadSetting()
        {
            IP.Text = Properties.Settings.Default.IP;
            Port.Text = Properties.Settings.Default.Port;
            FolderName.Text = Properties.Settings.Default.Location;
        }

        private void Sync_Btn_Click(object sender, RoutedEventArgs e)
        {
            appendReportText("== Saving Setting ==");
            SaveSetting();
                
            try
            {
                Ethernet.Ip = IP.Text;
                Ethernet.Port = Port.Text;
                Ethernet.Connect();
            }
            catch (Exception ex)
            {
                Global.mw.appendReportText("Failed to connect to Server {0}:{1}", IP.Text, Port.Text);
            }

            /////////// Send Folder To Sync ///////////
            ///
            FileObject folderInfo = new FileObject();
            string dirName = System.IO.Path.GetFileName(FolderName.Text);
            folderInfo.filename = dirName;
            folderInfo.mode = "LOCATION";
            folderInfo.status = "";

            string json = JsonConvert.SerializeObject(folderInfo);
            Ethernet.SendData(json);

            Global.location = dirName;
            //string current_path = FolderName.Text.Substring(FolderName.Text.IndexOf(":") + 2,
            //                                                        FolderName.Text.Length - 3); // Remove *:/

            Console.WriteLine(FolderName.Text);

            upload.isDone = 1;
            Global.watcher.location = dirName;
            Global.watcher.start();

            //Watcher.drive = FolderName.Text.Substring(0, FolderName.Text.IndexOf(":") + 2);

            //Global._upload.FileName.filename = "plate0638718.jpg";
            //Global._upload.FileName.mode = "UPLOAD";
            //Global._upload.FileName.status = "PROCESSING";
            //Console.WriteLine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            //Global._upload.Do_Upload();

            //Global._download.FileName.filename = "0000_02187_b.jpg";
            //Global._download.FileName.mode = "DOWNLOAD";
            //Global._download.FileName.status = "PROCESSING";
            //Global._download.DoDownload();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            appendReportText("== Loading Setting ==");
            LoadSetting();
            if (IP.Text == "")
            {
                IP.Text = "Input IP";
            }
            if (Port.Text == "")
            {
                Port.Text = "Input Port";
            }
        }

        /// <summary>
        /// Use to Download all folders, files from server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Download_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Ethernet.Ip = IP.Text;
                Ethernet.Port = Port.Text;
                Ethernet.Connect();
            }
            catch (Exception ex)
            {
                Global.mw.appendReportText("Failed to connect to Server {0}:{1}", IP.Text, Port.Text);
            }

            FileObject downloadObj = new FileObject();
            downloadObj.filename = "FileTable.json";
            downloadObj.mode = "SYNCBACK";
            downloadObj.status = "PROCESSING";

            download.drive = FolderName.Text.Substring(0, FolderName.Text.IndexOf(":") + 2);

            string json = JsonConvert.SerializeObject(downloadObj);
            Ethernet.SendData(json);
        }
    }
}
