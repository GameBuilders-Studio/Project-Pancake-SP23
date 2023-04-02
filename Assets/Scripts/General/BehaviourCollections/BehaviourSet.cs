using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BehaviourCollections.Serialization;

namespace BehaviourCollections
{
    public class BehaviourSet<TBehaviour> : BehaviourProvider<TBehaviour>
        where TBehaviour : ManagedMonoBehaviour<TBehaviour>
    {
        [SerializeField]
        private TBehaviour[] _behaviours = new TBehaviour[1];

        [SerializeField]
        private TBehaviour[] _interfaces = new TBehaviour[1];

        [SerializeField, HideInInspector]
        private SerializableType[] _behaviourTypes = new SerializableType[1];

        [SerializeField, HideInInspector]
        private SerializableType[] _interfaceTypes = new SerializableType[1];

        [SerializeField, HideInInspector]
        public bool NoDuplicates = true;

        private void Awake()
        {
            TypeToBehaviour = new();
            TypeToInterface = new();
            
            // convert serialized types to System.Type at runtime
            for (int i = 0; i < _behaviourTypes.Length; i++)
            {
                var behaviour = _behaviours[i];
                TypeToBehaviour.AddByTypeParameter(_behaviourTypes[i].Type, behaviour);
                behaviour.Initialize();
            }

            for (int i = 0; i < _interfaceTypes.Length; i++)
            {
                TypeToInterface.AddByTypeParameter(_interfaceTypes[i].Type, _interfaces[i]);
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
            if (components.Length < 0) { return; }

            ArrayUtility.Clear(ref _behaviourTypes);
            ArrayUtility.Clear(ref _behaviours);
            ArrayUtility.Clear(ref _interfaceTypes);
            ArrayUtility.Clear(ref _interfaces);

            List<TBehaviour> behaviours = new(components);

            foreach (TBehaviour behaviour in behaviours)
            {
                var type = behaviour.GetType();
                var interfaces = type.GetInterfaces();

                if (behaviour is ManagedMonoBehaviour<TBehaviour> managedBehaviour)
                {
                    managedBehaviour.Collection = this;
                }

                foreach (var _interface in interfaces)
                {
                    var interfaceKey = new SerializableType(_interface);

                    if (ArrayUtility.Contains(_interfaceTypes, interfaceKey))
                    {
                        NoDuplicates = false;
                        Debug.LogError($"Found multiple {typeof(TBehaviour).Name}s implementing {_interface}. This is not allowed!");
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
