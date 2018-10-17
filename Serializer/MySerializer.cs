using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public class MySerializer
    {
        public enum SerializeTypeEnum : byte
        {
            None = 0,
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
            Array = 13,
            Custom = 14,
        }

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
            { typeof(Enum), SerializeTypeEnum.Enum},
            { typeof(Array), SerializeTypeEnum.Array}
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


        public byte[] Serialize(object obj)
        {
            var stream = new MemoryStream();
            Type t = obj.GetType();
            if (t.IsArray)
            {
                stream.WriteByte((byte)(byte)GetTypeEnum(t.BaseType));
                stream.WriteByte((byte)GetTypeEnum(t.GetElementType()));
                var len = (int)t.GetProperty("Length").GetValue(obj);
                stream.WriteInt32(len);
                var rank = t.GetArrayRank();
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
                    WritePrimitiveOrStringType(stream, (obj as Array).GetValue(ind));
                }
            }
            else if (t.IsEnum)
            {
                WriteEnumType(stream, t);
            }
            else
            {
                stream.WriteByte((byte)GetTypeEnum(t));

                if (t.IsPrimitive || t.Name == typeof(string).Name)
                {
                    WritePrimitiveOrStringType(stream, obj);
                }
            }
            return stream.ToArray();
        }

        public object Deserialize(byte[] b)
        {
            object o = null;
            var stream = new MemoryStream(b);
            SerializeTypeEnum t = (SerializeTypeEnum)stream.ReadByte();
            Type type = _typeMap.FirstOrDefault(s => s.Value == t).Key;

            if (type.IsPrimitive || type.Name == typeof(string).Name)
            {
                o = ReadPrimitiveOrStringType(stream, type);
            }
            else if (type.IsEnum || typeof(Enum).Name == type.Name)
            {
                o = ReadEnumType(stream);
            }
            else if (type.IsArray || typeof(Array).Name == type.Name)
            {
                o = ReadArray(stream);
            }

            return o;
        }

        private static object ReadArray(MemoryStream stream)
        {
            SerializeTypeEnum t = (SerializeTypeEnum)stream.ReadByte();
            Type type = _typeMap.FirstOrDefault(s => s.Value == t).Key;
            var len = stream.ReadInt32();
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
                var val = ReadPrimitiveOrStringType(stream, type);
                arr.SetValue(val, ind);
            }
            return arr;
        }

        private static object ReadPrimitiveOrStringType(MemoryStream stream, Type type)
        {
            if (type.Name == typeof(Byte).Name)
            {
                return stream.ReadByte();
            }
            else if (type.Name == typeof(Boolean).Name)
            {
                return stream.ReadBool();
            }
            else if (type.Name == typeof(short).Name)
            {
                return stream.ReadInt16();
            }
            else if (type.Name == typeof(ushort).Name)
            {
                return stream.ReadUInt16();
            }
            else if (type.Name == typeof(int).Name)
            {
                return stream.ReadInt32();
            }
            else if (type.Name == typeof(uint).Name)
            {
                return stream.ReadUInt32();
            }
            else if (type.Name == typeof(long).Name)
            {
                return stream.ReadInt64();
            }
            else if (type.Name == typeof(ulong).Name)
            {
                return stream.ReadUInt64();
            }
            else if (type.Name == typeof(float).Name)
            {
                return stream.ReadFloat();
            }
            else if (type.Name == typeof(double).Name)
            {
                return stream.ReadDouble();
            }
            else if (type.Name == typeof(string).Name)
            {
                return stream.ReadString();
            }
            else return null;
        }


        private static void WritePrimitiveOrStringType(MemoryStream stream, object obj)
        {
            Type t = obj.GetType();

            if (t.Name == typeof(Byte).Name)
            {
                stream.WriteByte((byte)obj);
            }
            else if (t.Name == typeof(Boolean).Name)
            {
                stream.WriteBool((bool)obj);
            }
            else if (t.Name == typeof(short).Name)
            {
                stream.WriteInt16((short)obj);
            }
            else if (t.Name == typeof(ushort).Name)
            {
                stream.WriteUInt16((ushort)obj);
            }
            else if (t.Name == typeof(int).Name)
            {
                stream.WriteInt32((int)obj);
            }
            else if (t.Name == typeof(uint).Name)
            {
                stream.WriteUInt32((uint)obj);
            }
            else if (t.Name == typeof(long).Name)
            {
                stream.WriteInt64((long)obj);
            }
            else if (t.Name == typeof(ulong).Name)
            {
                stream.WriteUInt64((ulong)obj);
            }
            else if (t.Name == typeof(float).Name)
            {
                stream.WriteFloat((float)obj);
            }
            else if (t.Name == typeof(double).Name)
            {
                stream.WriteDouble((double)obj);
            }
            else if (t.Name == typeof(string).Name)
            {
                stream.WriteStringWithLength((string)obj);
            }


        }
        private static object ReadEnumType(MemoryStream stream)
        {
            var name = stream.ReadString();
            SerializeTypeEnum ut = (SerializeTypeEnum)stream.ReadByte();
            Type underType = _typeMap.FirstOrDefault(s => s.Value == ut).Key;
            var len = stream.ReadInt32();
            var names = new string[len];
            for (int i = 0; i < len; i++)
            {
                names[i] = (string)ReadPrimitiveOrStringType(stream, typeof(String));
            }
            AppDomain currentDomain = AppDomain.CurrentDomain;
            AssemblyName aName = new AssemblyName("TempAssembly");
            AssemblyBuilder ab = currentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");

            EnumBuilder eb = mb.DefineEnum(name, TypeAttributes.Public, underType);
            for (int i = 0; i < len; i++)
            {
                eb.DefineLiteral(names[i].ToString(), (byte)1);
            }

            Type finished = eb.CreateType();
            ab.Save(aName.Name + ".dll");
            return finished;
        }

        private static void WriteEnumType(Stream stream, Type t)
        {
            stream.WriteByte((byte)GetTypeEnum(t.BaseType));
            stream.WriteStringWithLength(t.Name);
            stream.WriteByte((byte)GetTypeEnum(t.GetEnumUnderlyingType()));
            var names = t.GetEnumNames();
            stream.WriteInt32(names.Length);
            for (int i = 0; i < names.Length; i++)
            {
                stream.WriteStringWithLength(names[i]);
            }
        }
    }
}
