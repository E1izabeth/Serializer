using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public class ArrayOfStructInfo : SerializeTypeInfo
    {
        private SerializeTypes.SerializeTypeEnum _elementType;
        private int _rank;
        private int[] _dimensions;
        private dynamic[] _values;

        public ArrayOfStructInfo()
        {

        }

        public ArrayOfStructInfo(object obj)
        {
            _elementType = SerializeTypes.GetTypeEnum(obj.GetType().GetElementType());
            _rank = obj.GetType().GetArrayRank();
            _dimensions = new int[_rank];
            for (int i = 0; i < _rank; i++)
            {
                _dimensions[i] = (obj as Array).GetLength(i);
            }
            int[] ind = new int[_rank];
            int currDim = 0;
            int count = 1;
            for (int i = 0; i < _rank; i++)
            {
                count *= _dimensions[i];
            }
            this._values = new dynamic[count];
            for (int i = 0; i < count; i++)
            {
                ind = MySerializer.GetNewArrayIndexes(_rank, _dimensions, ind, ref currDim);
                _values[i] = (obj as Array).GetValue(ind);
            }
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte((byte)SerializeTypes.SerializeTypeEnum.ArrayOfStruct);
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
                stream.WritePrimitiveOrStringType((object)_values[i]);
            }
        }

        public override void Read(Stream stream)
        {
            _elementType = (SerializeTypes.SerializeTypeEnum)stream.ReadByte();
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
            _values = new dynamic[count];
            for (int i = 0; i < count; i++)
            {
                ind = MySerializer.GetNewArrayIndexes(_rank, _dimensions, ind, ref currDim);
                _values[i] = stream.ReadPrimitiveOrStringType(elementType);
            }
        }

        public override object Get()
        {
            var arr = Array.CreateInstance(SerializeTypes.GetType(_elementType), _dimensions);
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

        public override ISerializeTypeInfo Apply(ITypesVisitor visitor, object obj)
        {
            return visitor.GetArrayOfStructInfo(obj);
        }
    }
}
