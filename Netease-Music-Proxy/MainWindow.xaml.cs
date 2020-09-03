using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

namespace Netease_Music_Proxy
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private NeteaseMusicConfigHelper helper;
        public MainWindow()
        {
            InitializeComponent();
            helper = new NeteaseMusicConfigHelper();
            if (!helper.isConfigExist())
            {
                autoUpdateProxyCheckBox.IsEnabled = false;
                autoUpdateProxyCheckBox.ToolTip = "Cannot read Netease Music config.";
            }
            var isRunning = Process.GetProcessesByName("cloudmusic").FirstOrDefault(p => p.MainModule.FileName.Contains("Netease")) != default(Process);
        }

        private MusicProxy proxy;
        private void ToggleClicked(object sender, RoutedEventArgs e)
        {
            var isCloudMusicRunning = Process.GetProcessesByName("cloudmusic").FirstOrDefault(p => p.MainModule.FileName.Contains("Netease")) != default(Process);
            if (isCloudMusicRunning)
            {
                MessageBox.Show("Netease Music is running, please close it first before changing proxy status.", "Warning");
                return;
            }
            if (proxy == null)
            {
                proxy = new MusicProxy();
                int port = proxy.Start();
                toggleBtn.Content = "Stop";
                writeLine("Proxy server listening on port " + port);
                if (autoUpdateProxyCheckBox.IsChecked ?? false)
                {
                    var cfg = helper.readConfig();
                    cfg.Proxy.Type = "http";
                    cfg.Proxy.http.Host = "localhost";
                    cfg.Proxy.http.Port = Convert.ToString(port);
                    cfg.Proxy.http.UserName = "";
                    cfg.Proxy.http.Password = "";
                    helper.writeConfig(cfg);
                    writeLine("Updated config to use proxy");
                }
            }
            else
            {
                proxy.Stop();
                proxy = null;
                toggleBtn.Content = "Start";
                writeLine("Proxy server stopped");
                if (autoUpdateProxyCheckBox.IsChecked ?? false)
                {
                    helper.restoreConfig();
                    writeLine("Restored orignal config");
                }
            }
        }

        private void writeLine(string str)
        {
            outputBox.Text += str + "\n";
        }


        private void OutputScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            outputScrollViewer.ScrollToVerticalOffset(outputScrollViewer.ExtentHeight);
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (proxy != null)
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
    }
}