using MyRpc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyRpc.Impl.Channel
{
    internal class RpcChannelListener<TService> : IRpcChannelListener<IPEndPoint, TService>
    {
        public IPEndPoint LocalEndPoint => throw new NotImplementedException();

        public void AcceptChannelAsync(Action<IRpcChannelAcceptContext<IPEndPoint, TService>> onAccept)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
