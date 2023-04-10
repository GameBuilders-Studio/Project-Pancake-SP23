using System;

namespace CustomAttributes
{
    /// <summary>
    /// Make a serialized field readonly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ReadOnlyAttribute : MetaAttribute
    {

    }
}
