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
        SByte = 2,
        Char = 3,
        Bool = 4,
        Int16 = 5,
        Int32 = 6,
        Int64 = 7,
        UInt16 = 8,
        UInt32 = 9,
        UInt64 = 10,
        Float = 11,
        Double = 12,
        String = 13,
        Enum = 14,
        ArrayOfPrimitives = 15,
        ArrayOfByref = 16,
        Custom = 17,
        SerializedYet = 18
    }

    public static class SerializeTypes
    {
        private static Dictionary<Type, SerializeTypeEnum> _typeMap = new Dictionary<Type, SerializeTypeEnum>{
            { typeof(byte), SerializeTypeEnum.Byte},
            { typeof(sbyte), SerializeTypeEnum.SByte},
            { typeof(char), SerializeTypeEnum.Char},
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
