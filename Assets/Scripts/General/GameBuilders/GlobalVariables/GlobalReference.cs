using System;

namespace GameBuilders.Variables
{
    /// <summary>
    /// A class that references a GlobalVariable. Use this to read GlobalVariable values.
    /// </summary>
    /// <remarks>
    /// For testing purposes, the returned value can be overridden in the inspector.
    /// </remarks>
    [Serializable]
    public class GlobalReference<T>
    {
        public GlobalVariable<T> Variable;
        public bool UseOverride = true;
        public T OverrideValue;
        
        public GlobalReference() {}

        public GlobalReference(T value)
        {
            UseOverride = true;
            OverrideValue = value;
        }

        public T Value
        {
            get { return UseOverride ? OverrideValue : Variable.Value; }
        }

        public static implicit operator T(GlobalReference<T> reference)
        {
            return reference.Value;
        }
    }
}

