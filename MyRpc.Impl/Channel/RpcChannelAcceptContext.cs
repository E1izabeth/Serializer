using MyRpc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyRpc.Impl.Channel
{
    class RpcChannelAcceptContext<TService> : IRpcChannelAcceptContext<IPEndPoint, TService>
    {
        public IPEndPoint RemoteEndPoint => throw new NotImplementedException();

        public IRpcChannel<TService> Confirm(TService service)
        {
            throw new NotImplementedException();
        }

        public void Reject()
        {
            throw new NotImplementedException();
        }
    }
}
