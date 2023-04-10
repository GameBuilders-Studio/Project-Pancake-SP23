using System;

namespace CustomAttributes
{
    /// <summary>
    /// Can be used to make serialized fields or buttons readonly based on some condition. The condition can be a field, property or function.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DisableIfAttribute : EnableIfAttributeBase
    {
        public DisableIfAttribute(string condition)
            : base(condition)
        {
            Inverted = true;
        }

        public DisableIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
            : base(conditionOperator, conditions)
        {
            Inverted = true;
        }

        public DisableIfAttribute(string enumName, object enumValue)
            : base(enumName, enumValue as Enum)
        {
            Inverted = true;
        }
    }
}
