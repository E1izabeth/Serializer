using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRpc.Model
{
    #region transport

    public interface IRpcTransport<TEndPoint, TPacket>
    {
        IRpcTransportConnection<TEndPoint, TPacket> CreateConnection(TEndPoint remoteEndPoint);
        IRpcTransportListener<TEndPoint, TPacket> CreateListener(TEndPoint localEndPoint);
    }

    public interface IRpcTransportConnection<TEndPoint, TPacket> : IDisposable
    {
        event Action<Exception> OnError;
        event Action OnClosed;

        TEndPoint RemoteEndPoint { get; }

        void SendPacketAsync(TPacket packet, Action onSent);
        void ReceivePacketAsync(Action<TPacket> onPacket);

        void Start();
    }

    public interface IRpcTransportListener<TEndPoint, TPacket> : IDisposable
    {
        event Action<Exception> OnError;

        TEndPoint LocalEndPoint { get; }

        void Start();

        void AcceptAsync(Action<IRpcTransportAcceptContext<TEndPoint, TPacket>> onAccepted);
    }

    public interface IRpcTransportAcceptContext<TEndPoint, TPacket>
    {
        TEndPoint RemoteEndPoint { get; }

        IRpcTransportConnection<TEndPoint, TPacket> Confirm();
        void Reject();
    }

    #endregion

    #region channel

    public interface IRpcProtocol<TEndPoint, TPacket, TMessage>
    {
        IRpcTransport<TEndPoint, TPacket> Transport { get; }
        IRpcSerializerFabric<TMessage, TPacket> SerializerFabric { get; }
    }

    public interface IRpcHost<TMessage>
    {
        IRpcChannelListener<TEndPoint, TService> Listen<TEndPoint, TPacket, TService>(IRpcProtocol<TEndPoint, TPacket, TMessage> protocol, TEndPoint localEndPoint);
        IRpcChannel<TService> Connect<TEndPoint, TPacket, TService>(IRpcProtocol<TEndPoint, TPacket, TMessage> protocol, TEndPoint remoteEndPoint);
    }

    public interface IRpcChannelListener<TEndPoint, TSerivce> : IDisposable
    {
        TEndPoint LocalEndPoint { get; }

        void Start();
        void AcceptChannelAsync(Action<IRpcChannelAcceptContext<TEndPoint, TSerivce>> onAccept);
    }

    public interface IRpcChannelAcceptContext<TEndPoint, TService>
    {
        TEndPoint RemoteEndPoint { get; }

        IRpcChannel<TService> Confirm(TService service);
        void Reject();
    }

    public interface IRpcChannel<TService> : IDisposable
    {
        event Action OnClosed;

        TService Service { get; }

        void Start();
    }

    #endregion

    #region serializer

    public interface IRpcSerializerFabric<TMessage, TPacket>
    {
        IRpcSerializerContext<TMessage, TPacket> CreateContext(IRpcSerializationMarshaller marshaller);
    }

    public interface IRpcSerializerContext<TMessage, TPacket>
    {
        IRpcSerializer<TMessage, TPacket> CreateSerializer();
    }

    public interface IRpcSerializer<TMessage, TPacket>
    {
        TPacket Serialize(TMessage obj);
        TMessage Deserialize(TPacket packet);
    }

    public interface IRpcSerializationMarshaller
    {
        object Marshal(object obj);
        object Unmarshal(object obj);
    }

    #endregion
}
