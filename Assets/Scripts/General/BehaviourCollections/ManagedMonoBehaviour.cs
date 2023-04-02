using UnityEngine;

namespace BehaviourCollections
{
    /// <summary>
    /// Base class for MonoBehaviours that are managed by a behaviour collection
    /// </summary>
    public abstract class ManagedMonoBehaviour<TBehaviour> : BehaviourProvider<TBehaviour>
        where TBehaviour : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        public BehaviourProvider<TBehaviour> Collection;

        internal void Initialize()
        {
            if (Collection == null)
            {
                Debug.LogError("ManagedMonoBehaviour has no associated Collection");
            }
            TypeToBehaviour = Collection.TypeToBehaviour;
            TypeToInterface = Collection.TypeToInterface;
        }

        /// <summary>
        /// Remove this <typeparamref name="TBehaviour"/> from its associated collection 
        /// </summary>
        /// <remarks>
        /// IMPORTANT: Pass the type of this script as a generic parameter.
        /// </remarks>
        public void Deregister<T>()
        {
            TypeToBehaviour.Remove<T>();
        }
    }
}