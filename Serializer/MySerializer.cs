using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
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
        ArrayOfStruct = 13,
        ArrayOfByref = 14,
        Custom = 15
    }

    public class MySerializer
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

        public static int[] GetNewIndexes(int rank, int[] dim, int[] indexes, ref int currDim)
        {
            var succ = false;
            while (currDim < rank && !succ)
            {
                if (currDim != rank)
                {
                    if (indexes[currDim] == dim[currDim] - 1)
                    {
                        indexes[currDim] = 0;
                        currDim++;
                    }
                    else
                    {
                        indexes[currDim]++;
                        succ = true;
                    }
                }
                else
                {
                    if (indexes[currDim] == dim[currDim])
                    {
                        succ = true;
                    }
                    else
                    {
                        indexes[currDim]++;
                        succ = true;
                    }
                }
            }
            if (succ)
            {
                currDim = 0;
            }
            return indexes;
        }


        public void Serialize(object obj, Stream stream)
        {
            if (obj is null)
            {
                stream.WriteByte((byte)SerializeTypeEnum.Null);
            }
            else
            {
                Type t = obj.GetType();
                if (t.IsArray)
                {
                    this.WriteArray(stream, obj);
                }
                else if (t.IsEnum)
                {
                    this.WriteEnum(stream, obj);
                }
                else if (t.IsPrimitive || t.Name == typeof(string).Name)
                {
                    stream.WriteByte((byte)GetTypeEnum(t));
                    stream.WritePrimitiveOrStringType(obj);
                }
                else
                {
                    var fieldsInfo = t.GetFields(BindingFlags.Public | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                    stream.WriteByte((byte)SerializeTypeEnum.Custom);
                    stream.WritePrimitiveOrStringType(t.Assembly.FullName.ToString());
                    stream.WritePrimitiveOrStringType(t.FullName.ToString());
                    stream.WriteInt32(fieldsInfo.Count());
                    foreach (var f in fieldsInfo)
                    {
                        this.Serialize(f.GetValue(obj), stream);
                    }
                }
            }
        }

        private void WriteEnum(Stream stream, object obj)
        {
            stream.WriteByte((byte)SerializeTypeEnum.Enum);
            var t = obj.GetType();
            var rawValue = Convert.ChangeType(obj, t.GetEnumUnderlyingType());
            stream.WritePrimitiveOrStringType(t.Assembly.FullName.ToString());
            stream.WritePrimitiveOrStringType(t.FullName.ToString());

            stream.WritePrimitiveOrStringType(rawValue);
        }

        public object Deserialize(Stream stream)
        {
            object o = null;
            SerializeTypeEnum t = (SerializeTypeEnum)stream.ReadByte();

            switch (t)
            {
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
                        var type = _typeMap.FirstOrDefault(s => s.Value == t).Key;
                        o = stream.ReadPrimitiveOrStringType(type);
                    }
                    break;
                case SerializeTypeEnum.Enum:
                    {
                        var assemblyName = stream.ReadString();
                        var fullname = stream.ReadString();
                        var enumType = Assembly.Load(assemblyName).GetType(fullname);
                        
                        o = Enum.ToObject(enumType, stream.ReadPrimitiveOrStringType(enumType.GetEnumUnderlyingType()));
                    }
                    break;
                case SerializeTypeEnum.ArrayOfByref:
                    {
                        o = this.ReadArrayOfByRef(stream);
                    }
                    break;
                case SerializeTypeEnum.ArrayOfStruct:
                    {
                        o = this.ReadArrayOfStruct(stream);
                    }
                    break;
                case SerializeTypeEnum.Custom:
                    {
                        var assemblyName = stream.ReadString();
                        var fullname = stream.ReadString();
                        var fcount = stream.ReadInt32();
                        var flist = new List<object>();
                        for (int i = 0; i < fcount; i++)
                        {
                            flist.Add(this.Deserialize(stream));
                        }
                        var objType = Assembly.Load(assemblyName).GetType(fullname);
                        o = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(objType);
                        var fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                        int j = 0;
                        foreach (var f in fields)
                        {
                            f.SetValue(o, flist[j]);
                            ++j;
                        }
                    }
                    break;
                case SerializeTypeEnum.Null:
                    {
                        o = null;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return o;
        }

        private void WriteArray(Stream stream, object obj)
        {
            var type = GetTypeEnum(obj.GetType().GetElementType());
            if (type == SerializeTypeEnum.Custom)
            {
                stream.WriteByte((byte)SerializeTypeEnum.ArrayOfByref);
            }
            else
            {
                stream.WriteByte((byte)SerializeTypeEnum.ArrayOfStruct);
                stream.WriteByte((byte)GetTypeEnum(obj.GetType().GetElementType()));
            }
            Array arr = (Array)obj;
            var rank = obj.GetType().GetArrayRank();
            stream.WriteInt32(rank);
            var dim = new int[rank];
            for (int i = 0; i < rank; i++)
            {
                dim[i] = (obj as Array).GetLength(i);
                stream.WriteInt32((obj as Array).GetLength(i));
            }
            int[] ind = new int[rank];
            int currDim = 0;
            int count = 1;
            for (int i = 0; i < rank; i++)
            {
                count *= dim[i];
            }
            for (int i = 0; i < count; i++)
            {
                ind = GetNewIndexes(rank, dim, ind, ref currDim);
                if (type == SerializeTypeEnum.Custom)
                {
                    this.Serialize((obj as Array).GetValue(ind), stream);
                }
                else
                {
                    stream.WritePrimitiveOrStringType((obj as Array).GetValue(ind));
                }
            }
        }

        private object ReadArrayOfStruct(Stream stream)
        {
            SerializeTypeEnum t = (SerializeTypeEnum)stream.ReadByte();
            var type = _typeMap.FirstOrDefault(s => s.Value == t).Key;

            var rank = stream.ReadInt32();
            var dim = new int[rank];
            for (int i = 0; i < rank; i++)
            {
                dim[i] = stream.ReadInt32();
            }
            var arr = Array.CreateInstance(type, dim);
            int currDim = 0;
            int count = 1;
            var ind = new int[rank];
            for (int i = 0; i < rank; i++)
            {
                count *= dim[i];
            }
            for (int i = 0; i < count; i++)
            {
                ind = GetNewIndexes(rank, dim, ind, ref currDim);
                var val = stream.ReadPrimitiveOrStringType(type);
                arr.SetValue(val, ind);
            }
            return arr;
        }

        private object ReadArrayOfByRef(Stream stream)
        {
            var rank = stream.ReadInt32();
            var dim = new int[rank];
            for (int i = 0; i < rank; i++)
            {
                dim[i] = stream.ReadInt32();
            }
            var arr = Array.CreateInstance(typeof(object), dim);
            int currDim = 0;
            int count = 1;
            var ind = new int[rank];
            for (int i = 0; i < rank; i++)
            {
                count *= dim[i];
            }
            for (int i = 0; i < count; i++)
            {
                ind = GetNewIndexes(rank, dim, ind, ref currDim);
                object val;
                val = this.Deserialize(stream);
                arr.SetValue(val, ind);
            }
            return arr;
        }

    }
}
