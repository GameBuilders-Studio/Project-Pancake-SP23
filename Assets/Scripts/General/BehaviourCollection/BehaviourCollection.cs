using System;
using System.Collections.Generic;
using UnityEngine;
using GameBuilders.Serialization;

namespace BehaviourCollections
{
    /// <summary>
    /// Collects and caches <typeparamref name="TBehaviour"/> components and their Interfaces on this gameObject
    /// </summary>
    public class BehaviourCollection<TBehaviour> : BehaviourProvider<TBehaviour>
        where TBehaviour : BehaviourProvider<TBehaviour>
    {
        [SerializeField]
        public List<TBehaviour> Behaviours;

        [SerializeField]
        public List<string> Interfaces;

        [SerializeField, HideInInspector]
        private SerializableDictionary<SerializableType, TBehaviour> _typeToBehaviourSerialized;

        [SerializeField, HideInInspector]
        private SerializableDictionary<SerializableType, TBehaviour> _typeToInterfaceSerialized;

        public Dictionary<Type, TBehaviour> TypeToBehaviour = new();
        public Dictionary<Type, TBehaviour> TypeToInterface = new();

        [SerializeField, HideInInspector]
        public bool NoDuplicates = true;

        protected sealed override BehaviourCollection<TBehaviour> Collection => this;

        private void OnValidate()
        {
            GetBehavioursAndInterfaces();
        }

        private void Awake()
        {
            // convert serialized types to System.Type at runtime
            foreach (KeyValuePair<SerializableType, TBehaviour> entry in _typeToBehaviourSerialized)
            {
                TypeToBehaviour.Add(entry.Key.Type, entry.Value);
            }

            foreach (KeyValuePair<SerializableType, TBehaviour> entry in _typeToInterfaceSerialized)
            {
                TypeToInterface.Add(entry.Key.Type, entry.Value);
            }
        }

        private void GetBehavioursAndInterfaces()
        {
            Behaviours = new(GetComponents<TBehaviour>());

            Interfaces.Clear();

            _typeToBehaviourSerialized.Clear();
            _typeToInterfaceSerialized.Clear();

            foreach (TBehaviour behaviour in Behaviours)
            {
                behaviour.InitializeCollection(this);

                var type = behaviour.GetType();
                var interfaces = type.GetInterfaces();

                foreach (var _interface in interfaces)
                {
                    if (ContainsType(_typeToInterfaceSerialized, _interface))
                    {
                        NoDuplicates = false;
                        Debug.LogError($"Found duplicate {typeof(TBehaviour).Name}s implementing {_interface}. This is not allowed!", behaviour);
                        return;
                    }

                    _typeToInterfaceSerialized.TryAdd(new SerializableType(_interface), behaviour);

                    Interfaces.Add(_interface.Name);
                }

                while (type != typeof(TBehaviour) && type != typeof(MonoBehaviour))
                {
                    if (ContainsType(_typeToBehaviourSerialized, type))
                    {
                        NoDuplicates = false;
                        Debug.LogError($"Found duplicate {typeof(TBehaviour).Name}s of type {type}. This is not allowed!", behaviour);
                        return;
                    }

                    _typeToBehaviourSerialized.TryAdd(new SerializableType(type), behaviour);

                    type = type.BaseType;
                }
            }

            NoDuplicates = true;
        }

        private bool ContainsType(SerializableDictionary<SerializableType, TBehaviour> dictionary, Type type)
        {
            foreach (SerializableType key in dictionary.Keys)
            {
                if (key.Type == type) { return true; }
            }
            return false;
        }
    }
}
