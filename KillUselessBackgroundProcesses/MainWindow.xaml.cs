using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Windows.Shell;

namespace KillUselessBackgroundProcesses
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        void Start()
        {
            // disable accesskeys without alt
            CoreCompatibilityPreferences.IsAltKeyRequiredInAccessKeyDefaultScope = true;

            // make window resizable (this didnt work when used pure xaml to do this)
            WindowChrome Resizable_BorderLess_Chrome = new WindowChrome();
            Resizable_BorderLess_Chrome.CornerRadius = new CornerRadius(0);
            Resizable_BorderLess_Chrome.CaptionHeight = 1.0;
            WindowChrome.SetWindowChrome(this, Resizable_BorderLess_Chrome);

            var url = "https://gist.githubusercontent.com/unitycoder/0a9fd6eb6252148208f4abed56c93235/raw/37947f69b3f360b655a29d70ca9f5bf29604e441/UnwantedProcessTempTest.json";
            LoadJSON(url);

        }

        void LoadJSON(string url)
        {
            string json = new WebClient().DownloadString(url);
            var apps = JsonConvert.DeserializeObject<List<TestTest>>(json);

            for (int i = 0; i < apps.Count; i++)
            {
                Console.WriteLine(apps[i].Name);
            }


        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            var list = Scanner.Scan();
            gridProcess.ItemsSource = list;

        }



        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            // remove focus from minimize button
            gridProcess.Focus();
            //if (chkMinimizeToTaskbar.IsChecked == true)
            //{
            //  notifyIcon.Visible = true;
            //   this.Hide();
            //}
            //else
            //{
            this.WindowState = WindowState.Minimized;
            // }
        }

        private void OnRectangleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

    }



}
