using UnityEngine;
using CustomAttributes;

[CreateAssetMenu(menuName = "Props/ThrowData")]
public class ThrowData : ScriptableObject
{
    [SerializeField]
    public bool IsThrowable = true;

    [Tooltip("Path of the thrown object, where 1 = initial throw height and 0 = ground height")]
    [SerializeField]
    [EnableIf("IsThrowable")]
    [CurveRange(0.0f, 0.0f, 1.0f, 2.0f)]
    public AnimationCurve Trajectory;

    [SerializeField]
    [EnableIf("IsThrowable")]
    [Min(0.0f)]
    public float Distance;

    [SerializeField]
    [EnableIf("IsThrowable")]
    public float ThrowDurationSeconds;

    void OnValidate()
    {
        if (Trajectory.keys.Length < 2)
        {
            Trajectory.MoveKey(0, new Keyframe(0.0f, 1.0f));
            Trajectory.MoveKey(Trajectory.keys.Length - 1, new Keyframe(1.0f, 0.0f));
        }

        var firstKey = Trajectory.keys[0];
        var lastKey = Trajectory.keys[Trajectory.keys.Length - 1];

        Trajectory.MoveKey(0, new Keyframe(0.0f, 1.0f, firstKey.inTangent, firstKey.outTangent));
        Trajectory.MoveKey(Trajectory.keys.Length - 1, new Keyframe(1.0f, 0.0f, lastKey.inTangent, lastKey.outTangent));
    }
}

