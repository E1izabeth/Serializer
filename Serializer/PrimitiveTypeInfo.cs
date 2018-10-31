using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public class PrimitiveTypeInfo : SerializeInstanceInfo
    {
        Type _type;
        private object _value;

        internal PrimitiveTypeInfo(SerializeTypeEnum type, ISerializationContext ctx)
            : base(ctx)
        {
            _type = SerializeTypes.GetType(type);
        }

        internal PrimitiveTypeInfo(object value, ISerializationContext ctx)
            : base(value, ctx)
        {
            _type = value.GetType();
            _value = value;
        }

        public override object Get(List<ISerializeInstanceInfo> instanceInfos)
        {
            this.Instance = _value;
            return _value;
        }

        public override void Read(Stream stream)
        {
            _value = stream.ReadPrimitiveOrStringType(_type);
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte((byte)SerializeTypes.GetTypeEnum(_type));
            stream.WritePrimitiveOrStringType(_value);
        }
    }
}
