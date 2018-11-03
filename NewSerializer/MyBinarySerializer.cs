using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NewSerializer
{
    class MyBinarySerializer
    {
        string _lastLog;

        public string LastLog { get { return _lastLog; } }

        public MyBinarySerializer()
        {
        }

        public void Serialize(object obj, Stream stream)
        {
            var writer = new StreamBinaryWriter(stream, false, true);
            var ctx = new MyBinarySerializerWriter(writer);
            ctx.WriteInstance(obj);
            writer.Flush();
            stream.Flush();
        }

        public object Deserialize(Stream stream)
        {
            var reader = new StreamBinaryReader(stream, false, true);
            var ctx = new MyBinarySerializerReader(reader);
            var obj = ctx.ReadInstance();
            _lastLog = ctx.GetDebugLog();
            return obj;
        }

    }
}
