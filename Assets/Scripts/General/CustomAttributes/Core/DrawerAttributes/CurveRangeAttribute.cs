using System;
using UnityEngine;

namespace CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class CurveRangeAttribute : DrawerAttribute
    {
        public Vector2 Min { get; protected set; }
        public Vector2 Max { get; protected set; }
        public EColor Color { get; protected set; }

        public CurveRangeAttribute(Vector2 min, Vector2 max, EColor color = EColor.Clear)
        {
            Min = min;
            Max = max;
            Color = color;
        }

        public CurveRangeAttribute()
            : this(Vector2.zero, Vector2.one, EColor.Clear)
        {
        }

        public CurveRangeAttribute(EColor color)
            : this(Vector2.zero, Vector2.one, color)
        {
        }

        public CurveRangeAttribute(float minX, float minY, float maxX, float maxY, EColor color = EColor.Clear)
            : this(new Vector2(minX, minY), new Vector2(maxX, maxY), color)
        {
        }
    }
}
