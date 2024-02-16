using System;
using System.Net.Sockets;
using System.Net;

namespace Netease_Music_Proxy
{
    class Utils
    {
        public static bool isTcpPortAvailable(int port)
        {
            try
            {
                TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 666);
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
