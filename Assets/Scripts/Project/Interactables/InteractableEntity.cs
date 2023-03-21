using System;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;
using GameBuilders.Serialization;

[Serializable]
public class InteractionDictionary : SerializableDictionary<SerializableType, InteractionBehaviour> {}

/// <summary>
/// Collects and caches InteractionBehaviours and IInteractionInterfaces attached to the gameObject
/// </summary>
public class InteractableEntity : InteractionProvider
{
    [SerializeField]
    [ValidateInput("NoDuplicates", "Multiple InteractionBehaviours cannot inherit from the same class or interface")]
    public List<InteractionBehaviour> Behaviours;

    [SerializeField]
    public List<string> Interfaces;

    [SerializeField]
    [HideInInspector]
    private InteractionDictionary _typeToBehaviourSerialized;

    [SerializeField]
    [HideInInspector]
    private InteractionDictionary _typeToInterfaceSerialized;

    private Dictionary<Type, InteractionBehaviour> _typeToBehaviourRuntime = new();
    private Dictionary<Type, IInteractionInterface> _typeToInterfaceRuntime = new();

    [HideInInspector] private bool _noDuplicates = true;
    private bool NoDuplicates() => _noDuplicates;

    [Button]
    public void RefreshBehaviours() => OnValidate();

    public Dictionary<Type, InteractionBehaviour> TypeToBehaviour => _typeToBehaviourRuntime;
    public Dictionary<Type, IInteractionInterface> TypeToInterface => _typeToInterfaceRuntime;

    protected override InteractableEntity Entity => this;

    void OnValidate()
    {
        Behaviours = new(GetComponents<InteractionBehaviour>());
        Interfaces.Clear();

        _typeToBehaviourSerialized.Clear();
        _typeToInterfaceSerialized.Clear();

        foreach (InteractionBehaviour behaviour in Behaviours)
        {
            var type = behaviour.GetType();

            var interfaces = type.GetInterfaces();

            foreach (var _interface in interfaces)
            {
                if (_interface == typeof(IInteractionInterface)) { continue; }

                if (!_typeToInterfaceSerialized.TryAdd(new SerializableType(_interface), behaviour))
                {
                    _noDuplicates = false;
                    Debug.LogWarning($"Found duplicate InteractionBehaviours implementing {_interface}", behaviour);
                    return;
                }
                Interfaces.Add(_interface.Name);
            }

            while (type != typeof(InteractionBehaviour))
            {
                if (_typeToBehaviourSerialized.TryAdd(new SerializableType(type), behaviour))
                {
                    behaviour.AssignToEntity(this);
                }
                else
                {
                    _noDuplicates = false;
                    Debug.LogWarning($"Found duplicate InteractionBehaviours of type {type}", behaviour);
                    return;
                }
                type = type.BaseType;
            }
        }

        _noDuplicates = true;
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
}