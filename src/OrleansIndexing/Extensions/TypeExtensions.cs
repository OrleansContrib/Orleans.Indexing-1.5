using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.Indexing
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Checks whether a givenType is assignable to a genericInterfaceType
        /// </summary>
        /// <param name="givenType">the give type, which is
        /// going to be tested</param>
        /// <param name="genericInterfaceType">the generic
        /// interface to be checked against</param>
        /// <returns></returns>
        public static bool IsAssignableToGenericType(this Type givenType, Type genericInterfaceType)
        {
            return givenType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericInterfaceType) ||
                   givenType.BaseType != null && (givenType.BaseType.IsGenericType && givenType.BaseType.GetGenericTypeDefinition() == genericInterfaceType ||
                                                  givenType.BaseType.IsAssignableToGenericType(genericInterfaceType));
        }

        /// <summary>
        /// This method finds a concrete generic type given a non-concrete 
        /// genericInterfaceType by looking into the type hierarchy of a givenType
        /// </summary>
        /// <param name="givenType">the concrete type</param>
        /// <param name="genericInterfaceType">the non-concrete 
        /// generic interface</param>
        /// <returns></returns>
        public static Type GetGenericType(this Type givenType, Type genericInterfaceType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericInterfaceType)
                    return it;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericInterfaceType)
                return givenType;

            Type baseType = givenType.BaseType;
            if (baseType == null) return null;

            return GetGenericType(baseType, genericInterfaceType);
        }

        /// <summary>
        /// This method returns the type of the class that implements
        /// a given interface, or the interface type itself, if none exists
        /// 
        /// This method is commented out for now, because we found another
        /// way for finding the implementation class type from a given grain,
        /// i.e., TypeCodeMapper.GetImplementation(_indexType).GrainClass
        /// </summary>
        /// <param name="interfaceType">the given interface</param>
        /// <returns>the implementation of the interface</returns>
        //public static Type GetImplementationClassType(this Type interfaceType)
        //{
        //    foreach (Type t in Assembly.GetCallingAssembly().GetTypes())
        //    {
        //        if (t.GetInterface(interfaceType.Name) != null)
        //        {
        //            return t;
        //        }
        //    }
        //    return interfaceType;
        //}
    }
}
