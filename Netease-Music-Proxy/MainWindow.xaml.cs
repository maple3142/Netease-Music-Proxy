﻿using Hardcodet.Wpf.TaskbarNotification;
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

        private NeteaseMusicProxyManager manager = new NeteaseMusicProxyManager();
        public MainWindow()
        {
            InitializeComponent();
            if (!manager.IsAbleToUpdateConfig())
            {
                autoUpdateProxyCheckBox.IsEnabled = false;
                autoUpdateProxyCheckBox.ToolTip = "Cannot read Netease Music config.";
            }
            var isRunning = Process.GetProcessesByName("cloudmusic").FirstOrDefault(p => p.MainModule.FileName.Contains("Netease")) != default(Process);
        }
        private void ToggleClicked(object sender, RoutedEventArgs e)
        {
            var isCloudMusicRunning = Process.GetProcessesByName("cloudmusic").FirstOrDefault(p => p.MainModule.FileName.Contains("Netease")) != default(Process);
            if (isCloudMusicRunning)
            {
                MessageBox.Show("Netease Music is running, please close it first before changing proxy status.", "Warning");
                return;
            }
            if (!manager.IsProxyRunning())
            {
                manager.Start();
                toggleBtn.Content = "Stop";
                WriteLine("Proxy server listening on port " + manager.GetPort());
                if (autoUpdateProxyCheckBox.IsChecked ?? false)
                {
                    manager.UpdateConfigAccordingly();
                    WriteLine("Updated config to use proxy");
                }
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


        private void OutputScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            outputScrollViewer.ScrollToVerticalOffset(outputScrollViewer.ExtentHeight);
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
                    tbi.TrayRightMouseDown += delegate
                    {
                        this.Show();
                        tbi.Dispose();
                    };
                    tbi.TrayMouseDoubleClick += delegate
                    {
                        this.Show();
                        tbi.Dispose();
                    };
                    break;
            }
        }
    }
}