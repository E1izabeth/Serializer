using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public static class StreamExtensions
    {
        public static byte ReadByte(this Stream s)
        {
            return (byte)s.ReadByte();
        }

        public static UInt32 ReadUInt32(this Stream s)
        {
            var temp = new byte[4];
            s.Read(temp, 0, 4);
            return BitConverter.ToUInt32(temp, 0);
        }

        public static Int32 ReadInt32(this Stream s)
        {
            var temp = new byte[4];
            s.Read(temp, 0, 4);
            return BitConverter.ToInt32(temp, 0);
        }

        public static Int16 ReadInt16(this Stream s)
        {
            var temp = new byte[2];
            s.Read(temp, 0, 2);
            return BitConverter.ToInt16(temp, 0);
        }

        public static UInt16 ReadUInt16(this Stream s)
        {
            var temp = new byte[2];
            s.Read(temp, 0, 2);
            return BitConverter.ToUInt16(temp, 0);
        }

        public static UInt64 ReadUInt64(this Stream s)
        {
            var temp = new byte[8];
            s.Read(temp, 0, 8);
            return BitConverter.ToUInt64(temp, 0);
        }

        public static Int64 ReadInt64(this Stream s)
        {
            var temp = new byte[8];
            s.Read(temp, 0, 8);
            return BitConverter.ToInt64(temp, 0);
        }

        public static float ReadFloat(this Stream s)
        {
            var temp = new byte[4];
            s.Read(temp, 0, 4);
            return BitConverter.ToSingle(temp, 0);
        }

        public static double ReadDouble(this Stream s)
        {
            var temp = new byte[8];
            s.Read(temp, 0, 8);
            return BitConverter.ToDouble(temp, 0);
        }

        public static string ReadString(this Stream s, int count)
        {
            byte[] temp = new byte[count];
            s.Read(temp, 0, count);
            return Encoding.UTF8.GetString(temp);
        }

        public static void WriteInt32(this Stream s, int v)
        {
            s.Write(BitConverter.GetBytes(v), 0, 4);
        }

        public static void WriteUInt32(this Stream s, uint v)
        {
            s.Write(BitConverter.GetBytes(v), 0, 4);
        }

        public static void WriteInt16(this Stream s, Int16 v)
        {
            s.Write(BitConverter.GetBytes(v), 0, 2);
        }
        public static void WriteUInt16(this Stream s, UInt16 v)
        {
            s.Write(BitConverter.GetBytes(v), 0, 2);
        }

        public static void WriteInt64(this Stream s, Int64 v)
        {
            s.Write(BitConverter.GetBytes(v), 0, 8);
        }

        public static void WriteUInt64(this Stream s, UInt64 v)
        {
            s.Write(BitConverter.GetBytes(v), 0, 8);
        }

        public static void WriteFloat(this Stream s, float v)
        {
            s.Write(BitConverter.GetBytes(v), 0, 2);
        }

        public static void WriteDouble(this Stream s, double v)
        {
            s.Write(BitConverter.GetBytes(v), 0, 4);
        }

        public static int WriteString(this Stream s, string str)
        {
            if (str == null)
            {
                s.WriteInt32(-1);
                return 0;
            }
            else if (str == "")
            {
                s.WriteInt32(0);
                return 0;
            }

            var bytes = Encoding.UTF8.GetBytes(str);
            s.WriteInt32(bytes.Length);
            s.Write(bytes, 0, str.Length);
            return bytes.Length;
        }

        public static int WriteStringWithLength(this Stream s, string str)
        {
            if (str == null)
            {
                s.WriteInt32(-1);
                return 0;
            }
            else if (str == "")
            {
                s.WriteInt32(0);
                return 0;
            }

            var bytes = Encoding.UTF8.GetBytes(str);
            var len = bytes.Length;
            s.WriteInt32(len);
            s.Write(bytes, 0, len);
            return len;
        }

        public static void WriteInt16Array(this Stream s, short[] inputData)
        {
            s.WriteInt32(inputData.Length);
            foreach (short element in inputData)
            {
                s.WriteInt16(element);
            }
        }

        public static void WriteInt32Array(this Stream s, int[] inputData)
        {
            s.WriteInt32(inputData.Length);
            foreach (int element in inputData)
            {
               s.WriteInt32(element);
            }
        }

        public static void WriteLongArray(this Stream s, long[] inputData)
        {
            s.WriteInt32(inputData.Length);
            foreach (long element in inputData)
            {
                s.WriteInt64(element);
            }
        }

        public static void WriteUInt16Array(this Stream s, ushort[] inputData)
        {
            s.WriteInt32(inputData.Length);
            foreach (ushort element in inputData)
            {
                s.WriteUInt16(element);
            }
        }

        public static void WriteUInt32Array(this Stream s, uint[] inputData)
        {
            s.WriteInt32(inputData.Length);
            foreach (uint element in inputData)
            {
                s.WriteUInt32(element);
            }
        }

        public static void WriteUInt64Array(this Stream s, ulong[] inputData)
        {
            s.WriteInt32(inputData.Length);
            foreach (ulong element in inputData)
            {
                s.WriteUInt64(element);
            }
        }

        public static void WriteFloatArray(this Stream s, float[] inputData)
        {
            s.WriteInt32(inputData.Length);
            foreach (float element in inputData)
            {
                s.WriteFloat(element);
            }
        }

        public static void WriteDoubleArray(this Stream s, double[] inputData)
        {
            s.WriteInt32(inputData.Length);
            foreach (double element in inputData)
            {
                s.WriteDouble(element);
            }
        }

        public static void WriteBoolArray(this Stream s, bool[] inputData)
        {
            s.WriteInt32(inputData.Length);
            foreach (bool element in inputData)
            {
                s.WriteBool(element);
            }
        }

        public static void WriteByteArray(this Stream s, byte[] inputData)
        {
            s.WriteInt32(inputData.Length);
            s.Write(inputData, 0, inputData.Length);
        }

        public static void WriteStringArray(this Stream s, string[] inputData)
        {
            s.WriteInt32(inputData.Length);
            foreach (string element in inputData)
            {
                s.WriteString(element);
            }
        }


        public static string ReadString(this Stream s)
        {
            var len = s.ReadInt32();
            if (len == 0) return "";
            if (len == -1) return null;
            var temp = new byte[len];
            s.Read(temp, 0, len);
            return Encoding.UTF8.GetString(temp);
        }

        public static void WriteBool(this Stream s, bool v)
        {
            s.WriteByte((byte)(v ? 1 : 0));
        }

        public static bool ReadBool(this Stream s)
        {
            return s.ReadByte() == 1;
        }
    }
}
