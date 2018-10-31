using System;
using System.Collections.Generic;
using System.IO;

namespace Serializer
{
    public class ArrayOfPrimitivesInfo : SerializeInstanceInfo
    {
        private SerializeTypeEnum _elementType;
        private int _rank;
        private int[] _dimensions;
        private object[] _values;

        internal ArrayOfPrimitivesInfo(ISerializationContext ctx)
            : base(ctx)
        {

        }

        internal ArrayOfPrimitivesInfo(Array obj, ISerializationContext ctx)
            : base(obj, ctx)
        {
            _elementType = SerializeTypes.GetTypeEnum(obj.GetType().GetElementType());
            _rank = obj.Rank;
            _dimensions = new int[_rank];
            for (int i = 0; i < _rank; i++)
            {
                _dimensions[i] = obj.GetLength(i);
            }
            int[] ind = new int[_rank];
            int currDim = 0;
            int count = 1;
            for (int i = 0; i < _rank; i++)
            {
                count *= _dimensions[i];
            }
            this._values = new object[count];
            for (int i = 0; i < count; i++)
            {
                ind = MySerializer.GetNewArrayIndexes(_rank, _dimensions, ind, ref currDim);
                _values[i] = obj.GetValue(ind);
            }
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte((byte)SerializeTypeEnum.ArrayOfPrimitives);
            stream.WriteByte((byte)_elementType);

            stream.WriteInt32(_rank);

            for (int i = 0; i < _rank; i++)
            {
                stream.WriteInt32(_dimensions[i]);
            }
            int[] ind = new int[_rank];
            int count = 1;
            for (int i = 0; i < _rank; i++)
            {
                count *= _dimensions[i];
            }
            for (int i = 0; i < count; i++)
            {
                stream.WritePrimitiveOrStringType(_values[i]);
            }
        }

        public override void Read(Stream stream)
        {
            _elementType = (SerializeTypeEnum)stream.ReadByte();
            var elementType = SerializeTypes.GetType(_elementType);

            _rank = stream.ReadInt32();
            _dimensions = new int[_rank];
            for (int i = 0; i < _rank; i++)
            {
                _dimensions[i] = stream.ReadInt32();
            }
            int currDim = 0;
            int count = 1;
            var ind = new int[_rank];
            for (int i = 0; i < _rank; i++)
            {
                count *= _dimensions[i];
            }
            _values = new object[count];
            for (int i = 0; i < count; i++)
            {
                ind = MySerializer.GetNewArrayIndexes(_rank, _dimensions, ind, ref currDim);
                _values[i] = stream.ReadPrimitiveOrStringType(elementType);
            }
        }

        public override object Get(List<ISerializeInstanceInfo> instanceInfos)
        {
            var arr = Array.CreateInstance(SerializeTypes.GetType(_elementType), _dimensions);
            this.Instance = arr;
            int count = 1;
            var ind = new int[_rank];
            for (int i = 0; i < _rank; i++)
            {
                count *= _dimensions[i];
            }
            int currDim = 0;
            for (int i = 0; i < count; i++)
            {
                ind = MySerializer.GetNewArrayIndexes(_rank, _dimensions, ind, ref currDim);
                arr.SetValue(_values[i], ind);
            }
            return arr;
        }
    }
}
