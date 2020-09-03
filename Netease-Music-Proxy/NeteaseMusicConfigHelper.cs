using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netease_Music_Proxy
{
    class NeteaseMusicConfigHelper
    {
        private string configPath;

        public NeteaseMusicConfigHelper()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..\\Local\\Netease\\CloudMusic\\Config");
        }

        public bool ConfigExists()
        {
            return File.Exists(configPath);
        }

        private string originalJson;

        public dynamic ReadConfig()
        {
            originalJson = File.ReadAllText(configPath);
            return JObject.Parse(originalJson);
        }

        public void WriteConfig(dynamic obj)
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(obj));
        }

        public void RestoreConfig()
        {
            File.WriteAllText(configPath, originalJson);
        }
    }
}
