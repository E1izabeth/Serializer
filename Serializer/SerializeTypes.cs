using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public enum SerializeTypeEnum : byte
    {
        Null = 0,
        Byte = 1,
        Bool = 2,
        Int16 = 3,
        Int32 = 4,
        Int64 = 5,
        UInt16 = 6,
        UInt32 = 7,
        UInt64 = 8,
        Float = 9,
        Double = 10,
        String = 11,
        Enum = 12,
        ArrayOfPrimitives = 13,
        ArrayOfByref = 14,
        Custom = 15,
        SerializedYet = 16
    }

    public static class SerializeTypes
    {


        private static Dictionary<Type, SerializeTypeEnum> _typeMap = new Dictionary<Type, SerializeTypeEnum>{
            { typeof(byte), SerializeTypeEnum.Byte},
            { typeof(bool), SerializeTypeEnum.Bool},
            { typeof(Int16), SerializeTypeEnum.Int16},
            { typeof(Int32), SerializeTypeEnum.Int32},
            { typeof(Int64), SerializeTypeEnum.Int64},
            { typeof(UInt16), SerializeTypeEnum.UInt16},
            { typeof(UInt32), SerializeTypeEnum.UInt32},
            { typeof(UInt64), SerializeTypeEnum.UInt64},
            { typeof(float), SerializeTypeEnum.Float},
            { typeof(double), SerializeTypeEnum.Double},
            { typeof(string), SerializeTypeEnum.String},
        };

        public static SerializeTypeEnum GetTypeEnum(Type t)
        {
            if (_typeMap.ContainsKey(t))
                return _typeMap[t];
            return SerializeTypeEnum.Custom;
        }

        public static Type GetType(SerializeTypeEnum t)
        {
            return _typeMap.FirstOrDefault(s => s.Value == t).Key;
        }
    }
}
