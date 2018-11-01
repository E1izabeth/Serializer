using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NewSerializer
{
    class MyBinarySerializerReader : MyBinarySerializerContext
    {
        readonly StreamBinaryReader _reader;
        readonly List<object> _objById = new List<object>();

        public MyBinarySerializerReader(StreamBinaryReader reader)
        {
            _reader = reader;
        }

        private void RegisterReadInstance(object obj)
        {
            _objById.Add(obj);
        }

        private T ReadInstance<T>()
        {
            return (T)this.ReadInstance(typeof(T));
        }

        private Type ReadTypeSignature()
        {
            var assemblyFullName = this.ReadInstance<string>();
            var typeFullName = this.ReadInstance<string>();

            var asm = Assembly.Load(assemblyFullName);
            var type = asm.GetType(typeFullName);

            return type;
        }

        public object ReadInstance(Type type = null, bool withCache = true)
        {
            TypeKind kind;
            if (type == null || withCache)
            {
                kind = (TypeKind)_reader.ReadByte();
            }
            else
            {
                if (_primitiveWriters.TryGetValue(type, out var writerInfo))
                    kind = writerInfo.Item1;
                else if (type == typeof(string))
                    kind = TypeKind.String;
                else if (type.IsArray)
                    kind = TypeKind.Array;
                else
                    kind = TypeKind.Custom;
            }

            object obj;

            switch (kind)
            {
                case TypeKind.Null:
                    {
                        obj = null;
                    }
                    break;
                case TypeKind.Boolean:
                case TypeKind.Char:
                case TypeKind.Byte:
                case TypeKind.SByte:
                case TypeKind.Int16:
                case TypeKind.UInt16:
                case TypeKind.Int32:
                case TypeKind.UInt32:
                case TypeKind.Int64:
                case TypeKind.UInt64:
                case TypeKind.Single:
                case TypeKind.Double:
                    {
                        obj = _primitiveReaders[kind](_reader);
                    }
                    break;
                case TypeKind.String:
                    {
                        obj = _reader.ReadString();
                        this.RegisterReadInstance(obj);
                    }
                    break;
                case TypeKind.Array:
                    {
                        obj = this.ReadArrayInstanceImpl(type);
                    }
                    break;
                case TypeKind.Custom:
                    {
                        obj = this.ReadCustomTypeInstanceImpl(type);
                    }
                    break;
                case TypeKind.Ref:
                    {
                        var id = _reader.ReadInt32();
                        obj = _objById[id];
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return obj;
        }

        private object ReadArrayInstanceImpl(Type type)
        {
            object obj;
            Array arr;
            int[] dimensions;

            if (type == null)
            {
                var rank = _reader.ReadInt32();
                dimensions = Enumerable.Range(0, rank).Select(n => _reader.ReadInt32()).ToArray();

                var elementType = this.ReadTypeSignature();

                arr = Array.CreateInstance(elementType, dimensions);
            }
            else
            {
                arr = (Array)Activator.CreateInstance(type);
                dimensions = Enumerable.Range(0, arr.Rank).Select(n => arr.GetLength(n)).ToArray();
            }

            obj = arr;
            this.RegisterReadInstance(obj);

            var indexes = new int[dimensions.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                arr.SetValue(this.ReadInstance(), indexes);

                for (int j = 0; j < dimensions.Length; j++)
                {
                    indexes[j]++;
                    if (indexes[j] < dimensions[j])
                        break;

                    indexes[j] = 0;
                }
            }

            return obj;
        }

        private object ReadCustomTypeInstanceImpl(Type type)
        {
            object obj;
            if (type == null)
            {
                type = this.ReadTypeSignature();
            }

            if (type.IsEnum)
            {
                var underlyingType = type.GetEnumUnderlyingType();
                var primitiveReader = _primitiveReaders[_primitiveWriters[underlyingType].Item1];
                var rawValue = primitiveReader(_reader);
                obj = Enum.ToObject((Type)type, rawValue);
            }
            else
            {
                obj = FormatterServices.GetUninitializedObject((Type)type);
                this.RegisterReadInstance(obj);

                type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .ForEach(f => f.SetValue(obj, this.ReadInstance()));
            }

            return obj;
        }
    }
}
