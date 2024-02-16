using System;
using System.Net.Sockets;
using System.Net;

namespace Netease_Music_Proxy
{
    class Utils
    {
        public static bool isTcpPortAvailable(int port)
        {
            IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            try
            {
                TcpListener tcpListener = new TcpListener(ipAddress, 666);
                tcpListener.Start();
                tcpListener.Stop();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}
