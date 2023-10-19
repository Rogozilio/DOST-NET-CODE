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
        foreach (var configurable in grabbable.GetComponents<ConfigurableJointWithHand>())
        {
            if (!configurable.joint.connectedBody.CompareTag(grabber.tag))
            {
                Debug.Log("Wait Drop");
                Grabbed.Invoke(grabber, grabbable);
                isWaitDrop = true;
                return;
            }
        }
        
        isWaitDrop = false;
        base.GrabGrabbable(grabber, grabbable, raiseEvents);
    }
}
