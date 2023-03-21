using UnityEngine;

public abstract class InteractionProvider : MonoBehaviour
{
    protected abstract InteractableEntity Entity { get; }

    /// <summary>
    /// Returns true if any InteractionBehaviours match the given type.
    /// </summary>
    /// <remarks>
    /// For interfaces, use TryGetInterface.
    /// </remarks>
    public bool TryGetBehaviour<T>(out T component) where T : InteractionBehaviour
    {
        bool exists = Entity.TypeToBehaviour.TryGetValue(typeof(T), out InteractionBehaviour behaviour);
        component = (T)behaviour;
        return exists;
    }

    public bool HasBehaviour<T>() where T : InteractionBehaviour
    {
        return Entity.TypeToBehaviour.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Returns true if any InteractionBehaviours implement the given interface.
    /// </summary>
    public bool TryGetInterface<T>(out T _interface) where T : IInteractionInterface
    {
        bool exists = Entity.TypeToInterface.TryGetValue(typeof(T), out IInteractionInterface interfaceOut);
        _interface = (T)interfaceOut;
        return exists;
    }

    public bool HasInterface<T>() where T : IInteractionInterface
    {
        return Entity.TypeToInterface.ContainsKey(typeof(T));
    }
}