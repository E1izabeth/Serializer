using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public interface ISerializeTypeInfo
    {
        void Write(Stream stream);
        void Read(Stream stream);
        object Get();
    }

    public abstract class SerializeTypeInfo :ISerializeTypeInfo
    {
        public abstract ISerializeTypeInfo Apply(ITypesVisitor visitor, object obj);
        public abstract void Write(Stream stream);
        public abstract void Read(Stream stream);
        public abstract object Get();
        
        public static SerializeTypeInfo GetTypeInfo(object obj)
        {
            if (obj is null)
                return null;
            var t = obj.GetType();
            SerializeTypeInfo info = null;
            if (t.IsPrimitive || t == typeof(string))
            {
                info = new PrimitiveTypeInfo(obj);
            }
            else if (t.IsEnum)
            {
                info = new EnumInfo(obj);
            }
            else if (t.IsArray)
            {
                if (SerializeTypes.GetTypeEnum(t.GetElementType()) != SerializeTypes.SerializeTypeEnum.Custom)
                {
                    info = new ArrayOfStructInfo(obj);
                }
                else
                {
                    info = new ArrayOfByRefInfo(obj);
                }
            }
            else
            {
                info = new CustomTypeInfo(obj);
            }
            return info;
        }

        public static SerializeTypeInfo GetTypeInfo(SerializeTypes.SerializeTypeEnum type)
        {
            SerializeTypeInfo info = null;
            switch (type)
            {
                case SerializeTypes.SerializeTypeEnum.Null:
                case SerializeTypes.SerializeTypeEnum.Byte:
                case SerializeTypes.SerializeTypeEnum.Bool:
                case SerializeTypes.SerializeTypeEnum.Int16:
                case SerializeTypes.SerializeTypeEnum.Int32:
                case SerializeTypes.SerializeTypeEnum.Int64:
                case SerializeTypes.SerializeTypeEnum.UInt16:
                case SerializeTypes.SerializeTypeEnum.UInt32:
                case SerializeTypes.SerializeTypeEnum.UInt64:
                case SerializeTypes.SerializeTypeEnum.Float:
                case SerializeTypes.SerializeTypeEnum.Double:
                case SerializeTypes.SerializeTypeEnum.String:
                    {
                        info = new PrimitiveTypeInfo(type);
                    }
                    break;
                case SerializeTypes.SerializeTypeEnum.Enum:
                    {
                        info = new EnumInfo();
                    }
                    break;
                case SerializeTypes.SerializeTypeEnum.ArrayOfStruct:
                    {
                        info = new ArrayOfStructInfo();
                    }
                    break;
                case SerializeTypes.SerializeTypeEnum.ArrayOfByref:
                    {
                        info = new ArrayOfByRefInfo();
                    }
                    break;
                case SerializeTypes.SerializeTypeEnum.Custom:
                    {
                        info = new CustomTypeInfo();
                    }
                    break;
                default:
                    break;
            }
            return info;
        }
    }
}
