using UnityEngine;

namespace BehaviourCollections
{
    /// <summary>
    /// Base class for MonoBehaviours that are managed by a behaviour collection
    /// </summary>
    public abstract class ManagedMonoBehaviour<TBehaviour> : BehaviourCollectionComponent<TBehaviour>
        where TBehaviour : MonoBehaviour
    {
        [HideInInspector]
        public BehaviourCollectionComponent<TBehaviour> Collection;

        [SerializeField, HideInInspector]
        public int HashCode = -1;

        internal void Initialize()
        {
            if (HashCode == -1)
            {
                Debug.LogError("ManagedMonoBehaviour has no associated Collection");
            }
            TypeToBehaviour = Collection.TypeToBehaviour;
            TypeToInterface = Collection.TypeToInterface;
        }
        
        public override int GetHashCode()
        {
            return HashCode;
        }
    }
}