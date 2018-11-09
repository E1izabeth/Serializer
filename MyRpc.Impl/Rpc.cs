using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MyRpc.Model;

namespace MyRpc
{
    public static class Rpc
    {
        public static IRpcTransport<IPEndPoint, byte[]> TcpTransport { get => throw new NotImplementedException(); }

        public static IRpcSerializerFabric<object, byte[]> BinarySerializer { get => throw new NotImplementedException(); }

        public static IRpcHost<object> GenericHost { get => throw new NotImplementedException(); }

        class GenericProtocol<TEndPoint, TPacket, TMessage> : IRpcProtocol<TEndPoint, TPacket, TMessage>
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

    class GenericServiceHost<TMessage, TService> : IRpcServiceHost<TMessage, TService>
    {
        public IRpcHost<TMessage> Host { get; private set; }

        public GenericServiceHost(IRpcHost<TMessage> host)
        {
            this.Host = host;
        }

        public IRpcChannelListener<TEndPoint, TService> Listen<TEndPoint, TPacket>(IRpcProtocol<TEndPoint, TPacket, TMessage> protocol, TEndPoint localEndPoint)
        {
            return this.Host.Listen<TEndPoint, TPacket, TService>(protocol, localEndPoint);
        }

        public IRpcChannel<TService> Connect<TEndPoint, TPacket>(IRpcProtocol<TEndPoint, TPacket, TMessage> protocol, TEndPoint remoteEndPoint)
        {
            return this.Host.Connect<TEndPoint, TPacket, TService>(protocol, remoteEndPoint);
        }
    }

    class HostHelper<TMessage> : IRpcHostHelper<TMessage>
    {
        public IRpcHost<TMessage> Host { get; private set; }

        public HostHelper(IRpcHost<TMessage> host)
        {
            this.Host = host;
        }

        public IRpcServiceHost<TMessage, TService> ForService<TService>()
        {
            return new GenericServiceHost<TMessage, TService>(this.Host);
        }
    }

    public interface IRpcHostHelper<TMessage>
    {
        IRpcHost<TMessage> Host { get; }

        IRpcServiceHost<TMessage, TService> ForService<TService>();
    }

    public interface IRpcServiceHost<TMessage, TService>
    {
        IRpcHost<TMessage> Host { get; }

        IRpcChannelListener<TEndPoint, TService> Listen<TEndPoint, TPacket>(IRpcProtocol<TEndPoint, TPacket, TMessage> protocol, TEndPoint localEndPoint);
        IRpcChannel<TService> Connect<TEndPoint, TPacket>(IRpcProtocol<TEndPoint, TPacket, TMessage> protocol, TEndPoint remoteEndPoint);
    }

}
