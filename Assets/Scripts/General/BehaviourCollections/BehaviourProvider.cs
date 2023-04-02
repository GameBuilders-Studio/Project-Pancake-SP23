using UnityEngine;
using System.Runtime.CompilerServices;

namespace BehaviourCollections
{
    public abstract class BehaviourProvider<TBehaviour> : MonoBehaviour
        where TBehaviour : MonoBehaviour
    {
        internal FastTypeDictionary<TBehaviour> TypeToBehaviour = new();
        internal FastTypeDictionary<TBehaviour> TypeToInterface = new();

        /// <summary>
        /// Returns true if any cached <typeparamref name="TBehaviour"/> matches the given type.
        /// </summary>
        /// <remarks>
        /// For interfaces, use TryGetInterface
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehaviour<T>(out T component) where T : TBehaviour
        {
            var behaviour = TypeToBehaviour.Get<T>();
            component = behaviour as T;
            return component is not null;
        }

        /// <summary>
        /// Returns true if any cached <typeparamref name="TBehaviour"/>s match the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasBehaviour<T>() where T : TBehaviour
        {
            return TypeToBehaviour.Contains<T>();
        }

        /// <summary>
        /// Returns true if any cached <typeparamref name="TBehaviour"/>s implement the given interface.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetInterface<T>(out T @interface) where T : class
        {
            var behaviour = TypeToInterface.Get<T>();
            @interface = behaviour as T;
            return @interface is not null;
        }

        /// <summary>
        /// Returns true if any cached <typeparamref name="TBehaviour"/>s implement the given interface.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasInterface<T>() where T : class
        {
            return TypeToInterface.Contains<T>();
        }
    }
}
