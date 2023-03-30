using System;
using System.Collections.Generic;
using UnityEngine;
using BehaviourCollections.Serialization;

namespace BehaviourCollections
{
    public class BehaviourSet<TBehaviour> : BehaviourCollectionComponent<TBehaviour>
        where TBehaviour : ManagedMonoBehaviour<TBehaviour>
    {
        [SerializeField]
        private SerializableDictionary<SerializableType, TBehaviour> _typeToBehaviour;

        [SerializeField]
        private SerializableDictionary<SerializableType, TBehaviour> _typeToInterface;

        [SerializeField, HideInInspector]
        public bool NoDuplicates = true;

        private void Awake()
        {
            TypeToBehaviour = new();
            TypeToInterface = new();
            
            // convert serialized types to System.Type at runtime
            foreach (KeyValuePair<SerializableType, TBehaviour> entry in _typeToBehaviour)
            {
                var behaviour = entry.Value;
                TypeToBehaviour.Add(entry.Key.Type, behaviour);
                behaviour.Initialize();
            }

            foreach (KeyValuePair<SerializableType, TBehaviour> entry in _typeToInterface)
            {
                TypeToInterface.Add(entry.Key.Type, entry.Value);
            }
        }

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

            List<TBehaviour> behaviours = new(components);

            _typeToBehaviour.Clear();
            _typeToInterface.Clear();

            int nextHashCode = 0;

            foreach (TBehaviour behaviour in behaviours)
            {
                var type = behaviour.GetType();
                var interfaces = type.GetInterfaces();

                if (behaviour is ManagedMonoBehaviour<TBehaviour> managedBehaviour)
                {
                    managedBehaviour.HashCode = nextHashCode;
                    nextHashCode++;
                    managedBehaviour.Collection = this;
                }

                foreach (var _interface in interfaces)
                {
                    var interfaceKey = new SerializableType(_interface);

                    if (!_typeToInterface.TryAdd(interfaceKey, behaviour))
                    {
                        NoDuplicates = false;
                        Debug.LogError($"Found multiple {typeof(TBehaviour).Name}s implementing {_interface}. This is not allowed!");
                        return;
                    }
                }

                while (type != typeof(TBehaviour) && type != typeof(MonoBehaviour))
                {
                    var behaviourKey = new SerializableType(type);

                    if (!_typeToBehaviour.TryAdd(behaviourKey, behaviour))
                    {
                        NoDuplicates = false;
                        Debug.LogError($"Found multiple {typeof(TBehaviour).Name}s deriving from {type}. This is not allowed!");
                        return;
                    }

                    type = type.BaseType;
                }
            }

            NoDuplicates = true;
        }
    }
}
