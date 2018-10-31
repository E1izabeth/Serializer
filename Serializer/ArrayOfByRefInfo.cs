using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Serializer
{
    public class ArrayOfByRefInfo : SerializeInstanceInfo
    {
        private int _rank;
        private int[] _dimensions;
        private List<ISerializeInstanceInfo> _elementsInfo;
        private Type _arrayType;

        internal ArrayOfByRefInfo(ISerializationContext ctx)
            : base(ctx)
        {

        }

        internal ArrayOfByRefInfo(Array obj, ISerializationContext ctx)
            : base(obj, ctx)
        {
            _rank = obj.Rank;
            _arrayType = obj.GetType().GetElementType();
            _dimensions = new int[_rank];
            for (int i = 0; i < _rank; i++)
            {
                _dimensions[i] = obj.GetLength(i);
            }
            int[] ind = new int[_rank];
            int currDim = 0;
            int count = obj.Length;
            _elementsInfo = new List<ISerializeInstanceInfo>();
            for (int i = 0; i < count; i++)
            {
                ind = MySerializer.GetNewArrayIndexes(_rank, _dimensions, ind, ref currDim);
                _elementsInfo.Add(ctx.GetInstanceInfo(obj.GetValue(ind)));
            }
        }

        public override object Get(List<ISerializeInstanceInfo> instanceInfos)
        {
            var objType = Assembly.Load(_arrayType.Assembly.FullName).GetType(_arrayType.FullName);
            var arr = Array.CreateInstance(objType, _dimensions);
            this.Instance = arr;
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
                arr.SetValue(_elementsInfo[i].Get(instanceInfos), ind);
            }
            return arr;
        }

        public override void Read(Stream stream)
        {
            var assembly = stream.ReadString();
            var name = stream.ReadString();
            _arrayType = Assembly.Load(assembly).GetType(name);

            _rank = stream.ReadInt32();
            _dimensions = new int[_rank];
            int count = 1;
            for (int i = 0; i < _rank; i++)
            {
                _dimensions[i] = stream.ReadInt32();
                count *= _dimensions[i];
            }
            int currDim = 0;
            var ind = new int[_rank];
            _elementsInfo = new List<ISerializeInstanceInfo>();
            for (int i = 0; i < count; i++)
            {
                ind = MySerializer.GetNewArrayIndexes(_rank, _dimensions, ind, ref currDim);
                var info = this.Context.GetUninitializedTypeInfo((SerializeTypeEnum)stream.ReadByte());
                info.Read(stream);
                _elementsInfo.Add(info);
            }
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte((byte)SerializeTypeEnum.ArrayOfByref);
            stream.WriteString(_arrayType.Assembly.FullName);
            stream.WriteString(_arrayType.FullName);
            stream.WriteInt32(_rank);

            int count = 1;
            for (int i = 0; i < _rank; i++)
            {
                count *= _dimensions[i];
                stream.WriteInt32(_dimensions[i]);
            }

            foreach (var item in _elementsInfo)
            {
                if (item is null)
                {
                    new NullInfo().Write(stream);
                }
                else
                {
                    item.Write(stream);
                }
            }
        }
    }
}
