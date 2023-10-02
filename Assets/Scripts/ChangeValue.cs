using UnityEngine;

namespace DefaultNamespace
{
    public class ChangeValue
    {
            public bool Check(bool prevValue, bool value)
            {
                return prevValue != value;
            }

            public bool Check(float prevValue, float value, float delta = 0.01f)
            {
                return Mathf.Abs(value - prevValue) > delta;
            }

            public bool Check(Vector3 prevValue, Vector3 value, float delta = 0.01f)
            {
                return (prevValue - value).sqrMagnitude > delta * delta;
            }
            
            public bool Check(Quaternion prevValue, Quaternion value, float delta = 0.01f)
            {
                return Quaternion.Angle(prevValue, value) > delta;
            }
    }
}