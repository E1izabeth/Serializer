using MyRpc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyRpc.Impl.Transport
{
    class ChunkedTcpTransport : IRpcTransport<IPEndPoint, byte[]>
    {
        public static readonly IRpcTransport<IPEndPoint, byte[]> Instance = new ChunkedTcpTransport();

        private ChunkedTcpTransport()
        {
        }
        
        IRpcTransportConnection<IPEndPoint, byte[]> IRpcTransport<IPEndPoint, byte[]>.CreateConnection(IPEndPoint remoteEndPoint)
        {
            Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sck.Connect(remoteEndPoint);
            return new RpcTransportConnection(sck);
        }

        IRpcTransportListener<IPEndPoint, byte[]> IRpcTransport<IPEndPoint, byte[]>.CreateListener(IPEndPoint localEndPoint)
        {
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            return new RpcTransportListener(listenSocket);
        }
    }
}
