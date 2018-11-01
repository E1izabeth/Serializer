using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewSerializer
{
    internal class MyBinarySerializerWriter : MyBinarySerializerContext
    {
        private readonly Dictionary<object, int> _idByObj = new Dictionary<object, int>();
        private readonly Dictionary<Type, int> _idByType = new Dictionary<Type, int>();
        private readonly StreamBinaryWriter _writer;

        public MyBinarySerializerWriter(StreamBinaryWriter writer)
        {
            _writer = writer;
        }

        private void RegisterWrittenInstance(object obj)
        {
            if (obj is Type t)
            {
                if (!_idByType.ContainsKey(t))
                    _idByType.Add(t, _idByType.Count);
            }
            else
            {
                _idByObj.Add(obj, _idByObj.Count);
            }

            File.WriteAllLines(@"t1.txt", _idByObj.Select(kv => $"{kv.Value}: {kv.Key}"));
            File.WriteAllLines(@"t1t.txt", _idByType.Select(kv => $"{kv.Value}: {kv.Key}"));
        }

        private void WriteTypeSignature(Type type)
        {
            var pos = _writer.Position;

            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                _writer.WriteBoolean(true);
                this.WriteInstance(type.GetGenericArguments(), false, false);
                this.WriteInstance(type.GetGenericTypeDefinition(), false);

                this.RegisterWrittenInstance(type);
            }
            else
            {
                this.RegisterWrittenInstance(type);

                _writer.WriteBoolean(false);
                this.WriteInstance(type.Assembly.FullName, false);

                var parts = new List<string>();
                parts.Add(type.Name);

                while (type.IsNested)
                {
                    type = type.DeclaringType;
                    parts.Add(type.Name);
                }

                this.WriteInstance(type.Namespace, false);
                this.WriteInstance(parts.AsEnumerable().Reverse().ToArray(), false);
            }
            // Console.WriteLine($"emitting {_writer.Position - pos} bytes for {type}");
        }

        public void WriteInstance(object obj, bool withTypeInfo = true, bool withCache = true)
        {
            Type t = obj == null ? null : obj.GetType();
            Console.WriteLine("writing " + (obj == null ? "<NULL>" : obj.GetType().FullName) + " " + (obj == null || obj.ToString() == obj.GetType().ToString() ? "" : obj.ToString()));

            int id;
            if (obj != null && withCache && (_idByObj.TryGetValue(obj, out id) || (obj is Type type && _idByType.TryGetValue(type, out id))))
            {
                _writer.WriteByte((byte)(obj is Type ? TypeKind.TypeRef : TypeKind.Ref));
                _writer.WriteInt32(id);
            }
            else
            {
                switch (obj)
                {
                    case null:
                        {
                            if (withTypeInfo || withCache)
                                _writer.WriteByte((byte)TypeKind.Null);
                            else
                                throw new InvalidOperationException(); // can't recognize null without type kind token during deserialization
                        }
                        break;
                    case object value when t.IsPrimitive:
                        {
                            var (typeKind, primitiveWriter) = _primitiveWriters[t];

                            if (withTypeInfo || withCache)
                                _writer.WriteByte((byte)typeKind);

                            primitiveWriter(_writer, obj);
                        }
                        break;
                    case Type typeRef:
                        {
                            if (withTypeInfo || withCache)
                                _writer.WriteByte((byte)TypeKind.Type);

                            this.WriteTypeSignature(typeRef);
                        }
                        break;
                    case string str:
                        {
                            if (withTypeInfo || withCache)
                                _writer.WriteByte((byte)TypeKind.String);

                            if (withCache)
                                this.RegisterWrittenInstance(obj);

                            _writer.WriteString(str);
                        }
                        break;
                    case Enum value:
                        {
                            this.WriteEnumInstanceImpl(obj, withTypeInfo, withCache, t);
                        }
                        break;
                    case Array arr:
                        {
                            this.WriteArrayInstanceImpl(obj, withTypeInfo, withCache, t, arr);
                        }
                        break;
                    case ValueType value:
                    default:
                        {
                            this.WriteCustomTypeInstanceImpl(obj, withTypeInfo, withCache, t);
                        }
                        break;

                }
            }
        }

        private void WriteEnumInstanceImpl(object obj, bool withTypeInfo, bool withCache, Type t)
        {
            if (withTypeInfo || withCache)
            {
                _writer.WriteByte((byte)TypeKind.Custom);

                if (withTypeInfo)
                    this.WriteInstance(t, false);
            }

            var underlyingType = t.GetEnumUnderlyingType();
            var rawValue = Convert.ChangeType(obj, underlyingType);

            var (_, primitiveWriter) = _primitiveWriters[underlyingType];
            primitiveWriter(_writer, rawValue);
        }

        private void WriteArrayInstanceImpl(object obj, bool withTypeInfo, bool withCache, Type t, Array arr)
        {
            if (withTypeInfo || withCache)
            {
                _writer.WriteByte((byte)TypeKind.Array);

                if (withTypeInfo)
                {
                    this.WriteInstance(t.GetElementType(), false);
                    _writer.WriteInt32(arr.Rank);
                }
            }

            if (withCache)
                this.RegisterWrittenInstance(obj);

            Enumerable.Range(0, arr.Rank).ForEach(n => _writer.WriteInt32(arr.GetLength(n)));
            arr.OfType<object>().ForEach(o => this.WriteInstance(o));
        }

        private void WriteCustomTypeInstanceImpl(object obj, bool withTypeInfo, bool withCache, Type t)
        {
            if (withTypeInfo || withCache)
            {
                _writer.WriteByte((byte)TypeKind.Custom);

                if (withTypeInfo)
                    this.WriteInstance(t, false);
            }

            if (withCache)
                this.RegisterWrittenInstance(obj);

            t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             .ForEach(f => this.WriteInstance(f.GetValue(obj)));
        }
    }
}
