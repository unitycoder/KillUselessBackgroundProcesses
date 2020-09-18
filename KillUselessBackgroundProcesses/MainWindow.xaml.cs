using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;

namespace KillUselessBackgroundProcesses
{
    public partial class MainWindow : Window
    {
        List<ProcessData> processDataSource;
        List<ProcessData> loadedAppsList;
        Dictionary<string, ProcessData> unwantedApps = new Dictionary<string, ProcessData>();

        static readonly string settingsFile = "unwanted.json";
        static readonly string rootFolder = AppDomain.CurrentDomain.BaseDirectory;
        static readonly string unwantedFile = Path.Combine(rootFolder, settingsFile);

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

            //var url = "https://gist.githubusercontent.com/unitycoder/0a9fd6eb6252148208f4abed56c93235/raw/37947f69b3f360b655a29d70ca9f5bf29604e441/UnwantedProcessTempTest.json";
            //LoadJSON(url);
            LoadUnwantedList();
        }

        private void LoadUnwantedList()
        {
            if (File.Exists(unwantedFile) == true)
            {
                var json = File.ReadAllText(unwantedFile);
                //var apps = JsonConvert.DeserializeObject<Dictionary<string, ProcessData>>(json);
                loadedAppsList = JsonConvert.DeserializeObject<List<ProcessData>>(json);

                //// put into dictionary
                for (int i = 0; i < loadedAppsList.Count; i++)
                {
                    Console.WriteLine(loadedAppsList[i].Name);
                    // uses exe path for now.. NOTE its different for each computer..
                    unwantedApps.Add(loadedAppsList[i].FileName, loadedAppsList[i]);
                }
            }
            else
            {
                //Console.WriteLine("file not found: " + unwantedFile);
            }
        }

        //void LoadJSONFromWeb(string url)
        //{
        //    string json = new WebClient().DownloadString(url);
        //    var apps = JsonConvert.DeserializeObject<List<TestTest>>(json);

        //    for (int i = 0; i < apps.Count; i++)
        //    {
        //        //Console.WriteLine(apps[i].Name);
        //    }
        //}

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            processDataSource = Scanner.Scan();

            // enable objects that are in unwanted list
            for (int i = 0; i < processDataSource.Count; i++)
            {
                if (processDataSource[i].FileName != null && unwantedApps.ContainsKey(processDataSource[i].FileName) == true)
                {
                    processDataSource[i].Selected = true;
                }
            }

            gridProcess.ItemsSource = processDataSource;

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

        private void GridProcess_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space: // toggle select for this item
                    var row = gridProcess.SelectedIndex;
                    processDataSource[row].Selected = !processDataSource[row].Selected;
                    gridProcess.Items.Refresh();
                    // fix row selection?
                    SetFocusToGrid(gridProcess, row);
                    break;
                default:
                    break;
            }
        }

        public static void SetFocusToGrid(DataGrid targetGrid, int index = -1)
        {
            // no items
            if (targetGrid.Items.Count < 1) return;
            // keep current row selected
            if (index == -1 && targetGrid.SelectedIndex > -1) index = targetGrid.SelectedIndex;
            // if no item selected, pick first
            if (index == -1) index = 0;
            targetGrid.SelectedIndex = index;
            // set full focus
            DataGridRow row = (DataGridRow)targetGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                targetGrid.UpdateLayout();
                // scroll to view if outside
                targetGrid.ScrollIntoView(targetGrid.Items[index]);
                row = (DataGridRow)targetGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            // NOTE does this causes move below?
            row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
            row.Focus();
            Keyboard.Focus(row);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            if (processDataSource == null) return;

            // TODO how to remove items from unwanted list? i guess user can remove, if the exe is running and listed, can deselect..

            //var newList = new List<string>();
            var newList = new List<ProcessData>();
            // add existing items first
            if (loadedAppsList != null) newList.AddRange(loadedAppsList);

            // TODO if failed to load appslist, dont overwrite with empty list..

            // add selected items to unwanted list, unless they are already there
            for (int i = 0; i < processDataSource.Count; i++)
            {
                // check if already in list or later use dictionary
                if (processDataSource[i].Selected == true)
                {
                    if (unwantedApps.ContainsKey(processDataSource[i]?.FileName) == false)
                    {
                        newList.Add(processDataSource[i]);
                    }
                }
            }

            //var json = JsonConvert.SerializeObject(unwantedApps, Formatting.Indented);
            var json = JsonConvert.SerializeObject(newList, Formatting.Indented);
            //Console.WriteLine(json);

            File.WriteAllText(unwantedFile, json);
        }

        private void BtnKill_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < processDataSource.Count; i++)
            {
                // check if already in list or later use dictionary
                if (processDataSource[i].Selected == true)
                {
                    Console.WriteLine("Killing " + processDataSource[i].Name);
                    try
                    {
                        processDataSource[i].process.Kill();
                        processDataSource.RemoveAt(i);
                    }
                    catch (Exception)
                    {
                    }
                    // TODO scan again or refresh?
                    gridProcess.Items.Refresh();
                }
            }
        }
    } // class
} // namespace
