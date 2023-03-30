using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BehaviourCollections
{
    public abstract class BehaviourCollectionComponent<TBehaviour> : MonoBehaviour
        where TBehaviour : MonoBehaviour
    {
        internal Dictionary<Type, TBehaviour> TypeToBehaviour = new();
        internal Dictionary<Type, TBehaviour> TypeToInterface = new();

        /// <summary>
        /// Returns true if any InteractionBehaviours match the given type.
        /// </summary>
        /// <remarks>
        /// For interfaces, use TryGetInterface
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehaviour<T>(out T component) where T : TBehaviour
        {
            bool exists = TypeToBehaviour.TryGetValue(typeof(T), out TBehaviour behaviour);
            component = (T)behaviour;
            return exists;
        }

        /// <summary>
        /// Returns true if any InteractionBehaviours match the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasBehaviour<T>() where T : TBehaviour
        {
            return TypeToBehaviour.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Returns true if any InteractionBehaviours implement the given interface.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetInterface<T>(out T @interface) where T : class
        {
            bool exists = TypeToInterface.TryGetValue(typeof(T), out TBehaviour behaviour);
            @interface = behaviour as T;
            return exists;
        }

        /// <summary>
        /// Returns true if any InteractionBehaviours implement the given interface.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasInterface<T>() where T : class
        {
            return TypeToInterface.ContainsKey(typeof(T));
        }

        public void UpdateEntries()
        {
            foreach(var key in TypeToBehaviour.Keys.ToArray())
            {
                if (TypeToBehaviour[key] == null)
                {
                    TypeToBehaviour.Remove(key);
                }
            }

            foreach(var key in TypeToInterface.Keys.ToArray())
            {
                if (TypeToInterface[key] == null)
                {
                    TypeToInterface.Remove(key);
                }
            }
        }
    }
}
