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
using static Serializer.SerializeTypes;

namespace Serializer
{
    public class MySerializer
    {
        
        public void Serialize(object obj, Stream stream)
        {
            if (obj is null)
            {
                stream.WriteByte((byte)SerializeTypeEnum.Null);
            }
            else
            {
                var info = SerializeTypeInfo.GetTypeInfo(obj);
                info.Write(stream);
            }
        }

        public object Deserialize(Stream stream)
        {
            object o = null;
            SerializeTypeEnum t = (SerializeTypeEnum)stream.ReadByte();

            switch (t)
            {
                case SerializeTypeEnum.Null:
                    {
                        o = null;
                    }
                    break;
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
                case SerializeTypeEnum.Enum:
                case SerializeTypeEnum.ArrayOfStruct:
                case SerializeTypeEnum.ArrayOfByref:
                case SerializeTypeEnum.Custom:
                    {
                        var info = SerializeTypeInfo.GetTypeInfo(t);
                        info.Read(stream);
                        o = info.Get();
                    }
                    break;
                default:
                    throw new NotImplementedException();
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
