using System;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;
using GameBuilders.Serialization;

[Serializable]
public class InteractionDictionary : SerializableDictionary<SerializableType, InteractionBehaviour> {}

public class InteractableEntity : MonoBehaviour, IInteractable
{
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

    private Dictionary<Type, InteractionBehaviour> _typeToBehaviour = new();
    private Dictionary<Type, IInteractionInterface> _typeToInterface = new();

    [HideInInspector] private bool _noDuplicates = true;
    private bool NoDuplicates() => _noDuplicates;

    [Button]
    public void RefreshBehaviours() => OnValidate();

    void OnValidate()
    {
        // use reflection to build a serialized dictionary of:
        // SerializableType -> InteractionBehaviour
        // SerializableType -> IInteractionInterface

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
                    behaviour.Entity = this;
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
        foreach (KeyValuePair<SerializableType, InteractionBehaviour> entry in _typeToBehaviourSerialized)
        {
            _typeToBehaviour.Add(entry.Key.Type, entry.Value);
        }

        foreach (KeyValuePair<SerializableType, InteractionBehaviour> entry in _typeToInterfaceSerialized)
        {
            _typeToInterface.Add(entry.Key.Type, (IInteractionInterface)entry.Value);
        }
    }

    /// <summary>
    /// Returns true if any InteractionBehaviours match the given type.
    /// </summary>
    /// <remarks>
    /// For interfaces, use TryGetInterface.
    /// </remarks>
    public bool TryGetBehaviour<T>(out T component) where T : InteractionBehaviour
    {
        bool exists = _typeToBehaviour.TryGetValue(typeof(T), out InteractionBehaviour behaviour);
        component = (T)behaviour;
        return exists;
    }

    public bool HasBehaviour<T>() where T : InteractionBehaviour
    {
        return _typeToBehaviour.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Returns true if any InteractionBehaviours implement the given interface.
    /// </summary>
    public bool TryGetInterface<T>(out T _interface) where T : IInteractionInterface
    {
        bool exists = _typeToInterface.TryGetValue(typeof(T), out IInteractionInterface interfaceOut);
        _interface = (T)interfaceOut;
        return exists;
    }

    public bool HasInterface<T>() where T : IInteractionInterface
    {
        return _typeToInterface.ContainsKey(typeof(T));
    }
}

public interface IInteractable
{
    public bool TryGetBehaviour<T>(out T component) where T : InteractionBehaviour;
    public bool HasBehaviour<T>() where T : InteractionBehaviour;
    public bool TryGetInterface<T>(out T _interface) where T : IInteractionInterface;
    public bool HasInterface<T>() where T : IInteractionInterface;
}