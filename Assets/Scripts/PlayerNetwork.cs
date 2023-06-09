using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Serialization;

public class PlayerNetwork : NetworkBehaviour
{
    public GameObject head;
    public GameObject headModel;
    public TrackedPoseDriver leftHandInput;
    public TrackedPoseDriver rightHandInput;

    public override void OnStartClient()
    {
        if(!isLocalPlayer) return;
        
        head.GetComponent<Camera>().enabled = true;
        head.GetComponent<AudioListener>().enabled = true;
        head.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>().enabled = true;
        headModel.layer = LayerMask.NameToLayer("HideForPlayer");

        leftHandInput.enabled = true;
        rightHandInput.enabled = true;
    }
}
