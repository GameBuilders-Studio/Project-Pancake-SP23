using UnityEngine;
using UnityEngine.Events;

namespace GameBuilders.Variables
{
    /// <summary>
    /// A scriptable object wrapper for <typeparamref name="T"/>. Use this for shared data between systems.
    /// </summary>
    public class GlobalVariable<T> : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif
        public T _value;

        public event UnityAction<T> OnValueChanged;

        public T Value
        {
            get => _value;
            set
            {
                if (value.Equals(_value)) { return; }
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
    }
}
