using System;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace Netease_Music_Proxy
{
    class MusicProxy
    {
        private ProxyServer server;
        public MusicProxy(int port = 64538)
        {
            server = new ProxyServer(true, false, false);
            server.BeforeRequest += OnRequest;
            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Loopback, port, true);
            server.AddEndPoint(explicitEndPoint);
        }

        private string chinaIp = null;
        public string getChinaIP()
        {
            if (chinaIp == null)
            {
                var rnd = new Random();
                chinaIp = string.Format("111.{0}.{1}.{2}", rnd.Next(1, 63), rnd.Next(1, 255), rnd.Next(1, 254));
            }
            return chinaIp;
        }

        public Task OnRequest(object sender, SessionEventArgs e)
        {
            OnRequestUrl?.Invoke(e.HttpClient.Request.Url);
            var headers = e.HttpClient.Request.Headers;
            headers.AddHeader("X-Real-IP", getChinaIP());
            return Task.FromResult(0);
        }

        public int Start()
        {
            server.Start(false);
            var end = server.ProxyEndPoints[0];
            return end.Port;
        }

        public void Stop()
        {
            server.Stop();
        }

        public event Action<string> OnRequestUrl;
    }
}
