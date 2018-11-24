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
    public class RpcTransportAcceptContext : IRpcTransportAcceptContext<IPEndPoint, byte[]>
    {
        public IPEndPoint RemoteEndPoint { get; }
        private Socket _socket;

        public RpcTransportAcceptContext(Socket socket)
        {
            this.RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            _socket = socket;
        }

        public IRpcTransportConnection<IPEndPoint, byte[]> Confirm()
        {
            return new RpcTransportConnection(_socket);
        }

        public void Reject()
        {
            _socket.Close();
            _socket.Dispose();
        }
    }
}
