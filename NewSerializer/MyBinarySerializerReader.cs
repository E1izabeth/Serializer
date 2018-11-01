using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NewSerializer
{
    internal class MyBinarySerializerReader : MyBinarySerializerContext
    {
        private readonly StreamBinaryReader _reader;
        private readonly List<object> _objById = new List<object>();
        private readonly List<Type> _typeById = new List<Type>();

        public MyBinarySerializerReader(StreamBinaryReader reader)
        {
            _reader = reader;
        }

        private void RegisterReadInstance(object obj)
        {
            if (obj is Type t)
            {
                _typeById.Add(t);
            }
            else
            {
                _objById.Add(obj);
            }
        }

        private T ReadInstance<T>(bool withCache = true)
        {
            return (T)this.ReadInstance(typeof(T), withCache);
        }

        private Type ReadTypeSignature()
        {
            var isGeneric = _reader.ReadBoolean();
            Type type;

            if (isGeneric)
            {
                var args = this.ReadInstance<Type[]>(false);
                var def = this.ReadInstance<Type>();
                type = def.MakeGenericType(args);
            }
            else
            {
                var assemblyFullName = this.ReadInstance<string>();
                var namespaceName = this.ReadInstance<string>();
                var typeNameParts = this.ReadInstance<string[]>();
                var typeFullName = namespaceName + "." + string.Join("+", typeNameParts);

                var asm = Assembly.Load(assemblyFullName);
                type = asm.GetType(typeFullName);
            }

            Console.WriteLine($"reading {type}");
            this.RegisterReadInstance(type);
            return type;
        }

        int _n = 0;

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
                else if (type == typeof(Type))
                    kind = TypeKind.Type;
                else if (type.IsArray)
                    kind = TypeKind.Array;
                else
                    kind = TypeKind.Custom;
            }

            Console.WriteLine((++_n) + ": reading " + kind + " " + (type == null ? "" : type.FullName));
            var n = _n;

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

                        if (withCache)
                            this.RegisterReadInstance(obj);
                    }
                    break;
                case TypeKind.Type:
                    {
                        obj = this.ReadTypeSignature();
                    }
                    break;
                case TypeKind.Array:
                    {
                        obj = this.ReadArrayInstanceImpl(type, withCache);
                    }
                    break;
                case TypeKind.Custom:
                    {
                        obj = this.ReadCustomTypeInstanceImpl(type, withCache);
                    }
                    break;
                case TypeKind.TypeRef:
                    {
                        var id = _reader.ReadInt32();
                        obj = _typeById[id];
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

            Console.WriteLine("\t" + n + ": " + (obj == null ? "<NULL>" : obj.GetType().FullName) + " " + (obj == null || obj.ToString() == obj.GetType().ToString() ? "" : obj.ToString()));

            return obj;
        }

        private object ReadArrayInstanceImpl(Type type, bool withCache)
        {
            object obj;
            Type elementType;
            int rank;

            if (type == null)
            {
                elementType = this.ReadInstance<Type>();
                rank = _reader.ReadInt32();
            }
            else
            {
                elementType = type.GetElementType();
                rank = type.GetArrayRank();
            }

            var dimensions = Enumerable.Range(0, rank).Select(n => _reader.ReadInt32()).ToArray();
            var arr = Array.CreateInstance(elementType, dimensions);

            obj = arr;
            if (withCache)
                this.RegisterReadInstance(obj);

            var indexes = new int[dimensions.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                arr.SetValue(this.ReadInstance(), indexes);

                for (int j = dimensions.Length - 1; j >= 0; j--)
                {
                    indexes[j]++;
                    if (indexes[j] < dimensions[j])
                        break;

                    indexes[j] = 0;
                }
            }

            return obj;
        }

        private object ReadCustomTypeInstanceImpl(Type type, bool withCache)
        {
            object obj;
            if (type == null)
            {
                type = this.ReadInstance<Type>();
            }

            if (type.IsEnum)
            {
                var underlyingType = type.GetEnumUnderlyingType();
                var primitiveReader = _primitiveReaders[_primitiveWriters[underlyingType].Item1];
                var rawValue = primitiveReader(_reader);
                obj = Enum.ToObject(type, rawValue);
            }
            else
            {
                obj = FormatterServices.GetUninitializedObject(type);
                if (withCache)
                    this.RegisterReadInstance(obj);

                type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .ForEach(f => f.SetValue(obj, this.ReadInstance()));
            }

            return obj;
        }
    }
}
