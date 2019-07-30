using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
                Console.WriteLine(dialog.SelectedPath);
                FolderName.Text = dialog.SelectedPath.ToString();
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

        private void Sync_Btn_Click(object sender, RoutedEventArgs e)
        {
            Ethernet.Ip = IP.Text;
            Ethernet.Port = Port.Text;
            Ethernet.Connect();

            Global._upload.file.filename = "plate0638718.jpg";
            Global._upload.file.mode = "UPLOAD";
            Global._upload.file.status = "PROCESSING";
            Console.WriteLine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            Global._upload.Do_Upload();
        }
    }
}
