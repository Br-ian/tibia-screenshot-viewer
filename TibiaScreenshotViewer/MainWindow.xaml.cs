using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using Cursors = System.Windows.Input.Cursors;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace TibiaScreenshotViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private List<TibiaScreenshot> _selectedScreenshots = new List<TibiaScreenshot>();
        private TibiaScreenshot _selectedScreenshot;
        private readonly Dictionary<TibiaScreenshot, TreeViewItem> _screenshotToTreeViewItem = new Dictionary<TibiaScreenshot, TreeViewItem>();
        private TreeViewItem _selectedTreeViewItem;

        public MainWindow()
        {
            InitializeComponent();

            SyncOnStartItem.IsChecked = Properties.Settings.Default.SyncOnStart;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(OnStartup));
        }

        private void OnStartup()
        {
            if (Properties.Settings.Default.SyncOnStart)
            {
                DoSync(false);
            }

            ReloadScreenshots();
        }

//        private void MonitorScreenshotFolder()
//        {
//            var sreenshotsFolderWatcher = new FileSystemWatcher
//            {
//                Path = TibiaUtils.GetScreenshotsPath(Properties.Settings.Default.TibiaFolder),
//                NotifyFilter = NotifyFilters.LastWrite,
//                Filter = "*.*"
//            };
//
//            sreenshotsFolderWatcher.Changed += ScreenshotsFolderWatcher_Changed;
//            sreenshotsFolderWatcher.EnableRaisingEvents = true;
//        }

        private void ReloadScreenshots()
        {
            Log.Debug("Reloading screenshots");

            MenuBar.IsEnabled = false;

            TibiaTreeView.Items.Clear();
            ClearSelectedScreenshot();

            if (Properties.Settings.Default.TibiaFolder == "")
            {
                Log.Warn("Tibia folder not set");
                SetTibiaFolder();
            }

            var selectedPath = Properties.Settings.Default.TibiaFolder;
            if (Properties.Settings.Default.DefaultFolder != "")
                selectedPath = Properties.Settings.Default.DefaultFolder;
            
            Log.Info($"Selected path to load is {selectedPath}");

            TibiaScreenshotViewerWindow.Title = $"Tibia Screenshot Viewer - {selectedPath}";

            string[] screenshotPaths = { };
            try
            {
                screenshotPaths = Directory.GetFiles(selectedPath, "*.png");
            }
            catch(Exception exception)
            {
                Log.Fatal($"Could not find screenshots at '{selectedPath}'", exception);
                MessageBox.Show($"Could not find screenshots at '{selectedPath}', exiting...", "Tibia Screenshot Viewer", MessageBoxButton.OK, MessageBoxImage.Error);

                if (Properties.Settings.Default.DefaultFolder != "")
                    Properties.Settings.Default.DefaultFolder = "";
                else 
                    Properties.Settings.Default.TibiaFolder = "";

                Properties.Settings.Default.Save();

                Environment.Exit(0);
            }

            var screenshots = screenshotPaths.Select(path => new TibiaScreenshot(path));

            var res = screenshots
                .OrderBy(screenshot => screenshot.Character)
                .GroupBy(screenshot => screenshot.Character)                
                .ToDictionary(characterGroup => characterGroup.Key, characterGroup => characterGroup
                    .OrderBy(screenshot => screenshot.TypeToDisplayString())
                    .GroupBy(screenshot => screenshot.TypeToDisplayString())
                    .ToDictionary(typeGroup => typeGroup.Key, typeGroup => typeGroup
                        .GroupBy(screenshot => screenshot.Timestamp.Date)
                        .OrderByDescending(kv => kv.Key)
                        .ToDictionary(dateGroup => dateGroup.Key, dateGroup => dateGroup
                            .OrderByDescending(screenshot => screenshot.Timestamp.TimeOfDay)
                            .ToList())));

            
            foreach (var kv1 in res)
            {
                // Character
                var parent1 = new TreeViewItem {Header = kv1.Key, Cursor = Cursors.Hand };
                TibiaTreeView.Items.Add(parent1);

                foreach (var kv2 in kv1.Value)
                {
                    // Type
                    var parent2 = new TreeViewItem { Header = kv2.Key, Cursor = Cursors.Hand };
                    parent1.Items.Add(parent2);

                    foreach (var kv3 in kv2.Value)
                    {
                        // Date
                        var dateFormat = CultureInfo.CurrentUICulture.DateTimeFormat.LongDatePattern;
                        
                        var parent3 = new TreeViewItem { Header = kv3.Key.ToString(dateFormat), Cursor = Cursors.Hand };
                        parent2.Items.Add(parent3);
                        
                        foreach (var tibiaScreenshot in kv3.Value)
                        {
                            // Time
                            var timeFormat = CultureInfo.CurrentUICulture.DateTimeFormat.LongTimePattern;

                            var parent4 = new TreeViewItem { Header = tibiaScreenshot.Timestamp.ToString(timeFormat), Cursor = Cursors.Hand, DataContext = tibiaScreenshot };
                            
                            _screenshotToTreeViewItem.Add(tibiaScreenshot, parent4);

                            parent3.Items.Add(parent4);
                        }
                    }
                }
            }

            if (screenshotPaths.Length == 0)
            {
                Log.Warn($"No screenshots found in {selectedPath}");
                EditMenuItem.IsEnabled = false;                
            }
            else
            {
                EditMenuItem.IsEnabled = true;
                TibiaTreeView.Focus();
                ((TreeViewItem)TibiaTreeView.Items[0]).IsSelected = true;
            }

            CountText.Text = $"{screenshotPaths.Length} Screenshots";
            //MonitorScreenshotFolder();

            MenuBar.IsEnabled = true;

            Log.Info("Reloaded screenshots");            
        }

        private void DoSync(bool askForConfirmation = true)
        {
            Log.Debug("Syncing screenshots");

            if (Properties.Settings.Default.TibiaFolder == "")
            {
                Log.Warn("Tibia folder not set");
                MessageBox.Show("No Tibia folder selected.", "Set Tibia Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                SetTibiaFolder();
            }

            if (Properties.Settings.Default.SyncFolder == "")
            {
                Log.Warn("Sync folder not set");
                MessageBox.Show("No Sync folder selected.", "Set Tibia Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                SetSyncFolder();
            }

            if (askForConfirmation)
            {
                var result =
                    MessageBox.Show($"Do you want to synchronize the screenshots from folder '{Properties.Settings.Default.TibiaFolder}' to folder '{Properties.Settings.Default.SyncFolder}'?", 
                        "Synchronize?", 
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    Log.Info("Syncing cancelled");
                    return;
                }
            }

            Log.Info($"Synchronizing the screenshots from folder '{Properties.Settings.Default.TibiaFolder}' to folder '{Properties.Settings.Default.SyncFolder}'");

            var syncWorker = new BackgroundWorker { WorkerReportsProgress = true };
            syncWorker.DoWork += SyncWorker_DoWork; ;
            syncWorker.ProgressChanged += SyncWorker_ProgressChanged;
            syncWorker.RunWorkerCompleted += SyncWorker_RunWorkerCompleted;

            MenuBar.IsEnabled = false;
            TibiaTreeView.IsEnabled = false;
            ScreenshotImage.IsEnabled = false;
            UpdateStatusBarText($"Synchronizing...");
            StatusBarProgressBar.Visibility = Visibility.Visible;
            syncWorker.RunWorkerAsync();
        }

        private void SyncWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ScreenshotImage.IsEnabled = true;
            TibiaTreeView.IsEnabled = true;
            MenuBar.IsEnabled = true;
            StatusBarProgressBar.Visibility = Visibility.Hidden;
            UpdateStatusBarText($"Ready");
        }

        private void SyncWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusBarProgressBar.Value = e.ProgressPercentage;
        }

        private void SyncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var screenshotPaths = Directory.GetFiles(Properties.Settings.Default.TibiaFolder, "*.png");

            for (var i = 0; i < screenshotPaths.Length; i++)
            {
                var screenshotPath = screenshotPaths[i];
                var filename = Path.GetFileName(screenshotPath);
                var destinationPath = Path.Combine(Properties.Settings.Default.SyncFolder, filename);

                if (!File.Exists(destinationPath))
                {
                    File.Copy(screenshotPath, destinationPath, true);
                    Log.Debug($"Copied '{screenshotPath}' to '{destinationPath}'");
                }
                (sender as BackgroundWorker)?.ReportProgress((int) ((double)i/ screenshotPaths.Length*100.0));
            }

            Log.Info("Synced screenshots");
        }

        private void SetTibiaFolder()
        {
            var registeryFolder = "";
            try
            {
                registeryFolder = TibiaUtils.GetTibiaPath();
            }
            catch (Exception exception)
            {
                Log.Error("Could not find Tibia in the registry", exception);
            }

            if (registeryFolder != "")
            {
                var useRegisteryResult = MessageBox.Show($"Tibia found in the registry. Use '{registeryFolder}' as the Tibia folder?", "Set Tibia Folder", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (useRegisteryResult == MessageBoxResult.Yes)
                {
                    Properties.Settings.Default.TibiaFolder = TibiaUtils.GetScreenshotsPath(registeryFolder);
                    Properties.Settings.Default.Save();

                    return;
                }
            }

            MessageBox.Show("Please select the folder containing Tibia.exe", "Set Tibia Folder", MessageBoxButton.OK, MessageBoxImage.Information);

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Tibia executable|Tibia.exe",
                Title = "Select Tibia.exe",
                InitialDirectory = Properties.Settings.Default.TibiaFolder
            };
            openFileDialog.FileOk += OpenFileDialog_FileOk;

            var dialogFolder = "";
            if (openFileDialog.ShowDialog() == true)
            {
                dialogFolder = Path.GetDirectoryName(openFileDialog.FileName);
            }

            if (dialogFolder != "")
            {
                var useFolderResult = MessageBox.Show($"Tibia.exe selected. Use '{dialogFolder}' as the Tibia folder?", "Set Tibia Folder", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (useFolderResult == MessageBoxResult.Yes)
                {
                    Properties.Settings.Default.TibiaFolder = TibiaUtils.GetScreenshotsPath(dialogFolder);
                    Properties.Settings.Default.Save();

                    return;
                }
            }

            var reset = MessageBox.Show($"No new Tibia folder selected. Current Tibia folder is '{Properties.Settings.Default.TibiaFolder}'. Reset the Tibia folder?", "Set Tibia Folder", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (reset == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.TibiaFolder = "";
                Properties.Settings.Default.Save();
            }
        }

        private void SetSyncFolder()
        {
            var selectedSyncFolder = Properties.Settings.Default.SyncFolder;

            var folderBrowserDialog = new FolderBrowserDialog { Description = @"Select the folder to synchronize to", SelectedPath = Properties.Settings.Default.SyncFolder };

            var result = folderBrowserDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                selectedSyncFolder = folderBrowserDialog.SelectedPath;               
            }
            else
            {
                var reset = MessageBox.Show($"No new Sync folder selected. Current Sync folder is '{Properties.Settings.Default.SyncFolder}'. Reset the Sync folder?", "Set Sync Folder", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (reset == MessageBoxResult.Yes)
                {
                    selectedSyncFolder = "";                  
                }
            }

            Properties.Settings.Default.SyncFolder = selectedSyncFolder;
            Properties.Settings.Default.Save();
            Log.Info($"Sync folder set to '{selectedSyncFolder}'");
        }

        private void SetDefaultFolder()
        {
            var selectedDefaultFolder = Properties.Settings.Default.DefaultFolder;

            var folderBrowserDialog = new FolderBrowserDialog { Description = @"Select the folder to open by default", SelectedPath = Properties.Settings.Default.DefaultFolder };

            var result = folderBrowserDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                selectedDefaultFolder = folderBrowserDialog.SelectedPath;                
            }
            else
            {
                var reset = MessageBox.Show($"No new Default folder selected. Current Default folder is '{Properties.Settings.Default.DefaultFolder}'. Reset the Default folder?", "Set Default Folder", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (reset == MessageBoxResult.Yes)
                {
                    selectedDefaultFolder = "";                    
                }
            }

            Properties.Settings.Default.DefaultFolder = selectedDefaultFolder;
            Properties.Settings.Default.Save();
            Log.Info($"Default folder set to '{selectedDefaultFolder}'");
        }

        private void DisplayScreenshot(TibiaScreenshot tibiaScreenshot)
        {
            _selectedScreenshot = tibiaScreenshot;

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(_selectedScreenshot.Path);
            image.EndInit();

            ScreenshotImage.Source = image;

            var dateTimeFormat = CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern;
            DescriptionLabel.Content = $"{_selectedScreenshot.Character} - {_selectedScreenshot.TypeToDisplayString()} - {_selectedScreenshot.Timestamp.ToString(dateTimeFormat)}";            
        }

        private void ClearSelectedScreenshot()
        {
            _selectedScreenshot = null;
            ScreenshotImage.Source = null;
            DescriptionLabel.Content = "";
        }
        
        private void OpenSelectedScreenshot()
        {
            if (_selectedScreenshot == null)
            {
                Log.Warn("Cannot open screenshot, no screenshot selected");
                return;
            }

            ShowSelectedInExplorer.FileOrFolder(_selectedScreenshot.Path);
        }

        private void CopySelectedScreenshot()
        {
            if (_selectedScreenshot == null)
            {
                Log.Warn("Cannot copy screenshot, no screenshot selected");
                return;
            }

            using (var fs = new FileStream(_selectedScreenshot.Path, FileMode.Open, FileAccess.Read))
            {
                var img = new BitmapImage();

                img.BeginInit();
                img.StreamSource = fs;
                img.EndInit();

                Clipboard.SetImage(img);
            }
        }

        private void DeleteSelectedScreenshot()
        {
            if (_selectedScreenshot == null)
            {
                Log.Warn("Cannot delete screenshot, no screenshot selected");
                return;
            }

            var delete = MessageBox.Show($"Delete the selected screenshot from '{_selectedScreenshot.Path}'", "Delete Screenshot", MessageBoxButton.YesNo, MessageBoxImage.Stop, MessageBoxResult.No);

            if (delete == MessageBoxResult.Yes)
            {
                var path = _selectedScreenshot.Path;
                ClearSelectedScreenshot();
                File.Delete(path);
                ReloadScreenshots();
            }
        }

        private void UpdateStatusBarText(string text)
        {
            StatusBarText.Text = text;
        }

        private static IEnumerable<TibiaScreenshot> GetLeafs(TreeViewItem tvi)
        {
            var result = new List<TibiaScreenshot>();
            if (tvi == null)
                return result;

            if (tvi.Items.Count == 0)
            {
                return new List<TibiaScreenshot> { tvi.DataContext as TibiaScreenshot }; 
            }

            foreach (TreeViewItem child in tvi.Items)
            {
                var leafs = GetLeafs(child);
                result.AddRange(leafs);
            }

            return result;
        }

//        private void ScreenshotsFolderWatcher_Changed(object sender, FileSystemEventArgs e)
//        {
//            // TODO: handle new screenshots
//            //throw new NotImplementedException();
//        }

        private void TibiaTreeView_OnSelectedItemChangedTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedTreeViewItem = (TreeViewItem)e.NewValue;
            _selectedScreenshots = GetLeafs(_selectedTreeViewItem).OrderByDescending(screenshot => screenshot.Timestamp).ToList();

            if (_selectedScreenshots.Count == 0)
                return;

            DisplayScreenshot(_selectedScreenshots.First());
        }

        private void ScreenshotImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenSelectedScreenshot();
        }

        private void OpenFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var extension = Path.GetExtension(((OpenFileDialog)sender).FileName);
            if (!string.Equals(extension, ".exe", StringComparison.Ordinal))
            {
                e.Cancel = true;
            }
        }

        private void SyncItem_Click(object sender, RoutedEventArgs e)
        {
            DoSync();
            ReloadScreenshots();
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SetTibiaFolderItem_Click(object sender, RoutedEventArgs e)
        {
            SetTibiaFolder();
            ReloadScreenshots();
        }

        private void SetSyncFolderItem_Click(object sender, RoutedEventArgs e)
        {
            SetSyncFolder();
        }

        private void SetDefaultFolderItem_Click(object sender, RoutedEventArgs e)
        {
            SetDefaultFolder();
            ReloadScreenshots();
        }

        private void SyncOnStartItem_Click(object sender, RoutedEventArgs e)
        {
            SyncOnStartItem.IsChecked = !SyncOnStartItem.IsChecked;
            Properties.Settings.Default.SyncOnStart = SyncOnStartItem.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void ReloadCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ReloadScreenshots();
        }

        private void ResetItem_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
        }

        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow {Owner = this};
            aboutWindow.ShowDialog();
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenSelectedScreenshot();
        }

        private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CopySelectedScreenshot();
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteSelectedScreenshot();
        }

        private void ReportItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Br-ian/tibia-screenshot-viewer/issues/new");
        }
    }
}
