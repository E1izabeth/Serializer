using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public class PrimitiveTypeInfo : SerializeTypeInfo
    {
        Type _type;
        private object _value;

        public PrimitiveTypeInfo(SerializeTypes.SerializeTypeEnum type)
        {
            _type = SerializeTypes.GetType(type);
        }

        public PrimitiveTypeInfo(object value)
        {
            _type = value.GetType();
            _value = value;
        }

        public override ISerializeTypeInfo Apply(ITypesVisitor visitor, object obj)
        {
            throw new NotImplementedException();
        }

        public override object Get()
        {
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
