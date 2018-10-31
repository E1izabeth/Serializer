using System;
using System.Collections.Generic;
using System.IO;

namespace Serializer
{
    public interface ISerializeInstanceInfo
    {
        object Instance { get; }

        void Write(Stream stream);
        void Read(Stream stream);
        object Get(List<ISerializeInstanceInfo> instanceInfos);
    }

    public abstract class SerializeInstanceInfo : ISerializeInstanceInfo
    {
        public int NumberInList { get; protected set; }

        public object Instance { get; protected set; }
        internal ISerializationContext Context { get; set; }

        internal SerializeInstanceInfo(ISerializationContext ctx)
        {
            this.RegisterMe(ctx);
        }

        internal SerializeInstanceInfo(object obj, ISerializationContext ctx)
        {
            this.Instance = obj;
            this.RegisterMe(ctx);
        }

        private void RegisterMe(ISerializationContext ctx)
        {
            this.Context = ctx;

            if (this.Context != null)
                this.NumberInList = this.Context.RegisterInstanceInfo(this);
        }

        public abstract void Write(Stream stream);
        public abstract void Read(Stream stream);
        public abstract object Get(List<ISerializeInstanceInfo> instanceInfos);
    }
}
