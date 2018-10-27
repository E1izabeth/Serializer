using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Serializer.SerializeTypes;

namespace Serializer
{
    public class NullInfo : SerializeTypeInfo
    {
        public override ISerializeTypeInfo Apply(ITypesVisitor visitor, object obj)
        {
            throw new NotImplementedException();
        }

        public override object Get()
        {
            return null;
        }

        public override void Read(Stream stream)
        {
            
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte((byte)SerializeTypeEnum.Null);
        }
    }
}
