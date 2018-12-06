using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MyRpc.Impl;
using MyRpc.Impl.Transport;
using MyRpc.Model;

namespace MyRpc
{
    public static class Rpc
    {
        public static IRpcTransport<IPEndPoint, byte[]> TcpTransport { get => ChunkedTcpTransport.Instance; }

        public static IRpcSerializerFabric<object, byte[]> BinarySerializer { get; private set; } = new Impl.BinarySerializerImpl.MyBinarySerializerFabric();

        public static IRpcHost<object> GenericHost { get => throw new NotImplementedException(); }

        protected class GenericProtocol<TEndPoint, TPacket, TMessage> : IRpcProtocol<TEndPoint, TPacket, TMessage>
        {
            public IRpcTransport<TEndPoint, TPacket> Transport { get; private set; }
            public IRpcSerializerFabric<TMessage, TPacket> SerializerFabric { get; private set; }

            public GenericProtocol(IRpcTransport<TEndPoint, TPacket> transport, IRpcSerializerFabric<TMessage, TPacket> serializerFabric)
            {
                this.Transport = transport;
                this.SerializerFabric = serializerFabric;
            }
        }

        public static IRpcProtocol<TEndPoint, TPacket, TMessage> MakeProtocol<TEndPoint, TPacket, TMessage>(this IRpcTransport<TEndPoint, TPacket> transport, IRpcSerializerFabric<TMessage, TPacket> serializer)
        {
            return new GenericProtocol<TEndPoint, TPacket, TMessage>(transport, serializer);
        }

        public static IRpcHostHelper<TMessage> Helper<TMessage>(this IRpcHost<TMessage> host)
        {
            return new HostHelper<TMessage>(host);
        }
    }
}
