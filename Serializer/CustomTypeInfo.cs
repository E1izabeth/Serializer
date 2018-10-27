using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Serializer.SerializeTypes;

namespace Serializer
{
    public class CustomTypeInfo : SerializeTypeInfo
    {
        private string _assemblyName;
        private string _fullName;
        private int _fieldsCount;
        private List<SerializeTypeInfo> _fieldsInfo;

        public CustomTypeInfo()
        {

        }

        public CustomTypeInfo(object obj)
        {
            var t = obj.GetType();
            var fieldsInfo = t.GetFields(BindingFlags.Public | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            _assemblyName = t.Assembly.FullName.ToString();
            _fieldsInfo = new List<SerializeTypeInfo>();
            _fullName = t.FullName.ToString();
            _fieldsCount = fieldsInfo.Count();
            foreach (var f in fieldsInfo)
            {
                _fieldsInfo.Add(GetTypeInfo(f.GetValue(obj)));
            }
        }

        public override ISerializeTypeInfo Apply(ITypesVisitor visitor, object obj)
        {
            return visitor.GetCustomTypeInfo(obj);
        }

        public override object Get()
        {
            var objType = Assembly.Load(_assemblyName).GetType(_fullName);
            var o = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(objType);
            var fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            int j = 0;
            foreach (var f in fields)
            {
                f.SetValue(o, _fieldsInfo[j].Get());
                ++j;
            }
            return o;
        }

        public override void Read(Stream stream)
        {
            _assemblyName = stream.ReadString();
            _fullName = stream.ReadString();
            _fieldsCount = stream.ReadInt32();
            _fieldsInfo = new List<SerializeTypeInfo>();
            for (int i = 0; i < _fieldsCount; i++)
            {
                var info = GetTypeInfo((SerializeTypeEnum)stream.ReadByte());
                info.Read(stream);
                _fieldsInfo.Add(info);
            }
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte((byte)SerializeTypeEnum.Custom);
            stream.WritePrimitiveOrStringType(_assemblyName);
            stream.WritePrimitiveOrStringType(_fullName);
            stream.WriteInt32(_fieldsCount);
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
        }
    }
}
