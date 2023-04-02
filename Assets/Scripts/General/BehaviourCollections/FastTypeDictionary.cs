using System;
using System.Threading;
using System.Collections.Generic;

/// <summary>
/// A high-performance dictionary that uses System.Type as keys.
/// </summary>
public class FastTypeDictionary<TValue>
{
    private static int typeIndex;
    private static readonly Dictionary<Type, AddCall> typeToAddCall = new();

    private delegate void AddCall(FastTypeDictionary<TValue> dict, TValue value);

    private readonly object _lockObject = new();

    private TValue[] _values = new TValue[100];

    /// <summary>
    /// This method uses reflection at runtime! For better performance, use <see cref="FastTypeDictionary{T}.Add{K}(T)"/>
    /// </summary>
    public void AddByTypeParameter(Type type, TValue value)
    {
        if (!typeToAddCall.TryGetValue(type, out AddCall addCall))
        {
            var methodInfo =
                typeof(FastTypeDictionary<TValue>)
                    .GetMethod("Add")
                    .MakeGenericMethod(type);

            // open delegates are ~10x faster than MethodInfo.Invoke calls
            // create a delegate for each Type and cache it for repeat calls
            
            addCall = (AddCall)Delegate.CreateDelegate(typeof(AddCall), methodInfo);
            typeToAddCall.Add(type, addCall);
        }

        addCall.Invoke(this, value);
    }

    public void Add<TKey>(TValue value)
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

    public TValue Get<TKey>()
    {
        lock (_lockObject)
        {
            var id = TypeKey<TKey>.Id;
            return id >= _values.Length ? default : _values[id];
        }
    }

    public bool Contains<TKey>()
    {
        lock (_lockObject)
        {
            var id = TypeKey<TKey>.Id;
            return id < _values.Length && _values[id] is not null;
        }
    }

    public void Remove<TKey>()
    {
        lock (_lockObject)
        {
            var id = TypeKey<TKey>.Id;
            if (id >= _values.Length) { return; }
            _values[id] = default;
        }
    }

    // assign every Type a unique Id with this inner class
    private static class TypeKey<TKey>
    {
        internal static readonly int Id = Interlocked.Increment(ref typeIndex);
    }
}
