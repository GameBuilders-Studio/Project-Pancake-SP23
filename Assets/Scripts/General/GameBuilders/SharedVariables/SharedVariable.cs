using System;
using UnityEngine;
using UnityEngine.Events;

namespace GameBuilders.Variables
{
    /// <summary>
    /// A scriptable object wrapper for <typeparamref name="T"/>. Use this for shared data between systems.
    /// </summary>
    [Serializable]
    public class SharedVariable<T> : ScriptableObject
    {
        public T Value;

#if UNITY_EDITOR
        [TextArea]
        public string DeveloperDescription = "";
#endif
    }
}
