using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Serializer.SerializeTypes;

namespace Serializer
{
    interface ISerializationContext
    {
        int KnownInstancesCount { get; }

        SerializeInstanceInfo GetInstanceInfo(object obj);
        void RegisterInstanceInfo(ISerializeInstanceInfo obj);
    }


    public class MySerializer : ISerializationContext
    {
        private List<ISerializeInstanceInfo> _serializedInstances;

        int ISerializationContext.KnownInstancesCount { get { return _serializedInstances.Count; } }

        public MySerializer()
        {
            _serializedInstances = new List<ISerializeInstanceInfo>();
        }

        public void Serialize(object obj, Stream stream)
        {
            if (obj is null)
            {
                stream.WriteByte((byte)SerializeTypeEnum.Null);
            }
            else
            {
                var info = this.GetInstanceInfo(obj);
                info.Write(stream);
            }
        }

        void ISerializationContext.RegisterInstanceInfo(ISerializeInstanceInfo obj)
        {
            _serializedInstances.Add(obj);
        }

        SerializeInstanceInfo ISerializationContext.GetInstanceInfo(object obj)
        {
            var index = _serializedInstances.FindIndex(t => t.Instance == obj);

            if (index >= 0)
            {
                return new SerializedYetInfo() { numberInList = index };
            }
            else
            {
                return this.GetInstanceInfo(obj);
            }
        }

        private SerializeInstanceInfo GetInstanceInfo(object obj)
        {
            if (obj is null)
                return null;

            var t = obj.GetType();
            SerializeInstanceInfo info;

            if (t.IsPrimitive || t == typeof(string))
            {
                info = new PrimitiveTypeInfo(obj, this);
            }
            else if (t.IsEnum)
            {
                info = new EnumInfo(obj);
            }
            else if (t.IsArray)
            {
                var arr = obj as Array;

                if (SerializeTypes.GetTypeEnum(t.GetElementType()) != SerializeTypeEnum.Custom)
                {
                    info = new ArrayOfPrimitivesInfo(arr, this);
                }
                else
                {
                    info = new ArrayOfByRefInfo(arr, this);
                }
            }
            else
            {
                info = new CustomInfo(obj, this);
            }

            // _serializedInstances.Add(info);
            return info;
        }

        public object Deserialize(Stream stream)
        {
            object o = null;
            SerializeTypeEnum t = (SerializeTypeEnum)stream.ReadByte();

            if (t is SerializeTypeEnum.Null)
            {
                o = null;
            }
            else
            {
                var info = SerializeInstanceInfo.GetUninitializedTypeInfo(t);
                info.Read(stream);
                o = info.Get(_serializedInstances);
            }

            return o;
        }

        public static int[] GetNewArrayIndexes(int rank, int[] dim, int[] indexes, ref int currDim)
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
    }
}
