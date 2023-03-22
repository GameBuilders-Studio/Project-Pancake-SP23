using System;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;
using GameBuilders.Serialization;

[Serializable]
public class InteractionDictionary : SerializableDictionary<SerializableType, InteractionBehaviour> 
{
    public bool ContainsType(Type type)
    {
        foreach (SerializableType key in Keys)
        {
            if (key.Type == type) { return true; }
        }
        return false;
    }
}

/// <summary>
/// Collects and caches InteractionBehaviours and IInteractionInterfaces attached to the game object
/// </summary>
public class InteractionCollection : InteractionProvider
{
    [SerializeField]
    [ValidateInput("NoDuplicates", "Multiple InteractionBehaviours cannot inherit from the same class or interface")]
    [ReadOnly]
    public List<InteractionBehaviour> Behaviours;

    [SerializeField]
    [ReadOnly]
    public List<string> Interfaces;

    [SerializeField]
    [HideInInspector]
    private InteractionDictionary _typeToBehaviourSerialized;

    [SerializeField]
    [HideInInspector]
    private InteractionDictionary _typeToInterfaceSerialized;

    private Dictionary<Type, InteractionBehaviour> _typeToBehaviourRuntime = new();
    private Dictionary<Type, IInteractionInterface> _typeToInterfaceRuntime = new();

    [SerializeField]
    [HideInInspector]
    private bool _noDuplicates = true;

    private bool NoDuplicates() => _noDuplicates;

    [Button]
    public void RefreshBehaviours() => OnValidate();

    public IDictionary<Type, InteractionBehaviour> TypeToBehaviour => _typeToBehaviourRuntime;
    public IDictionary<Type, IInteractionInterface> TypeToInterface => _typeToInterfaceRuntime;

    protected sealed override InteractionCollection Collection => this;

    void OnValidate()
    {
        GetBehavioursAndInterfaces();
    }

    void Awake()
    {
        // convert serialized types to System.Type at runtime
        foreach (KeyValuePair<SerializableType, InteractionBehaviour> entry in _typeToBehaviourSerialized)
        {
            _typeToBehaviourRuntime.Add(entry.Key.Type, entry.Value);
        }

        foreach (KeyValuePair<SerializableType, InteractionBehaviour> entry in _typeToInterfaceSerialized)
        {
            _typeToInterfaceRuntime.Add(entry.Key.Type, (IInteractionInterface)entry.Value);
        }
    }

    private void GetBehavioursAndInterfaces()
    {
        Behaviours = new(GetComponents<InteractionBehaviour>());
        Interfaces.Clear();

        _typeToBehaviourSerialized.Clear();
        _typeToInterfaceSerialized.Clear();

        foreach (InteractionBehaviour behaviour in Behaviours)
        {
            behaviour.AssignToEntity(this);

            var type = behaviour.GetType();
            var interfaces = type.GetInterfaces();

            foreach (var _interface in interfaces)
            {
                if (_interface == typeof(IInteractionInterface)) { continue; }

                if (_typeToInterfaceSerialized.ContainsType(_interface))
                {
                    _noDuplicates = false;
                    Debug.LogError($"Found duplicate InteractionBehaviours implementing {_interface}", behaviour);
                    return;
                }

                _typeToInterfaceSerialized.TryAdd(new SerializableType(_interface), behaviour);

                Interfaces.Add(_interface.Name);
            }

            while (type != typeof(InteractionBehaviour))
            {
                if (_typeToBehaviourSerialized.ContainsType(type))
                {
                    _noDuplicates = false;
                    Debug.LogError($"Found duplicate InteractionBehaviours of type {type}", behaviour);
                    return;
                }

                _typeToBehaviourSerialized.TryAdd(new SerializableType(type), behaviour);

                type = type.BaseType;
            }
        }

        _noDuplicates = true;
    }
}