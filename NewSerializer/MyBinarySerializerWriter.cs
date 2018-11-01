using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewSerializer
{
    class MyBinarySerializerWriter : MyBinarySerializerContext
    {
        readonly Dictionary<object, int> _idByObj = new Dictionary<object, int>();

        readonly StreamBinaryWriter _writer;

        public MyBinarySerializerWriter(StreamBinaryWriter writer)
        {
            _writer = writer;
        }

        private void RegisterWrittenInstance(object obj)
        {
            _idByObj.Add(obj, _idByObj.Count);
        }

        private void WriteTypeSignature(Type type)
        {
            this.WriteInstance(type.Assembly.FullName, false);
            this.WriteInstance(type.FullName, false);
        }

        public void WriteInstance(object obj, bool withTypeInfo = true, bool withCache = true)
        {
            Type t = obj == null ? null : obj.GetType();

            if (obj != null && withCache && _idByObj.TryGetValue(obj, out int id))
            {
                _writer.WriteByte((byte)TypeKind.Ref);
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
                    case string str:
                        {
                            if (withTypeInfo || withCache)
                                _writer.WriteByte((byte)TypeKind.String);

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
                    this.WriteTypeSignature(t);
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
                    _writer.WriteInt32(arr.Rank);
                    Enumerable.Range(0, arr.Rank).ForEach(n => _writer.WriteInt32(arr.GetLength(n)));
                    this.WriteTypeSignature(t.GetElementType());
                }
            }

            this.RegisterWrittenInstance(obj);

            arr.OfType<object>().ForEach(o => this.WriteInstance(o));
        }

        private void WriteCustomTypeInstanceImpl(object obj, bool withTypeInfo, bool withCache, Type t)
        {
            if (withTypeInfo || withCache)
            {
                _writer.WriteByte((byte)TypeKind.Custom);

                if (withTypeInfo)
                    this.WriteTypeSignature(t);
            }

            this.RegisterWrittenInstance(obj);

            t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             .ForEach(f => this.WriteInstance(f.GetValue(obj)));
        }
    }
}
