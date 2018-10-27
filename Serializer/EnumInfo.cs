using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public class EnumInfo : SerializeTypeInfo
    {
        private string _assemblyName;
        private string _fullName;
        private dynamic _value;

        private Type _enumType = null;

        public EnumInfo()
        {

        }

        public EnumInfo(object obj)
        {
            var t = obj.GetType();
            _assemblyName = t.Assembly.FullName;
            _fullName = t.FullName;
            _value = Convert.ChangeType(obj, t.GetEnumUnderlyingType());
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte((byte)SerializeTypes.SerializeTypeEnum.Enum);
            stream.WritePrimitiveOrStringType(_assemblyName);
            stream.WritePrimitiveOrStringType(_fullName);
            stream.WritePrimitiveOrStringType((object)_value);
        }

        public override void Read(Stream stream)
        {
            _assemblyName = stream.ReadString();
            _fullName = stream.ReadString();
            _enumType = Assembly.Load(_assemblyName).GetType(_fullName);
            _value = stream.ReadPrimitiveOrStringType(_enumType.GetEnumUnderlyingType());
        }

        public override object Get()
        {
            var o = Enum.ToObject(_enumType, _value);
            return o;
        }

        public override ISerializeTypeInfo Apply(ITypesVisitor visitor, object obj)
        {
            return visitor.GetEnumInfo(obj);
        }
    }
}
