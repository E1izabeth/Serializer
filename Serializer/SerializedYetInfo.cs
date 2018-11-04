using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Serializer.SerializeTypes;

namespace Serializer
{
    public class SerializedYetInfo : SerializeInstanceInfo
    {
        public SerializedYetInfo(int index)
            : base(null)
        {
            this.NumberInList = index;
        }

        public override object Get(List<ISerializeInstanceInfo> instanceInfos)
        {
            var o = instanceInfos[this.NumberInList].Instance;
            return o;
        }

        public override void Read(Stream stream)
        {
            this.NumberInList = stream.ReadInt32();
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte((byte)SerializeTypeEnum.SerializedYet);
            stream.WriteInt32(this.NumberInList);
        }
    }
}
