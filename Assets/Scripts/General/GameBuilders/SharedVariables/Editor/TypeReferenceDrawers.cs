using UnityEditor;

namespace GameBuilders.Variables
{
    [CustomPropertyDrawer(typeof(FloatReference), true)]
    public class FloatReferenceDrawer : VariableReferenceDrawer<FloatVariable> {}

    [CustomPropertyDrawer(typeof(BoolReference), true)]
    public class BoolReferenceDrawer : VariableReferenceDrawer<BoolVariable> {}
}