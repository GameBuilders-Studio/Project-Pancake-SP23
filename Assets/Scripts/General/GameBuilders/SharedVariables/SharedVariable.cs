using System;
using UnityEngine;

namespace GameBuilders.Variables
{
    /// <summary>
    /// A scriptable object wrapper for <typeparamref name="T"/>. Use this for shared data between systems.
    /// </summary>
    [Serializable]
    public class SharedVariable<T> : ScriptableObject
    {
        public T value;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Tooltip("Whether this variable can be modified at runtime.")]
        public bool readOnly = false;

        [Tooltip("Whether this variable should be initialized before being read.")]
        public bool requireInitialization = false;

        [TextArea(1, 10)]
        public string developerDescription = "";

        [HideInInspector]
        public bool isInitialized = false;

        void OnEnable()
        {
            isInitialized = false;
        }
#endif
    }
}
