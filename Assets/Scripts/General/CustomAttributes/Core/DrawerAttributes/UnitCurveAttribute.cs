using System;
using UnityEngine;

namespace CustomAttributes
{
    /// <summary>
    /// Constrain an animation curve from (0, 0) to (1, 1)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class UnitCurveAttribute : CurveRangeAttribute
    {
        public UnitCurveAttribute(EColor color = EColor.Clear)
        {
            Min = Vector2.zero;
            Max = Vector2.one;
            Color = color;
        }
    }
}
