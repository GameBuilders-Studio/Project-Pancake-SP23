using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourCollections
{
    /// <summary>
    /// A high-performance dictionary that uses System.Type as keys.
    /// </summary>
    public class FastTypeDictionary<TValue>
    {
        private static int typeIndex;

        private static readonly Dictionary<Type, AddCall> s_typeToAddCall = new();
        private static readonly Dictionary<Type, RemoveCall> s_typeToRemoveCall = new();

        private delegate void AddCall(FastTypeDictionary<TValue> dict, TValue value);
        private delegate void RemoveCall(FastTypeDictionary<TValue> dict);

        private readonly object _lockObject = new();

        private TValue[] _values = new TValue[32];

        public TValue Get<TKey>()
        {
            var id = TypeKey<TKey>.Id;
            return id >= _values.Length ? default : _values[id];
        }

        public bool Contains<TKey>()
        {
            var id = TypeKey<TKey>.Id;
            return id < _values.Length && _values[id] is not null;
        }

        public void AddGeneric<TKey>(TValue value)
        {
            lock (_lockObject)
            {
                var id = TypeKey<TKey>.Id;
                if (id >= _values.Length)
                {
                    Array.Resize(ref _values, id * 2);
                }

                _values[id] = value;
            }
        }

        public void RemoveGeneric<TKey>()
        {
            var id = TypeKey<TKey>.Id;
            
            if (id >= _values.Length)
            {
                return;
            }

            _values[id] = default!;
        }

        public void Add(Type type, TValue value)
        {
            if (!s_typeToAddCall.TryGetValue(type, out AddCall addCall))
            {
                RegisterType(type);
                addCall = s_typeToAddCall[type];
            }

            addCall.Invoke(this, value);
        }

        public void Remove(Type type)
        {
            if (!s_typeToRemoveCall.TryGetValue(type, out RemoveCall removeCall))
            {
                RegisterType(type);
                removeCall = s_typeToRemoveCall[type];
            }

            removeCall.Invoke(this);
        }

        /// <summary>
        /// Optimizes the addition and removal of this type at runtime
        /// </summary>
        public static void RegisterType(Type type)
        {
            // open delegates are ~10x faster than MethodInfo.Invoke calls
            // create a delegate for adding/removing each Type and cache it for repeat calls

            if (!s_typeToAddCall.ContainsKey(type))
            {
                var addMethodInfo = typeof(FastTypeDictionary<TValue>)
                    .GetMethod("AddGeneric")!
                    .MakeGenericMethod(type);

                var addCall = (AddCall)Delegate.CreateDelegate(typeof(AddCall), addMethodInfo);
                s_typeToAddCall.Add(type, addCall);
            }

            if (!s_typeToRemoveCall.ContainsKey(type))
            {
                var removeMethodInfo = typeof(FastTypeDictionary<TValue>)
                    .GetMethod("RemoveGeneric")!
                    .MakeGenericMethod(type);

                var removeCall = (RemoveCall)Delegate.CreateDelegate(typeof(RemoveCall), removeMethodInfo);
                s_typeToRemoveCall.Add(type, removeCall);
            }
        }

        /// <summary>
        /// An inner class that assigns a unique Id to each Type.
        /// </summary>
        private static class TypeKey<TKey>
        {
            internal static readonly int Id = Interlocked.Increment(ref typeIndex);
        }
    }
}