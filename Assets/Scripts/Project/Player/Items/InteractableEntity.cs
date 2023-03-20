using System;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

public class InteractableEntity : MonoBehaviour
{
    [ValidateInput("HasNoDuplicates", "Multiple InteractionBehaviours cannot inherit from the same type")]
    public List<InteractionBehaviour> Behaviours;

    [HideInInspector]
    private SerializableDictionary<Type, InteractionBehaviour> _typeToBehaviour = new();

    [HideInInspector]
    private SerializableDictionary<Type, IInteractionInterface> _typeToInterface = new();

    [HideInInspector]
    private bool _noDuplicates = true;

    [Button]
    public void RefreshBehaviours() => OnValidate();

    void OnValidate()
    {
        Behaviours = new(GetComponents<InteractionBehaviour>());

        _typeToBehaviour.Clear();
        _typeToInterface.Clear();

        foreach (InteractionBehaviour behaviour in Behaviours)
        {
            var type = behaviour.GetType();

            var interfaces = type.GetInterfaces();
            foreach (var _interface in interfaces)
            {
                if (_interface == typeof(IInteractionInterface)) { continue; }
                if (!_typeToInterface.TryAdd(_interface, (IInteractionInterface)behaviour))
                {
                    _noDuplicates = false;
                    Debug.LogWarning($"Found duplicate InteractionBehaviours implementing {_interface}", behaviour);
                    return;
                }
            }

            while (type != typeof(InteractionBehaviour))
            {
                if (_typeToBehaviour.TryAdd(type, behaviour))
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

    private bool HasNoDuplicates()
    {
        return _noDuplicates;
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

    public bool HasBehaviour<T>()
    {
        return _typeToBehaviour.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Returns true if any InteractionBehaviours implement the given interface.
    /// </summary>
    public bool TryGetInterface<T>(out T _interface) where T : IInteractionInterface
    {
        bool exists = _typeToInterface.TryGetValue(typeof(T), out IInteractionInterface _interfaceOut);
        _interface = (T)_interfaceOut;
        return exists;
    }

    public bool HasInterface<T>() where T : IInteractionInterface
    {
        return _typeToInterface.ContainsKey(typeof(T));
    }
}