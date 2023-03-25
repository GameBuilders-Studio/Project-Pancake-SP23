using UnityEngine;

namespace BehaviourCollections
{
    public class BehaviourProvider<TBehaviour> : MonoBehaviour
        where TBehaviour : BehaviourProvider<TBehaviour>
    {
        private BehaviourCollection<TBehaviour> _collection = null;

        protected virtual BehaviourCollection<TBehaviour> Collection { get; }

        internal void InitializeCollection(BehaviourCollection<TBehaviour> collection)
        {
            if (Collection != null) { return; }
            _collection = collection;
        }

        /// <summary>
        /// Returns true if any InteractionBehaviours match the given type.
        /// </summary>
        /// <remarks>
        /// For interfaces, use TryGetInterface
        /// </remarks>
        public bool TryGetBehaviour<T>(out T component) where T : TBehaviour
        {
            bool exists = Collection.TypeToBehaviour.TryGetValue(typeof(T), out TBehaviour behaviour);
            component = (T)behaviour;
            return exists;
        }

        /// <summary>
        /// Returns true if any InteractionBehaviours match the given type.
        /// </summary>
        public bool HasBehaviour<T>() where T : TBehaviour
        {
            return Collection.TypeToBehaviour.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Returns true if any InteractionBehaviours implement the given interface.
        /// </summary>
        public bool TryGetInterface<T>(out T _interface) where T : class
        {
            bool exists = Collection.TypeToInterface.TryGetValue(typeof(T), out TBehaviour interfaceOut);
            _interface = interfaceOut as T;
            return exists;
        }

        /// <summary>
        /// Returns true if any InteractionBehaviours implement the given interface.
        /// </summary>
        public bool HasInterface<T>() where T : class
        {
            return Collection.TypeToInterface.ContainsKey(typeof(T));
        }
    }
}
