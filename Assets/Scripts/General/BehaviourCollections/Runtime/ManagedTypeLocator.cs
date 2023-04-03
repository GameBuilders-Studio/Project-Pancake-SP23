using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BehaviourCollections
{
    internal static class ManagedTypeLocator
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterManagedTypes()
        {
            var managedTypes = GetAllDerivedTypes(typeof(ManagedMonoBehaviour<>));

            Debug.Log($"Found {managedTypes.Count} derived types");

            foreach (var type in managedTypes)
            {
                FastBehaviourDictionary.RegisterType(type);

                var interfaces = type.GetInterfaces();
                foreach (var interfaceType in interfaces)
                {
                    FastBehaviourDictionary.RegisterType(interfaceType);
                }
            }
        }

        public static List<Type> GetAllDerivedTypes(Type baseType)
        {
            var typesInAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .ToArray();

            var results = new List<Type>();
            GetAllDerivedTypesRecursively(typesInAssembly, baseType, ref results);

            return results;
        }

        private static void GetAllDerivedTypesRecursively(Type[] types, Type baseType, ref List<Type> results)
        {
            Type[] derivedTypes;

            if (baseType.IsGenericType)
            {
                derivedTypes = types.Where
                (
                    t => t.BaseType != null 
                    && t.BaseType.IsGenericType 
                    && t.BaseType.GetGenericTypeDefinition() == baseType
                ).ToArray();
            }
            else
            {
                derivedTypes = types.Where(t => t != baseType && baseType.IsAssignableFrom(t)).ToArray();
            }

            results.AddRange(derivedTypes);

            foreach (Type derivedType in derivedTypes)
            {
                GetAllDerivedTypesRecursively(types, derivedType, ref results);
            }
        }
    }
}
