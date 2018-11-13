using MyRpc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRpc.Impl
{

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
