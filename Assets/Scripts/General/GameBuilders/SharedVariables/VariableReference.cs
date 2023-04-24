using System;
using UnityEngine;
using UnityEngine.Events;

namespace GameBuilders.Variables
{
    public abstract class VariableReference 
    {
        public event UnityAction OnValueChanged;
        
        public void RaiseChangedEvent()
        {
            OnValueChanged?.Invoke();
        }
    }
    
    /// <summary>
    /// Stores a reference to a SharedVariable. This value can be set to a constant in the inspector.
    /// </summary>
    public abstract class VariableReference<T, SharedVariableSO> : VariableReference
        where SharedVariableSO : SharedVariable<T>
    {
        public SharedVariableSO Variable;

        [SerializeField]
        private bool UseGlobal = true;

        [SerializeField]
        private T LocalValue;

        public T value
        {
            get
            {
                if (UseGlobal)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (Variable.requireInitialization)
                    {
                        if (!Variable.isInitialized)
                        {
                            Debug.LogError($"Shared Variable {Variable.name} is not initialized!");
                        }
                    }
#endif
                    return Variable.value;
                }  
                else
                {
                    return LocalValue;
                }
            }
        }

        public void SetValue(T value)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (UseGlobal)
            {
                Variable.isInitialized = true;
                if (Variable.readOnly)
                {
                    Debug.LogError($"Shared Variable {Variable.name} is read only!");
                    return;
                }
            }
#endif
            if (value.Equals(this.value)) { return; }

            if (UseGlobal)
            {
                Variable.value = value;
            }
            else
            {
                LocalValue = value;
            }

            RaiseChangedEvent();
        }

        public static implicit operator T(VariableReference<T, SharedVariableSO> reference)
        {
            return reference.value;
        }
    }
}

