using EIP712.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace EIP712.Utilities
{
    internal static class GenericExtensions
    {
        // Get all properties on which MemberAttribute is applied and order them by "Order" property
        public static Tuple<PropertyInfo, MemberAttribute>[] 
            GetMemberProperties<T>(this T structure)            
            => structure.GetType().GetTypeInfo().DeclaredProperties.
                Where(prop => prop.CustomAttributes.Any(
                    attr => attr.AttributeType == typeof(MemberAttribute)) 
                    && prop.GetValue(structure) != null).
                Select(prop => Tuple.Create(prop, prop.GetCustomAttribute<MemberAttribute>())).
                OrderBy(propAttrPair => propAttrPair.Item2.Order).ToArray();


        /// <summary>
        /// Gets structure name which is going be used in encoding the type
        /// </summary>
        /// <typeparam name="T">Structured data datatype</typeparam>
        /// <param name="structure">Structured data</param>
        /// <returns>Structure name</returns>
        public static string GetStructureName<T>(this T structure)
        {
            Type structType = structure.GetType();
            StructNameAttribute nameAttr = structType.GetTypeInfo().
                GetCustomAttribute<StructNameAttribute>();

            return nameAttr == null ? structType.Name : nameAttr.Name;
        }
    }
}
