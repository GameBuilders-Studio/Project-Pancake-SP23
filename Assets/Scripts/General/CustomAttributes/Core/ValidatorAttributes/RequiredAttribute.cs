using System;

namespace CustomAttributes
{
    /// <summary>
    /// Reminds the user that a given reference type field is required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class RequiredAttribute : ValidatorAttribute
    {
        public string Message { get; private set; }

        public RequiredAttribute(string message = null)
        {
            Message = message;
        }
    }
}
