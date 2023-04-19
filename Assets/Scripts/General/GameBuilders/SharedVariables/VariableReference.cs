using System;
using UnityEngine.Events;

namespace GameBuilders.Variables
{
    /// <summary>
    /// A class that references a GlobalVariable. Use this to read GlobalVariable values.
    /// </summary>
    /// <remarks>
    /// For testing purposes, the returned value can be overridden in the inspector.
    /// </remarks>
    [Serializable]
    public abstract class VariableReference 
    {

    }
    
    /// <summary>
    /// Stores a reference to a SharedVariable. For testing purposes, the value can also be set to a constant in the inspector.
    /// </summary>
    [Serializable]
    public abstract class VariableReference<T> : VariableReference
    {
        public SharedVariable<T> Variable;
        public bool UseGlobal = true;
        public T LocalValue;

        public event UnityAction<T> OnValueChanged;
        
        public VariableReference() {}

        public VariableReference(T value)
        {
            UseGlobal = false;
            LocalValue = value;
        }

        public T Value
        {
            get { return UseGlobal ? Variable.Value : LocalValue; }
            set
            {
                if (value.Equals(Value)) { return; }

                if (UseGlobal)
                {
                    Variable.Value = value;
                }
                else
                {
                    LocalValue = value;
                }

                OnValueChanged?.Invoke(value);
            }
        }

        public static implicit operator T(VariableReference<T> reference)
        {
            return reference.Value;
        }
    }
}

