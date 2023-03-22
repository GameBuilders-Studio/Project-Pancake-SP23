using UnityEngine;

public abstract class InteractionProvider : MonoBehaviour
{
    protected abstract InteractionCollection Collection { get; }

    /// <summary>
    /// Returns true if any InteractionBehaviours match the given type.
    /// </summary>
    /// <remarks>
    /// For interfaces, use TryGetInterface.
    /// </remarks>
    public bool TryGetBehaviour<T>(out T component) where T : InteractionBehaviour
    {
        bool exists = Collection.TypeToBehaviour.TryGetValue(typeof(T), out InteractionBehaviour behaviour);
        component = (T)behaviour;
        return exists;
    }

    public bool HasBehaviour<T>() where T : InteractionBehaviour
    {
        return Collection.TypeToBehaviour.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Returns true if any InteractionBehaviours implement the given interface.
    /// </summary>
    public bool TryGetInterface<T>(out T _interface) where T : IInteractionInterface
    {
        bool exists = Collection.TypeToInterface.TryGetValue(typeof(T), out IInteractionInterface interfaceOut);
        _interface = (T)interfaceOut;
        return exists;
    }

    public bool HasInterface<T>() where T : IInteractionInterface
    {
        return Collection.TypeToInterface.ContainsKey(typeof(T));
    }
}