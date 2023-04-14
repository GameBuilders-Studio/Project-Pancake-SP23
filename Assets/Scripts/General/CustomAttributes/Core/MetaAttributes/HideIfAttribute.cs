using System;

namespace CustomAttributes
{
    /// <summary>
    /// Can be used to show/hide serialized fields or buttons based on a condition. The condition can be a field, property or function.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HideIfAttribute : ShowIfAttributeBase
    {
        public HideIfAttribute(string condition)
            : base(condition)
        {
            Inverted = true;
        }

        public HideIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
            : base(conditionOperator, conditions)
        {
            Inverted = true;
        }

        public HideIfAttribute(string enumName, object enumValue)
            : base(enumName, enumValue as Enum)
        {
            Inverted = true;
        }
    }
}
