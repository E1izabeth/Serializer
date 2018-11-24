using MyRpc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyRpc.Impl.Channel
{
    class RpcProtocol : IRpcProtocol<IPEndPoint, byte[], string>
    {
        public IRpcTransport<IPEndPoint, byte[]> Transport => throw new NotImplementedException();

        public IRpcSerializerFabric<string, byte[]> SerializerFabric => throw new NotImplementedException();
    }
}
