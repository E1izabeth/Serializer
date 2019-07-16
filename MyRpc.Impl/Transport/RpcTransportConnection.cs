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
        public event Action<Exception> OnError = delegate { };

        public IPEndPoint RemoteEndPoint { get; }

        public event Action OnClosed = delegate { };

        readonly Socket _socket;
        readonly NetworkStream _stream;

        public RpcTransportConnection(Socket sck)
        {
            _socket = sck;
            _stream = new NetworkStream(_socket);

            this.RemoteEndPoint = (IPEndPoint)_socket.RemoteEndPoint;
        }

        public void Dispose()
        {
            _socket.Close();
            _stream.Dispose();
        }

        //public void ReceivePacketAsync(Action<byte[]> onPacket)
        //{
        //    var len = 0;
        //    byte[] buff = new byte[4];
        //    _stream.BeginRead(buff, 0, 4, ac1 =>
        //    {
        //        _stream.EndRead(ac1);
        //        len = BitConverter.ToInt32(buff, 0);
        //        buff = new byte[len];
        //        _stream.BeginRead(buff, 0, len, ac2 =>
        //        {
        //            _stream.EndRead(ac2);

        //            onPacket(buff);
        //        }, null);
        //    }, null);
        //}

        //public void SendPacketAsync(byte[] packet, Action onSent)
        //{
        //    _stream.BeginWrite(BitConverter.GetBytes(packet.Length), 0, 4, ar1 =>
        //    {
        //        _stream.EndWrite(ar1);

        //        _stream.BeginWrite(packet, 0, packet.Length, ar2 =>
        //        {
        //            _stream.EndWrite(ar2);

        //            onSent();
        //        }, null);
        //    }, null);
        //}

        public async Task SendPacketAsync(byte[] packet)
        {
            await _stream.WriteAsync(BitConverter.GetBytes(packet.Length), 0, 4);
            await _stream.WriteAsync(packet, 0, packet.Length);
        }

        public async Task<byte[]> ReceivePacketAsync()
        {
            var len = 0;
            byte[] buff = new byte[4];
            await _stream.ReadAsync(buff, 0, 4);
            len = BitConverter.ToInt32(buff, 0);
            buff = new byte[len];
            await _stream.ReadAsync(buff, 0, len);
            return buff;
        }
    }
}
