using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serializer
{
    public interface ITypesVisitor
    {
        EnumInfo GetEnumInfo(object obj);
        ArrayOfPrimitivesInfo GetArrayOfStructInfo(object obj);
        ArrayOfByRefInfo GetArrayOfByRefInfo(object obj);
        CustomInfo GetCustomTypeInfo(object obj);
    }

    //public class TypesVisitor : ITypesVisitor
    //{
    //    public ArrayOfByRefInfo GetArrayOfByRefInfo(object obj)
    //    {
    //        return new ArrayOfByRefInfo(obj);
    //    }

    //    public ArrayOfStructInfo GetArrayOfStructInfo(object obj)
    //    {
    //        return new ArrayOfStructInfo(obj);
    //    }

    //    public CustomTypeInfo GetCustomTypeInfo(object obj)
    //    {
    //        return new CustomTypeInfo(obj);
    //    }

    //    public EnumInfo GetEnumInfo(object obj)
    //    {
    //        return new EnumInfo(obj);
    //    }
    //}
}
