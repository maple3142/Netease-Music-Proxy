using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using System.Windows.Threading;

namespace Netease_Music_Proxy
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    /// 
    class LogFilterDataContext
    {
        public ObservableCollection<string> items { get; }
        private Dictionary<string, bool> selectionTable;
        public LogFilterDataContext(Dictionary<string, bool> initItems)
        {
            items = new ObservableCollection<string>(initItems.Keys);
            selectionTable = new Dictionary<string, bool>(initItems);
        }
        private void resetSelectedTable()
        {
            foreach (string item in items)
            {
                selectionTable[item] = false;
            }
        }
        public string SelectedValue
        {
            get
            {
                return string.Join(",", from name in items where selectionTable[name] select name);
            }
            set
            {
                resetSelectedTable();
                foreach (string name in value.Split(','))
                {
                    selectionTable[name] = true;
                }
            }
        }

        public ObservableCollection<string> SelectedItems
        {
            get
            {
                return new ObservableCollection<string>(
                    (from x in items where selectionTable[x] select x).ToList()
                );
            }
        }

        public bool isNameSelected(string name)
        {
            bool val;
            selectionTable.TryGetValue(name, out val);
            return val;
        }
    }

    public partial class MainWindow : Window
    {

        private NeteaseMusicProxyManager manager = new NeteaseMusicProxyManager();
        private LogFilterDataContext ldc = new LogFilterDataContext(
            new Dictionary<string, bool>
            {
                { "info", true },
                { "error", true },
                { "proxy", false }
            }
        );
        public MainWindow()
        {
            InitializeComponent();
            if (!manager.IsAbleToUpdateConfig())
            {
                autoUpdateProxyCheckBox.IsEnabled = false;
                autoUpdateProxyCheckBox.ToolTip = "Cannot read Netease Music config.";
            }
            logFilterCheckComboBox.DataContext = ldc;
        }
        private void ToggleClicked(object sender, RoutedEventArgs e)
        {
            //var isCloudMusicRunning = Process.GetProcessesByName("cloudmusic").FirstOrDefault(p => p.MainModule.FileName.Contains("Netease")) != default(Process);
            //if (isCloudMusicRunning)
            //{
            //    MessageBox.Show("Netease Music is running, please close it first before changing proxy status.", "Warning");
            //    return;
            //}
            if (!manager.IsProxyRunning())
            {
                uint port = 0;
                if (!UInt32.TryParse(preferredPortTextBox.Text, out port))
                {
                    MessageBox.Show("Preferred port is not an integer");
                    return;
                }
                manager.Start((int)(port % 65536));
                toggleBtn.Content = "Stop";
                WriteLine("Proxy server listening on port " + manager.GetPort() + " using X-Real-IP: " + manager.proxy.getChinaIP());
                if (autoUpdateProxyCheckBox.IsChecked ?? false)
                {
                    manager.UpdateConfigAccordingly();
                    WriteLine("Updated config to use proxy");
                }
                manager.OnLogMessage += (tag, msg) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (ldc.isNameSelected(tag))
                        {
                            WriteLine("[" + tag + "] " + msg);
                        }
                    });
                };
            }
            else
            {
                manager.Stop();
                toggleBtn.Content = "Start";
                WriteLine("Proxy server stopped");
                if (autoUpdateProxyCheckBox.IsChecked ?? false)
                {
                    manager.RestoreConfig();
                    WriteLine("Restored orignal config");
                }
            }
        }

        private void WriteLine(string str)
        {
            outputBox.Text += str + "\n";
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (manager.IsProxyRunning())
            {
                e.Cancel = true;
                MessageBox.Show("Please stop proxy before closing", "Warning");
            }
        }

        private void LaunchClicked(object sender, RoutedEventArgs e)
        {
            var path64 = @"C:\Program Files (x86)\Netease\CloudMusic\cloudmusic.exe";
            var path32 = @"C:\Program Files\Netease\CloudMusic\cloudmusic.exe";
            var path = Environment.Is64BitOperatingSystem ? path64 : path32;
            if (File.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                MessageBox.Show("Can't find Netease Music", "Error");
            }
        }

        private void WindowStateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Minimized:
                    this.Hide();
                    TaskbarIcon tbi = new TaskbarIcon();
                    tbi.Icon = Netease_Music_Proxy.Properties.Resources.Icon;
                    tbi.ToolTipText = "Netease Music Proxy";
                    tbi.ContextMenu = new ContextMenu();
                    tbi.TrayRightMouseDown += (s, evt) =>
                    {
                        evt.Handled = true;
                        this.Show();
                        this.WindowState = WindowState.Normal;
                        this.Activate();
                        tbi.Dispose();
                    };
                    tbi.TrayMouseDoubleClick += (s, evt) =>
                    {
                        evt.Handled = true;
                        this.Show();
                        this.WindowState = WindowState.Normal;
                        this.Activate();
                        tbi.Dispose();
                    };
                    break;
            }
        }
    }
}