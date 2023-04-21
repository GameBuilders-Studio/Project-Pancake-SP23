using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GameBuilders.Variables.Editor
{
    public static partial class ReflectionUtility
    {
        public static string FromCppToCsPath(string cppPath)
        {
            return cppPath.Replace(".Array.data", "");
        }

        public static T FindFieldByPath<T>(object subject, string propertyPath)
        {
            if (!propertyPath.Contains("."))
            {
                return FindFieldByName<T>(subject, propertyPath);
            }
            else
            {
                string[] splitPropertyPath = propertyPath.Split('.');

                object currentSubject = subject;

                foreach (string propertyName in splitPropertyPath)
                {
                    currentSubject = FindFieldByName<object>(currentSubject, propertyName);
                }

                return (T)currentSubject;
            }
        }

        public static T FindFieldByName<T>(object subject, string fieldName)
        {
            if (fieldName.EndsWith("]"))
            {
                int indexOfEndBracket = fieldName.IndexOf("]");
                int indexOfStartBracket = fieldName.IndexOf("[");

                string arrayName = fieldName.Substring(0, indexOfStartBracket);
                string indexString = fieldName.Substring(indexOfStartBracket + 1, indexOfEndBracket - indexOfStartBracket - 1);
                int index = Int32.Parse(indexString);

                FieldInfo field = FindFieldInfoByName(subject.GetType(), arrayName);

                object fieldValue = field.GetValue(subject);

                if (fieldValue == null)
                {
                    return default;
                }
                else if (fieldValue is T[] arrayOfT)
                {
                    if (index < arrayOfT.Length)
                    {
                        return arrayOfT[index];
                    }
                    else
                    {
                        return default;
                    }
                }
                else if (fieldValue is IList list)
                {
                    if (index < list.Count)
                    {
                        return (T)list[index];
                    }
                    else
                    {
                        return default;
                    }
                }
                else if (fieldValue is IEnumerable enumerableObject)
                {
                    IEnumerator enumerator = enumerableObject.GetEnumerator();

                    // Enumerator is positioned before the first element in the collection
                    // after initialization. If MoveNext returns false here, the collection is empty
                    if (!enumerator.MoveNext())
                    {
                        return default;
                    }

                    // We move the enumerator forward according to the index
                    for (int i = 0; i < index; i++)
                    {
                        // check, we do not move past the end of the list
                        if (!enumerator.MoveNext())
                        {
                            return default;
                        }
                    }

                    return (T)enumerator.Current;
                }
                else
                {
                    Debug.LogErrorFormat("Unknown collection type {1} on field {0} on type {2}.",
                        fieldName, fieldValue.GetType().ToString(), subject.GetType().Name);

                    return default;
                }
            }
            else
            {
                FieldInfo field = FindFieldInfoByName(subject.GetType(), fieldName);

                if (field != null)
                {
                    T refValue = (T)field.GetValue(subject);

                    if (refValue != null)
                    {
                        return refValue;
                    }
                    else
                    {
                        Debug.LogWarningFormat("Could not cast field '{0}' to type '{1}'. Is of type '{2}'.",
                            fieldName, typeof(T).FullName, field.FieldType.FullName);
                        return default;
                    }
                }
                else
                {
                    Debug.LogWarningFormat("Could not find field with name '{0}' on type '{1}'.",
                        fieldName, subject.GetType().FullName);
                    return default;
                }
            }
        }

        public static FieldInfo FindFieldInfoByName(Type type, string fieldName)
        {
            BindingFlags searchFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy;

            FieldInfo fieldInfo = type.GetField(fieldName, searchFlags);

            if (fieldInfo != null)
            {
                return fieldInfo;
            }
            else if (type.BaseType != typeof(object))
            {
                return FindFieldInfoByName(type.BaseType, fieldName);
            }
            else
            {
                return null;
            }
        }

        public static bool TryExecuteMethod(object subject, string methodName)
        {
            MethodInfo[] methodsWithName = FindMethodsByName(subject, methodName);

            if (methodsWithName.Length == 0)
            {
                return false;
            }

            MethodInfo methodMatch = FilterParametersless(methodsWithName).FirstOrDefault();

            if (methodMatch == null)
            {
                return false;
            }

            methodMatch.Invoke(subject, null);
            return true;
        }

        public static bool TryExecuteMethod(object subject, string methodName, params object[] parameters)
        {
            MethodInfo[] methodsWithName = FindMethodsByName(subject, methodName);

            if (methodsWithName.Length == 0)
            {
                return false;
            }

            Type[] parameterTypesGiven = GetTypes(parameters);

            MethodInfo methodMatch = FilterMatchingParameters(methodsWithName, parameterTypesGiven).FirstOrDefault();

            if (methodMatch == null)
            {
                return false;
            }

            methodMatch.Invoke(subject, parameters);
            return true;
        }

        public static MethodInfo[] FindMethodsByName(object subject, string methodName)
        {
            BindingFlags publicInstanceFlags = BindingFlags.Public | BindingFlags.Instance;

            MemberInfo[] methodsWithName = subject.GetType().FindMembers(MemberTypes.Method, publicInstanceFlags,
                MethodMatchesName, methodName);

            return Array.ConvertAll(methodsWithName, (MemberInfo method) => (MethodInfo)method);
        }

        public static bool MethodMatchesName(MemberInfo memberInfo, object nameToMatch)
        {
            return memberInfo.Name.Equals(nameToMatch);
        }

        public static Type[] GetTypes(object[] subjects)
        {
            Type[] subjectTypes = new Type[subjects.Length];

            for (int i = 0; i < subjectTypes.Length; i++)
            {
                if (subjects[i] == null)
                {
                    subjectTypes[i] = null;
                }
                else
                {
                    subjectTypes[i] = subjects[i].GetType();
                }
            }

            return subjectTypes;
        }

        public static IEnumerable<MethodInfo> FilterParametersless(MethodInfo[] methods)
        {
            return methods.Where(m => m.GetParameters().Length == 0);
        }

        public static IEnumerable<MethodInfo> FilterMatchingParameters(MethodInfo[] methods, Type[] parametersToMatch)
        {
            return methods.Where(m => ParametersMatch(m.GetParameters(), parametersToMatch));
        }

        public static bool ParametersMatch(ParameterInfo[] parametersFound, Type[] parameterTypesGiven)
        {
            if (parametersFound.Length != parameterTypesGiven.Length)
            {
                return false;
            }

            for (int i = 0; i < parametersFound.Length; i++)
            {
                if (!parametersFound[i].ParameterType.IsAssignableFrom(parameterTypesGiven[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}