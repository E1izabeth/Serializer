using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NewSerializer
{
    class MyBinarySerializer
    {
        enum TypeKind : byte
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

        static readonly Dictionary<Type, (TypeKind, Action<StreamBinaryWriter, object>)> _primitiveWriters;
        static readonly Dictionary<TypeKind, Func<StreamBinaryReader, object>> _primitiveReaders;

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

        static MyBinarySerializer()
        {
            _primitiveWriters = _primitiveTypes.ToDictionary(t => t, t =>
            {
                Func<object, object[], object> writeMethod = typeof(StreamBinaryWriter).GetMethod("Write" + t.Name).Invoke;
                return (t.Name.ParseEnum<TypeKind>(), new Action<StreamBinaryWriter, object>((w, v) => writeMethod(w, new[] { v })));
            });

            _primitiveReaders = _primitiveTypes.ToDictionary(
                t => t.Name.ParseEnum<TypeKind>(),
                t =>
                {
                    Func<object, object[], object> readMethod = typeof(StreamBinaryReader).GetMethod("Read" + t.Name).Invoke;
                    return new Func<StreamBinaryReader, object>(r => readMethod(r, new object[0]));
                }
            );
        }

        readonly Dictionary<object, int> _idByObj = new Dictionary<object, int>();
        readonly List<object> _objById = new List<object>();

        public MyBinarySerializer()
        {
        }

        public void Serialize(object obj, Stream stream)
        {
            var writer = new StreamBinaryWriter(stream, false, true);
            this.WriteInstance(obj, writer);
            writer.Flush();
            stream.Flush();

            _idByObj.Clear();
        }

        private void RegisterWrittenInstance(object obj)
        {
            _idByObj.Add(obj, _idByObj.Count);
        }

        private void WriteTypeSignature(Type type, StreamBinaryWriter w)
        {
            this.WriteInstance(type.Assembly.FullName, w, false);
            this.WriteInstance(type.FullName, w, false);
        }

        private void WriteInstance(object obj, StreamBinaryWriter w, bool withTypeInfo = true, bool withCache = true)
        {
            Type t = obj == null ? null : obj.GetType();

            if (obj != null && withCache && _idByObj.TryGetValue(obj, out int id))
            {
                w.WriteByte((byte)TypeKind.Ref);
                w.WriteInt32(id);
            }
            else
            {
                switch (obj)
                {
                    case null:
                        {
                            w.WriteByte((byte)TypeKind.Null);
                        }
                        break;
                    case object value when t.IsPrimitive:
                        {
                            var (typeKind, primitiveWriter) = _primitiveWriters[t];

                            if (withTypeInfo || withCache)
                                w.WriteByte((byte)typeKind);

                            primitiveWriter(w, obj);
                        }
                        break;
                    case string str:
                        {
                            if (withTypeInfo || withCache)
                                w.WriteByte((byte)TypeKind.String);

                            this.RegisterWrittenInstance(obj);

                            w.WriteString(str);
                        }
                        break;
                    case Enum value:
                        {
                            if (withTypeInfo || withCache)
                            {
                                w.WriteByte((byte)TypeKind.Custom);

                                if (withTypeInfo)
                                    this.WriteTypeSignature(t, w);
                            }

                            var underlyingType = t.GetEnumUnderlyingType();
                            var rawValue = Convert.ChangeType(obj, underlyingType);

                            var (_, primitiveWriter) = _primitiveWriters[underlyingType];
                            primitiveWriter(w, rawValue);
                        }
                        break;
                    case Array arr:
                        {
                            if (withTypeInfo || withCache)
                            {
                                w.WriteByte((byte)TypeKind.Array);

                                if (withTypeInfo)
                                {
                                    w.WriteInt32(arr.Rank);
                                    Enumerable.Range(0, arr.Rank).ForEach(n => w.WriteInt32(arr.GetLength(n)));
                                    this.WriteTypeSignature(t.GetElementType(), w);
                                }
                            }

                            this.RegisterWrittenInstance(obj);

                            arr.OfType<object>().ForEach(o => this.WriteInstance(o, w));
                        }
                        break;
                    case ValueType value:
                    default:
                        {
                            if (withTypeInfo || withCache)
                            {
                                w.WriteByte((byte)TypeKind.Custom);

                                if (withTypeInfo)
                                    this.WriteTypeSignature(t, w);
                            }

                            this.RegisterWrittenInstance(obj);

                            t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                             .ForEach(f => this.WriteInstance(f.GetValue(obj), w));
                        }
                        break;

                }
            }
        }

        public object Deserialize(Stream stream)
        {
            var reader = new StreamBinaryReader(stream, false, true);
            var obj = this.ReadInstance(reader);
            _objById.Clear();
            return obj;
        }

        private void RegisterReadInstance(object obj)
        {
            _objById.Add(obj);
        }

        private T ReadInstance<T>(StreamBinaryReader r)
        {
            return (T)this.ReadInstance(r, typeof(T));
        }

        private Type ReadTypeSignature(StreamBinaryReader r)
        {
            var assemblyFullName = this.ReadInstance<string>(r);
            var typeFullName = this.ReadInstance<string>(r);

            var asm = Assembly.Load(assemblyFullName);
            var type = asm.GetType(typeFullName);

            return type;
        }

        private object ReadInstance(StreamBinaryReader r, Type type = null, bool withCache = true)
        {
            TypeKind kind;
            if (type == null || withCache)
            {
                kind = (TypeKind)r.ReadByte();
            }
            else
            {
                // TODO: what if real value to read is null?

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
                        obj = _primitiveReaders[kind](r);
                    }
                    break;
                case TypeKind.String:
                    {
                        obj = r.ReadString();
                        this.RegisterReadInstance(obj);
                    }
                    break;
                case TypeKind.Array:
                    {
                        Array arr;
                        int[] dimensions;

                        if (type == null)
                        {
                            var rank = r.ReadInt32();
                            dimensions = Enumerable.Range(0, rank).Select(n => r.ReadInt32()).ToArray();

                            var elementType = this.ReadTypeSignature(r);

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
                            arr.SetValue(this.ReadInstance(r), indexes);

                            for (int j = 0; j < dimensions.Length; j++)
                            {
                                indexes[j]++;
                                if (indexes[j] < dimensions[j])
                                    break;

                                indexes[j] = 0;
                            }
                        }
                    }
                    break;
                case TypeKind.Custom:
                    {
                        if (type == null)
                        {
                            type = this.ReadTypeSignature(r);
                        }

                        if (type.IsEnum)
                        {
                            var underlyingType = type.GetEnumUnderlyingType();
                            var primitiveReader = _primitiveReaders[_primitiveWriters[underlyingType].Item1];
                            var rawValue = primitiveReader(r);
                            obj = Enum.ToObject((Type)type, rawValue);
                        }
                        else
                        {
                            obj = FormatterServices.GetUninitializedObject((Type)type);
                            this.RegisterReadInstance(obj);

                            type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .ForEach(f => f.SetValue(obj, this.ReadInstance(r)));
                        }
                    }
                    break;
                case TypeKind.Ref:
                    {
                        var id = r.ReadInt32();
                        obj = _objById[id];
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return obj;
        }
    }
}
