using System.Collections;
using System.Collections.Generic;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Shared;
using UnityEngine;

public class HVRHandGrabberNetwork : HVRHandGrabber
{
    private bool isWaitDrop;

    public bool IsWaitDrop => isWaitDrop;
    protected override void GrabGrabbable(HVRGrabberBase grabber, HVRGrabbable grabbable, bool raiseEvents = true)
    {
        var isLeftHand = grabber.GetComponent<HVRHandGrabber>().HandSide == HVRHandSide.Left;
        foreach (var configurable in grabbable.GetComponents<ConfigurableJointWithHand>())
        {
            if (configurable.isLeftHand == isLeftHand)
            {
                Grabbed.Invoke(grabber, grabbable);
                isWaitDrop = true;
                Debug.Log("Not Take Object");
                return;
            }
        }
        
        isWaitDrop = false;
        base.GrabGrabbable(grabber, grabbable, raiseEvents);
    }
}
