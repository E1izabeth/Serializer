using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Serializer
{
    public class CustomInfo : SerializeInstanceInfo
    {
        private string _assemblyName;
        private string _fullName;
        private List<SerializeInstanceInfo> _fieldsInfo;

        internal CustomInfo(ISerializationContext ctx)
            : base(ctx)
        {

        }

        internal CustomInfo(object obj, ISerializationContext ctx)
            : base(obj, ctx)
        {
            var t = obj.GetType();
            var fieldsInfo = t.GetFields(BindingFlags.Public | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

            _fieldsInfo = new List<SerializeInstanceInfo>();
            _assemblyName = t.Assembly.FullName;
            _fullName = t.FullName;

            foreach (var f in fieldsInfo)
            {
                var fieldVal = f.GetValue(obj);
                _fieldsInfo.Add(ctx.GetInstanceInfo(fieldVal));

            }
        }

        public override object Get(List<ISerializeInstanceInfo> instanceInfos)
        {
            var objType = Assembly.Load(_assemblyName).GetType(_fullName);
            var o = FormatterServices.GetUninitializedObject(objType);
            var fields = objType.GetFields(BindingFlags.Public | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            this.Instance = o;
            int j = 0;
            foreach (var f in fields)
            {
                f.SetValue(o, _fieldsInfo[j].Get(instanceInfos));
                ++j;
            }

            return o;
        }

        public override void Read(Stream stream)
        {
            _assemblyName = stream.ReadString();
            _fullName = stream.ReadString();
            var count = stream.ReadInt32();
            _fieldsInfo = new List<SerializeInstanceInfo>(count);
            for (int i = 0; i < count; i++)
            {
                var info = this.Context.GetUninitializedTypeInfo((SerializeTypeEnum)stream.ReadByte());
                info.Read(stream);
                _fieldsInfo.Add(info);
            }
        }

        public override void Write(Stream stream)
        {
            //if (numberInList != 0)
            //{
            //    stream.WriteByte((byte)SerializeTypeEnum.SerializedYet);
            //    stream.WritePrimitiveOrStringType(numberInList);
            //}
            //else
            //{
                stream.WriteByte((byte)SerializeTypeEnum.Custom);
                stream.WritePrimitiveOrStringType(_assemblyName);
                stream.WritePrimitiveOrStringType(_fullName);
                stream.WriteInt32(_fieldsInfo.Count);
                foreach (var f in _fieldsInfo)
                {
                    if (f is null)
                    {
                        (new NullInfo()).Write(stream);
                    }
                    else
                    {
                        f.Write(stream);
                    }
                }
            //}
        }
    }
}
