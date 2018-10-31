using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Serializer.SerializeTypes;

namespace Serializer
{
    public class NullInfo : SerializeInstanceInfo
    {
        public NullInfo()
            : base(null)
        {
        }

        public override object Get(List<ISerializeInstanceInfo> instanceInfos)
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
