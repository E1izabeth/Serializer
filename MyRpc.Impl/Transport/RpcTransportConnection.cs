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
    class RpcTransportConnection : IRpcTransportConnection<IPEndPoint, byte[]>
    {
        public IPEndPoint RemoteEndPoint { get; }
        private SocketAsyncEventArgs _scktAsyncEventArgs;
        public event Action OnClosed;
        private byte[] _buff;
        readonly Socket _sck;

        public RpcTransportConnection(Socket sck)
        {
            _sck = sck;
            this.RemoteEndPoint = (IPEndPoint)sck.RemoteEndPoint;
            _scktAsyncEventArgs = new SocketAsyncEventArgs();
            _scktAsyncEventArgs.Completed += this.SockAsyncEventArgs_Completed; 
        }

        public void Dispose()
        {
            _sck.Close();
            _sck.Dispose();
        }

        public void ReceivePacketAsync(Action<byte[]> onPacket)
        {
            if(this.ReceiveAsync(_scktAsyncEventArgs))
                onPacket(_scktAsyncEventArgs.Buffer);
        }

        public void SendPacketAsync(byte[] packet, Action onSent)
        {
            _scktAsyncEventArgs.SetBuffer(packet, 0, packet.Length);
            _sck.SendAsync(_scktAsyncEventArgs);
        }

        public void Start()
        {
            _sck.Connect(this.RemoteEndPoint);
        }

        private void SockAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            this.ReceiveAsync(e);
        }

        private bool ReceiveAsync(SocketAsyncEventArgs e)
        {
           return _sck.ReceiveAsync(e);
        }
    }
}
