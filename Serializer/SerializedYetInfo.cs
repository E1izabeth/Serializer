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
        public override object Get(List<ISerializeInstanceInfo> instanceInfos)
        {
            //var info = instanceInfos.ElementAt(numberInList);
            //var o = info.Get(instanceInfos);
            var o = instanceInfos[this.numberInList].Instance;
            return o;
        }

        public override void Read(Stream stream)
        {
            numberInList = stream.ReadInt32();
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte((byte)SerializeTypeEnum.SerializedYet);
            stream.WriteInt32(numberInList);
        }
    }
}
