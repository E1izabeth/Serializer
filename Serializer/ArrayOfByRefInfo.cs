using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Serializer.SerializeTypes;

namespace Serializer
{
    public class ArrayOfByRefInfo : SerializeTypeInfo
    {
        private int _rank;
        private int[] _dimensions;
        private List<SerializeTypeInfo> _elementsInfo;

        public ArrayOfByRefInfo()
        {

        }

        public ArrayOfByRefInfo(object obj)
        {
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
            _elementsInfo = new List<SerializeTypeInfo>();
            for (int i = 0; i < count; i++)
            {
                ind = MySerializer.GetNewArrayIndexes(_rank, _dimensions, ind, ref currDim);
                _elementsInfo.Add(GetTypeInfo((obj as Array).GetValue(ind)));
            }
        }

        public override ISerializeTypeInfo Apply(ITypesVisitor visitor, object obj)
        {
            return visitor.GetArrayOfByRefInfo(obj);
        }

        public override object Get()
        {
            var arr = Array.CreateInstance(typeof(object), _dimensions);
            int currDim = 0;
            int count = 1;
            var ind = new int[_rank];
            for (int i = 0; i < _rank; i++)
            {
                count *= _dimensions[i];
            }
            for (int i = 0; i < count; i++)
            {
                ind = MySerializer.GetNewArrayIndexes(_rank, _dimensions, ind, ref currDim);
                arr.SetValue(_elementsInfo[i].Get(), ind);
            }
            return arr;
        }

        public override void Read(Stream stream)
        {
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
            _elementsInfo = new List<SerializeTypeInfo>();
            for (int i = 0; i < count; i++)
            {
                ind = MySerializer.GetNewArrayIndexes(_rank, _dimensions, ind, ref currDim);
                var info = GetTypeInfo((SerializeTypeEnum)stream.ReadByte());
                info.Read(stream);
                _elementsInfo.Add(info);
            }
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte((byte)SerializeTypeEnum.ArrayOfByref);
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
            foreach (var item in _elementsInfo)
            {
                if (item is null)
                {
                    (new NullInfo()).Write(stream);
                }
                else
                {
                    item.Write(stream);
                }
            }
        }
    }
}
