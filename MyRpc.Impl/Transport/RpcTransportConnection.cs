using MyRpc.Model;
using System;
using System.Collections.Generic;
using System.IO;
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

        public event Action OnClosed;

        readonly Socket _socket;
        readonly NetworkStream _stream;

        public RpcTransportConnection(Socket sck)
        {
            _socket = sck;
            _stream = new NetworkStream(sck);

            this.RemoteEndPoint = (IPEndPoint)sck.RemoteEndPoint;
        }

        public void Dispose()
        {
            _socket.Close();
            _stream.Dispose();
        }

        public void ReceivePacketAsync(Action<byte[]> onPacket)
        {
            var len = 0;
            byte[] buff = null;
            _stream.BeginRead(buff, 0, 4, ac1 => {
                _stream.EndRead(ac1);

                _stream.BeginRead(buff, 0, len, ac2 =>{
                    _stream.EndWrite(ac2);

                    onPacket(buff);
                }, null);
            }, null);
        }

        public void SendPacketAsync(byte[] packet, Action onSent)
        {
            _stream.BeginWrite(BitConverter.GetBytes(packet.Length), 0, 4, ar1 => {
                _stream.EndWrite(ar1);

                _stream.BeginWrite(packet, 0, packet.Length, ar2 => {
                    _stream.EndWrite(ar2);

                    onSent();
                }, null);
            }, null);
        }

        public void Start()
        {
            _socket.Connect(this.RemoteEndPoint);
        }
        
    }
}
