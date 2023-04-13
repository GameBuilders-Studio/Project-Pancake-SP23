using System;
using UnityEngine;

namespace CustomAttributes
{
    /// <summary>
    /// Select an Animator parameter with a dropdown interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class AnimatorParamAttribute : DrawerAttribute
    {
        public string AnimatorName { get; private set; }
        public AnimatorControllerParameterType? AnimatorParamType { get; private set; }

        public AnimatorParamAttribute(string animatorName)
        {
            AnimatorName = animatorName;
            AnimatorParamType = null;
        }

        public AnimatorParamAttribute(string animatorName, AnimatorControllerParameterType animatorParamType)
        {
            AnimatorName = animatorName;
            AnimatorParamType = animatorParamType;
        }
    }
}
