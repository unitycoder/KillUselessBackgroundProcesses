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
        List<ProcessData> currentProcesses;
        List<ProcessData> unwantedAppsLoadedList;
        Dictionary<string, ProcessData> unwantedAppsLoaded = new Dictionary<string, ProcessData>();

        static readonly string settingsFile = "unwanted.json";
        static readonly string rootFolder = AppDomain.CurrentDomain.BaseDirectory;
        static readonly string unwantedFile = Path.Combine(rootFolder, settingsFile);

        bool autoScanAtStart = true;

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
            if (autoScanAtStart == true) Scan();
        }

        private void LoadUnwantedList()
        {
            if (File.Exists(unwantedFile) == true)
            {
                var json = File.ReadAllText(unwantedFile);
                unwantedAppsLoadedList = JsonConvert.DeserializeObject<List<ProcessData>>(json);

                // put into dictionary
                for (int i = 0; i < unwantedAppsLoadedList.Count; i++)
                {
                    Console.WriteLine(unwantedAppsLoadedList[i].Name);
                    // uses exe path for now.. NOTE its different for each computer..
                    unwantedAppsLoaded.Add(unwantedAppsLoadedList[i].FileName, unwantedAppsLoadedList[i]);
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
            Scan();
        }

        private void Scan()
        {
            currentProcesses = Scanner.Scan();

            // enable objects that are in unwanted list
            for (int i = 0; i < currentProcesses.Count; i++)
            {
                if (currentProcesses[i].FileName != null && unwantedAppsLoaded.ContainsKey(currentProcesses[i].FileName) == true)
                {
                    currentProcesses[i].Selected = true;
                }
            }

            gridProcess.ItemsSource = currentProcesses;
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
                    currentProcesses[row].Selected = !currentProcesses[row].Selected;
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
            if (currentProcesses == null) return;

            for (int i = 0; i < currentProcesses.Count; i++)
            {
                if (currentProcesses[i].FileName == null) continue; // NOTE for now cannot process if no filename available

                // if selected and not in the list already, then add
                if (currentProcesses[i].Selected == true)
                {
                    if (unwantedAppsLoaded.ContainsKey(currentProcesses[i].FileName) == false)
                    {
                        unwantedAppsLoaded.Add(currentProcesses[i].FileName, currentProcesses[i]);
                    }
                }
                else
                {
                    // not selected, check if was in the list previously, then remove from list
                    if (unwantedAppsLoaded.ContainsKey(currentProcesses[i].FileName) == true)
                    {
                        var b = unwantedAppsLoaded.Remove(currentProcesses[i].FileName);
                    }
                }
            }

            var json = JsonConvert.SerializeObject(unwantedAppsLoaded.Values, Formatting.Indented);
            //Console.WriteLine(json);
            Console.WriteLine("Saving settings..");
            File.WriteAllText(unwantedFile, json);
        }

        private void BtnKill_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();

            for (int i = currentProcesses.Count - 1; i >= 0; i--)
            {
                // TODO show progress, count killed exes
                if (currentProcesses[i].Selected == true)
                {
                    Console.WriteLine("Killing " + currentProcesses[i].Name + " " + currentProcesses[i].FileName);
                    if (currentProcesses[i].process != null && currentProcesses[i].process.HasExited == false)
                    {
                        try
                        {
                            currentProcesses[i].process.Kill();
                            currentProcesses[i].process.WaitForExit(); // needed if same exe is killed twice(?)
                        }
                        catch (Exception)
                        {
                        }
                    }
                    currentProcesses.RemoveAt(i);
                }
            }
            gridProcess.Items.Refresh();
            // TODO show stats
        }

        private void MenuSearchOnline_Click(object sender, RoutedEventArgs e)
        {
            var row = gridProcess.SelectedIndex;
            var exe = Path.GetFileName(currentProcesses[row].FileName);
            Console.WriteLine("search online: " + exe);
            Tools.Tools.SearchOnline(exe);
        }
    } // class
} // namespace
