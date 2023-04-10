using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BehaviourCollections.Serialization;

namespace BehaviourCollections
{
    /// <summary>
    /// Caches ManagedMonoBehaviours deriving from <typeparamref name="TBehaviour"/>.
    /// All behaviours managed by this collection must derive from unique types.
    /// </summary>
    public class BehaviourSet<TBehaviour> : BehaviourProvider<TBehaviour>
        where TBehaviour : ManagedMonoBehaviour<TBehaviour>
    {
        [SerializeField]
        private TBehaviour[] _behaviours = new TBehaviour[0];

        [SerializeField]
        private TBehaviour[] _interfaces = new TBehaviour[0];

        [SerializeField, HideInInspector]
        private SerializableType[] _behaviourTypes = new SerializableType[0];

        [SerializeField, HideInInspector]
        private SerializableType[] _interfaceTypes = new SerializableType[0];

        [SerializeField, HideInInspector]
        public bool NoDuplicates = true;

        private void Awake()
        {
            TypeToBehaviour = new();
            TypeToInterface = new();
            
            // convert serialized types to System.Type at runtime
            for (var i = 0; i < _behaviourTypes.Length; i++)
            {
                var behaviour = _behaviours[i];
                TypeToBehaviour.Add(_behaviourTypes[i].Type, behaviour);
                behaviour.Initialize();
            }

            for (var i = 0; i < _interfaceTypes.Length; i++)
            {
                TypeToInterface.Add(_interfaceTypes[i].Type, _interfaces[i]);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GetComponents<BehaviourSet<TBehaviour>>().Length > 1)
            {
                Debug.LogError($"Duplicate BehaviourCollections with type parameter {typeof(TBehaviour).Name} on gameObject! This is not allowed");
                return;
            }
            GetBehavioursAndInterfaces();
        }

        private void GetBehavioursAndInterfaces()
        {
            var components = GetComponents<TBehaviour>();
            
            ArrayUtility.Clear(ref _behaviourTypes);
            ArrayUtility.Clear(ref _behaviours);
            ArrayUtility.Clear(ref _interfaceTypes);
            ArrayUtility.Clear(ref _interfaces);
            
            if (components.Length == 0) { return; }

            List<TBehaviour> behaviours = new(components);

            foreach (var behaviour in behaviours)
            {
                var type = behaviour.GetType();
                var interfaces = type.GetInterfaces();

                if (behaviour is ManagedMonoBehaviour<TBehaviour> managedBehaviour)
                {
                    managedBehaviour.Collection = this;
                }

                foreach (var @interface in interfaces)
                {
                    var interfaceKey = new SerializableType(@interface);

                    if (ArrayUtility.Contains(_interfaceTypes, interfaceKey))
                    {
                        NoDuplicates = false;
                        Debug.LogError($"Found multiple {typeof(TBehaviour).Name}s implementing {@interface}. This is not allowed!");
                        return;
                    }
                    
                    ArrayUtility.Add(ref _interfaceTypes, interfaceKey);
                    ArrayUtility.Add(ref _interfaces, behaviour);
                }

                while (type != typeof(TBehaviour) && type != typeof(MonoBehaviour))
                {
                    var behaviourKey = new SerializableType(type);

                    if (ArrayUtility.Contains(_behaviourTypes, behaviourKey))
                    {
                        NoDuplicates = false;
                        Debug.LogError($"Found multiple {typeof(TBehaviour).Name}s deriving from {type}. This is not allowed!");
                        return;
                    }

                    ArrayUtility.Add(ref _behaviourTypes, behaviourKey);
                    ArrayUtility.Add(ref _behaviours, behaviour);

                    type = type.BaseType;
                }
            }

            NoDuplicates = true;
        }
#endif
    }
}
