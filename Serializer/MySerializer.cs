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
        int RegisterInstanceInfo(SerializeInstanceInfo obj);
        SerializeInstanceInfo GetUninitializedTypeInfo(SerializeTypeEnum serializeTypeEnum);
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
            _serializedInstances.Clear();

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

        int ISerializationContext.RegisterInstanceInfo(SerializeInstanceInfo obj)
        {
            var id = _serializedInstances.Count;
            _serializedInstances.Add(obj);
            return id;
        }

        SerializeInstanceInfo ISerializationContext.GetInstanceInfo(object obj)
        {
            var index = _serializedInstances.FindIndex(t => t.Instance == obj);

            if (index >= 0)
            {
                return new SerializedYetInfo(index);
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
            _serializedInstances.Clear();

            object o = null;
            SerializeTypeEnum t = (SerializeTypeEnum)stream.ReadByte();

            if (t is SerializeTypeEnum.Null)
            {
                o = null;
            }
            else
            {
                var info = this.GetUninitializedTypeInfo(t);
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

        SerializeInstanceInfo ISerializationContext.GetUninitializedTypeInfo(SerializeTypeEnum type) { return this.GetUninitializedTypeInfo(type); }

        private SerializeInstanceInfo GetUninitializedTypeInfo(SerializeTypeEnum type)
        {
            SerializeInstanceInfo info = null;
            switch (type)
            {
                case SerializeTypeEnum.Null:
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
                        info = new PrimitiveTypeInfo(type, this);
                    }
                    break;
                case SerializeTypeEnum.Enum:
                    {
                        info = new EnumInfo();
                    }
                    break;
                case SerializeTypeEnum.ArrayOfPrimitives:
                    {
                        info = new ArrayOfPrimitivesInfo(this);
                    }
                    break;
                case SerializeTypeEnum.ArrayOfByref:
                    {
                        info = new ArrayOfByRefInfo(this);
                    }
                    break;
                case SerializeTypeEnum.Custom:
                    {
                        info = new CustomInfo(this);
                    }
                    break;
                case SerializeTypeEnum.SerializedYet:
                    {
                        info = new SerializedYetInfo(-1);
                    }
                    break;
                default:
                    break;
            }
            return info;
        }
    }
}
