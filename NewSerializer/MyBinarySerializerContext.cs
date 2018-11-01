using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSerializer
{
    abstract class MyBinarySerializerContext
    {
        protected enum TypeKind : byte
        {
            Null = 0x00,

            Boolean,
            Char,
            Byte,
            SByte,
            Int16,
            UInt16,
            Int32,
            UInt32,
            Int64,
            UInt64,
            Single,
            Double,

            String,
            Array,
            Custom,

            Ref = 0xff
        }

        protected static readonly Dictionary<Type, (TypeKind, Action<StreamBinaryWriter, object>)> _primitiveWriters;
        protected static readonly Dictionary<TypeKind, Func<StreamBinaryReader, object>> _primitiveReaders;

        static Type[] _primitiveTypes = new[]
        {
            typeof(bool),
            typeof(char),
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
        };

        static MyBinarySerializerContext()
        {
            _primitiveWriters = _primitiveTypes.ToDictionary(t => t, t => {
                Func<object, object[], object> writeMethod = typeof(StreamBinaryWriter).GetMethod("Write" + t.Name).Invoke;
                return (t.Name.ParseEnum<TypeKind>(), new Action<StreamBinaryWriter, object>((w, v) => writeMethod(w, new[] { v })));
            });

            _primitiveReaders = _primitiveTypes.ToDictionary(
                t => t.Name.ParseEnum<TypeKind>(),
                t => {
                    Func<object, object[], object> readMethod = typeof(StreamBinaryReader).GetMethod("Read" + t.Name).Invoke;
                    return new Func<StreamBinaryReader, object>(r => readMethod(r, new object[0]));
                }
            );
        }

    }
}
