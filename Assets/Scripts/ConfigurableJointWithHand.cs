using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ConfigurableJointWithHand : MonoBehaviour
{
    public bool isLeftHand;
    public ConfigurableJoint joint;

    public void Init()
    {
        joint = gameObject.AddComponent<ConfigurableJoint>();
    }

    private void OnDestroy()
    {
        Destroy(joint);
    }
}