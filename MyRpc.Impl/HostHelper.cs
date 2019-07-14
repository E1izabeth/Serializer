using MyRpc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRpc.Impl
{
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

    [Serializable]
    internal sealed class ObjInfo
    {
        internal Int64 Id { get; private set; }
        internal string[] IfaceNames { get; private set; }

        public ObjInfo(long id, string[] ifaceNames)
        {
            this.Id = id;
            this.IfaceNames = ifaceNames;
        }
    }
}
