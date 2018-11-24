using MyRpc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRpc.Impl.Channel
{
    internal class RpcHost : IRpcHost<string>
    {
        public IRpcChannel<TService> Connect<TEndPoint, TPacket, TService>(IRpcProtocol<TEndPoint, TPacket, string> protocol, TEndPoint remoteEndPoint)
        {
            throw new NotImplementedException();
        }

        public IRpcChannelListener<TEndPoint, TService> Listen<TEndPoint, TPacket, TService>(IRpcProtocol<TEndPoint, TPacket, string> protocol, TEndPoint localEndPoint)
        {
            throw new NotImplementedException();
        }
    }
}
