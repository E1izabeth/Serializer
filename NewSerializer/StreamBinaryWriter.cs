using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSerializer
{
    public class StreamBinaryWriter 
    {
        byte[] _buffer = new byte[16];
        Stream _stream;
        bool _ownStream;
        bool _packedInts;

        public StreamBinaryWriter(Stream stream, bool ownStream = false, bool packedInts = false)
        {
            //var ss = new StatsStream(stream);
            //ss.OnWrite += (sender, ea) => {
            //    if (ss.BytesWasWritten > 0x90)
            //        Console.WriteLine();
            //};

            _stream = stream;
            _ownStream = ownStream;
            _packedInts = packedInts;
        }

        public long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }

        public void WriteBytes(byte[] buffer, int index, int count)
        {
            _stream.Write(buffer, index, count);
        }

        public void WriteChars(char[] chars, int index, int count)
        {
            var data = Encoding.UTF8.GetBytes(chars, index, count);
            _stream.Write(data, 0, data.Length);
        }

        public void WriteBoolean(bool value)
        {
            _stream.Write(_buffer, 0, _buffer.PackUInt8(value ? ((byte)1) : ((byte)0), 0));
        }

        public void WriteSByte(sbyte value)
        {
            _stream.Write(_buffer, 0, _buffer.PackInt8(value, 0));
        }

        public void WriteByteArray(byte[] buffer)
        {
            this.WriteInt32(buffer.Length);
            this.WriteBytes(buffer, 0, buffer.Length);
        }

        public void WriteCharArray(char[] chars)
        {
            var data = Encoding.UTF8.GetBytes(chars);
            this.WriteInt32(data.Length);
            _stream.Write(data, 0, data.Length);
        }

        public void WriteByte(byte value)
        {
            _stream.Write(_buffer, 0, _buffer.PackUInt8(value, 0));
        }

        public void WriteChar(char ch)
        {
            _stream.Write(_buffer, 0, _buffer.PackUInt16((ushort)ch, 0));
        }

        public void WriteDecimal(decimal value)
        {
            _stream.Write(_buffer, 0, _buffer.PackDecimal(value, 0));
        }

        public void WriteDouble(double value)
        {
            _stream.Write(_buffer, 0, _buffer.PackDouble(value, 0));
        }

        public void WriteInt16(short value)
        {
            _stream.Write(_buffer, 0, _buffer.PackInt16(value, 0));
        }

        public void WriteInt32(int value)
        {
            if (_packedInts)
            {
                this.Write7BitEncodedInt(value);
            }
            else
            {
                _stream.Write(_buffer, 0, _buffer.PackInt32(value, 0));
            }
        }

        public void WriteInt64(long value)
        {
            _stream.Write(_buffer, 0, _buffer.PackInt64(value, 0));
        }

        public void WriteSingle(float value)
        {
            _stream.Write(_buffer, 0, _buffer.PackSingle(value, 0));
        }

        public void WriteString(string value)
        {
            this.WriteByteArray(Encoding.UTF8.GetBytes(value));
        }

        public void WriteUInt16(ushort value)
        {
            _stream.Write(_buffer, 0, _buffer.PackUInt16(value, 0));
        }

        public void WriteUInt32(uint value)
        {
            _stream.Write(_buffer, 0, _buffer.PackUInt32(value, 0));
        }

        public void WriteUInt64(ulong value)
        {
            _stream.Write(_buffer, 0, _buffer.PackUInt64(value, 0));
        }

        public long Seek(int offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public void Flush()
        {
            _stream.Flush();
        }

        public void Close()
        {
            _stream.Close();
        }

        public void Dispose()
        {
            if (_ownStream)
                _stream.Dispose();
        }

        protected void Write7BitEncodedInt(int value)
        {
            uint num = (uint)value;
            while (num >= 0x80)
            {
                this.WriteByte((byte)(num | 0x80));
                num = num >> 7;
            }
            this.WriteByte((byte)num);
        }
    }
}
