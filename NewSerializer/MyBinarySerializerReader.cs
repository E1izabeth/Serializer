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
        class BytesLogger : IDisposable
        {
            readonly MyBinarySerializerReader _owner;
            readonly long _position;
            readonly string _msgFormat;
            readonly object[] _msgArgs;

            public BytesLogger(MyBinarySerializerReader owner, string format, params object[] args)
            {
                _owner = owner;
                _position = owner._reader.Position;
                _msgFormat = format;
                _msgArgs = args;
            }

            void IDisposable.Dispose()
            {
                var reader = _owner._reader;
                var end = reader.Position;

                reader.Position = _position;
                var bytes = reader.ReadBytes((int)(end - _position));

                _owner._log.Write("[{0}] ", bytes.RawHexDump());
                _owner._log.WriteLine(_msgFormat, _msgArgs.Select(arg => arg is Func<object> act ? act() : arg).ToArray());
            }
        }

        private readonly StreamBinaryReader _reader;
        private readonly List<object> _objById = new List<object>();
        private readonly List<Type> _typeById = new List<Type>();
        private readonly IndentedWriter _log = new IndentedWriter();

        public MyBinarySerializerReader(StreamBinaryReader reader)
        {
            _reader = reader;
        }

        BytesLogger LogBytes(string msg, params object[] args)
        {
            return new BytesLogger(this, msg, args);
        }

        BytesLogger LogBytes(string msg, params Func<object>[] args)
        {
            return new BytesLogger(this, msg, args);
        }

        public string GetDebugLog()
        {
            return _log.GetContentAsString();
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
            var isGeneric = false;
            using (this.LogBytes("type signature genericness: {0}", () => isGeneric))
                isGeneric = _reader.ReadBoolean();

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

            // Console.WriteLine($"reading {type}");
            this.RegisterReadInstance(type);
            return type;
        }

        // int _n = 0;

        public object ReadInstance(Type type = null, bool withCache = true)
        {
            _log.WriteLine("instance {0} {{", type);
            _log.Push();

            TypeKind kind = default(TypeKind);
            if (type == null || withCache)
            {
                using (this.LogBytes("instance kind: {0}", () => kind))
                {
                    kind = (TypeKind)_reader.ReadByte();
                }
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

            // Console.WriteLine((++_n) + ": reading " + kind + " " + (type == null ? "" : type.FullName));
            // var n = _n;

            object obj = default(object);

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
                        using (this.LogBytes("primitive: {0}", () => obj))
                            obj = _primitiveReaders[kind](_reader);
                    }
                    break;
                case TypeKind.String:
                    {
                        using (this.LogBytes("string: {0}", () => obj))
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
                        using (this.LogBytes("type ref: ", () => obj))
                        {
                            var id = _reader.ReadInt32();
                            obj = _typeById[id];
                        }
                    }
                    break;
                case TypeKind.Ref:
                    {
                        using (this.LogBytes("instance ref: ", () => obj))
                        {
                            var id = _reader.ReadInt32();
                            obj = _objById[id];
                        }
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }

            // Console.WriteLine("\t" + n + ": " + (obj == null ? "<NULL>" : obj.GetType().FullName) + " " + (obj == null || obj.ToString() == obj.GetType().ToString() ? "" : obj.ToString()));

            _log.Pop();
            _log.WriteLine($"}} {obj}");
            return obj;
        }

        private object ReadArrayInstanceImpl(Type type, bool withCache)
        {
            object obj;
            Type elementType;
            int rank = default(int);

            if (type == null)
            {
                elementType = this.ReadInstance<Type>();

                using (this.LogBytes("array rank: {0}", () => rank))
                    rank = _reader.ReadInt32();
            }
            else
            {
                elementType = type.GetElementType();
                rank = type.GetArrayRank();
            }

            var dimensions = default(int[]);
            using (this.LogBytes("array dimensions spec: {{{0}}}", () => string.Join(", ", dimensions)))
                dimensions = Enumerable.Range(0, rank).Select(n => _reader.ReadInt32()).ToArray();

            var arr = Array.CreateInstance(elementType, dimensions);

            obj = arr;
            if (withCache)
                this.RegisterReadInstance(obj);

            var indexes = new int[dimensions.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                arr.SetValue(this.ReadInstance(this.TypeIfInfoRequired(elementType), this.CanBeCached(this.TypeIfInfoRequired(elementType))), indexes);

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

                var rawValue = default(object);
                using (this.LogBytes("enum instance: {0}", () => rawValue))
                    rawValue = primitiveReader(_reader);

                obj = Enum.ToObject(type, rawValue);
            }
            else
            {
                obj = FormatterServices.GetUninitializedObject(type);
                if (withCache)
                    this.RegisterReadInstance(obj);

                type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .ForEach(f => f.SetValue(obj, this.ReadInstance(this.TypeIfInfoRequired(f.FieldType), this.CanBeCached(this.TypeIfInfoRequired(f.FieldType)))));
            }

            return obj;
        }

        private Type TypeIfInfoRequired(Type type)
        {
            return this.IsTypeInfoRequired(type) ? null : type;
        }
    }
}
