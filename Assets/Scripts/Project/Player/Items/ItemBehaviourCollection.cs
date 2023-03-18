using System;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

public class ItemBehaviourCollection : MonoBehaviour
{
    [SerializeField]
    public List<ItemBehaviour> Behaviours;

    [Button]
    public void RefreshBehaviours() => OnValidate();

    private Dictionary<Type, ItemBehaviour> _typeToBehaviour = new();

    void OnValidate()
    {
        Behaviours = new(GetComponents<ItemBehaviour>());
    }

    void Awake()
    {
        foreach (ItemBehaviour behaviour in Behaviours)
        {
            var type = behaviour.GetType();
            if (_typeToBehaviour.TryAdd(type, behaviour))
            {
                behaviour.Collection = this;
            }
        }
    }

    /// <summary>
    /// Returns true if the cached ItemBehaviour exists.
    /// </summary>
    /// <remarks>
    /// This method does not support polymorphism or interfaces! Use HasBehaviourType instead.
    /// </remarks>
    public bool TryGetBehaviour<T>(out T component) where T : ItemBehaviour
    {
        bool exists = _typeToBehaviour.TryGetValue(typeof(T), out ItemBehaviour behaviour);
        component = (T)behaviour;
        return exists;
    }

    /// <summary>
    /// Returns true if any cached ItemBehaviours with the given type exists.
    /// </summary>
    /// <remarks>
    /// This method supports polymorphism and interfaces!
    /// </remarks>
    public bool TryGetBehaviourType<T>(out T component)
    {
        foreach (ItemBehaviour behaviour in Behaviours)
        {
            if (behaviour is T castedBehaviour)
            {
                component = castedBehaviour;
                return true;
            }
        }
        component = default;
        return false;
    }
}