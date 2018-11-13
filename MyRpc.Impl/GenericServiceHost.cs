using MyRpc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRpc.Impl
{
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
}
