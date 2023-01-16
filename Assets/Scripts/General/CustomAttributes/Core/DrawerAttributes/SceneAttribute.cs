using System;

namespace CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SceneAttribute : DrawerAttribute
    {
    }
}