using MyRpc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRpc.Impl.Channel
{
    internal class RpcChannel<TService> : IRpcChannel<TService>
    {
        public TService Service => throw new NotImplementedException();

        public event Action OnClosed;

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
