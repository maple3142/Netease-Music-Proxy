﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netease_Music_Proxy
{
    class NeteaseMusicProxyManager
    {
        private MusicProxy proxy;
        private NeteaseMusicConfigHelper helper = new NeteaseMusicConfigHelper();

        public bool IsProxyRunning()
        {
            return proxy != null;
        }

        public bool IsAbleToUpdateConfig()
        {
            return helper.ConfigExists();
        }

        private int port = -1;

        public void Start()
        {
            proxy = new MusicProxy();
            port = proxy.Start();
        }


        public int GetPort()
        {
            return port;
        }

        public void Stop()
        {
            helper.RestoreConfig();
            proxy = null;
            port = -1;
        }
        public void UpdateConfigAccordingly()
        {
            if (!IsAbleToUpdateConfig() || !IsProxyRunning()) return;
            var cfg = helper.ReadConfig();
            cfg.Proxy.Type = "http";
            cfg.Proxy.http.Host = "localhost";
            cfg.Proxy.http.Port = Convert.ToString(port);
            cfg.Proxy.http.UserName = "";
            cfg.Proxy.http.Password = "";
            helper.WriteConfig(cfg);
        }

        public void RestoreConfig()
        {
            if (!IsAbleToUpdateConfig()) return;
            helper.RestoreConfig();
        }
    }
}
