using System;
using System.Collections.Generic;
using System.IO;

namespace Serializer
{
    public interface ISerializeInstanceInfo
    {
        object Instance { get; }

        void Write(Stream stream);
        void Read(Stream stream);
        object Get(List<ISerializeInstanceInfo> instanceInfos);
    }

    public abstract class SerializeInstanceInfo :ISerializeInstanceInfo
    {
        public int numberInList;
        public abstract void Write(Stream stream);
        public abstract void Read(Stream stream);
        public abstract object Get(List<ISerializeInstanceInfo> instanceInfos);

        public object Instance { get; protected set; }

        public SerializeInstanceInfo()
        {
        }

        internal SerializeInstanceInfo(object obj, ISerializationContext ctx)
        {
            this.Instance = obj;
            ctx.RegisterInstanceInfo(this);
        }

        public static SerializeInstanceInfo GetUninitializedTypeInfo(SerializeTypeEnum type)
        {
            SerializeInstanceInfo info = null;
            switch (type)
            {
                case SerializeTypeEnum.Null:
                case SerializeTypeEnum.Byte:
                case SerializeTypeEnum.Bool:
                case SerializeTypeEnum.Int16:
                case SerializeTypeEnum.Int32:
                case SerializeTypeEnum.Int64:
                case SerializeTypeEnum.UInt16:
                case SerializeTypeEnum.UInt32:
                case SerializeTypeEnum.UInt64:
                case SerializeTypeEnum.Float:
                case SerializeTypeEnum.Double:
                case SerializeTypeEnum.String:
                    {
                        info = new PrimitiveTypeInfo(type);
                    }
                    break;
                case SerializeTypeEnum.Enum:
                    {
                        info = new EnumInfo();
                    }
                    break;
                case SerializeTypeEnum.ArrayOfPrimitives:
                    {
                        info = new ArrayOfPrimitivesInfo();
                    }
                    break;
                case SerializeTypeEnum.ArrayOfByref:
                    {
                        info = new ArrayOfByRefInfo();
                    }
                    break;
                case SerializeTypeEnum.Custom:
                    {
                        info = new CustomInfo();
                    }
                    break;
                case SerializeTypeEnum.SerializedYet:
                    {
                        info = new SerializedYetInfo();
                    }
                    break;
                default:
                    break;
            }
            return info;
        }
    }
}
