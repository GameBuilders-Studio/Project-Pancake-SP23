using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Autotiles3D
{
    public static class Autotiles3D_EnumUtility
    {
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
        public static T Previous<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) - 1;
            return (j == -1) ? Arr[Arr.Length - 1] : Arr[j];
        }

        public static IEnumerable<Enum> GetFlags(this Enum e)
        {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
        }

        public static int GetArrayIndexOf<T>(this T src) where T : struct
        {
            T[] Arr = (T[])Enum.GetValues(src.GetType());
            return Array.IndexOf<T>(Arr, src);
        }

        public static T GetEnumByArrayIndex<T>(int arrayIndex) where T : struct
        {
            T[] Arr = (T[])Enum.GetValues(typeof(T));
            return (T)Arr[arrayIndex];
        }

        public static int GetLength<T>() where T : struct
        {
            return Enum.GetNames(typeof(T)).Length;
        }


    }
}
