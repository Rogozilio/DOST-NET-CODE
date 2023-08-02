using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.Player;
using HurricaneVR.Framework.Shared;
using Mirror;
using NetworkAPI;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public GameObject headModel;
    [Space] public NetworkAuthority networkAuthority;
    public NetworkTakeAndDrop networkTakeAndDrop;
    public NetworkSendTransform sendTransformItemForceGrab;

    private HVRManager _hvrManager;

    private HVRHandGrabber _leftHand;
    private HVRHandGrabber _rightHand;
    private HVRForceGrabber _leftForceHand;
    private HVRForceGrabber _rightForceHand;

    private void Awake()
    {
        headModel.layer = LayerMask.NameToLayer("HideForPlayer");
        networkAuthority.networkIdentity = GetComponent<NetworkIdentity>();
    }

    public override void OnStartLocalPlayer()
    {
        _leftHand = networkTakeAndDrop.leftHandRigidbody.GetComponent<HVRHandGrabberNetwork>();
        _rightHand = networkTakeAndDrop.rightHandRigidbody.GetComponent<HVRHandGrabberNetwork>();
        _leftForceHand = networkTakeAndDrop.leftHandRigidbody.GetComponentInChildren<HVRForceGrabber>();
        _rightForceHand = networkTakeAndDrop.rightHandRigidbody.GetComponentInChildren<HVRForceGrabber>();

        _hvrManager = FindObjectOfType<HVRManager>();

        _hvrManager.PlayerController ??= GetComponentInChildren<HVRPlayerController>();

        _leftForceHand.Grabbed.AddListener(networkAuthority.SetAuthority);
        _leftForceHand.Grabbed.AddListener(networkTakeAndDrop.ForceGrabObject);

        _leftHand.Grabbed.AddListener(networkAuthority.SetAuthority);
        _leftHand.Grabbed.AddListener(networkTakeAndDrop.TakeObject);
        _leftHand.Released.AddListener(networkTakeAndDrop.DropObject);

        _rightForceHand.Grabbed.AddListener(networkAuthority.SetAuthority);
        _rightForceHand.Grabbed.AddListener(networkTakeAndDrop.ForceGrabObject);

        _rightHand.Grabbed.AddListener(networkAuthority.SetAuthority);
        _rightHand.Grabbed.AddListener(networkTakeAndDrop.TakeObject);
        _rightHand.Released.AddListener(networkTakeAndDrop.DropObject);
    }

    private void Update()
    {
        if (isServer) return;
        
        if (networkTakeAndDrop.IsForceLeftHand)
            sendTransformItemForceGrab.Send(networkAuthority.netId, networkTakeAndDrop.GetItemInLeftHand,
                networkTakeAndDrop.GetItemInLeftHand.transform.position,
                networkTakeAndDrop.GetItemInLeftHand.transform.rotation);
        
        if (networkTakeAndDrop.IsForceRightHand)
            sendTransformItemForceGrab.Send(networkAuthority.netId, networkTakeAndDrop.GetItemInRightHand,
                networkTakeAndDrop.GetItemInRightHand.transform.position,
                networkTakeAndDrop.GetItemInRightHand.transform.rotation);
    }

    public override void OnStopLocalPlayer()
    {
        _leftForceHand.Grabbed.RemoveListener(networkAuthority.SetAuthority);
        _leftForceHand.Grabbed.RemoveListener(networkTakeAndDrop.ForceGrabObject);
        
        _leftHand.Grabbed.RemoveListener(networkAuthority.SetAuthority);
        _leftHand.Grabbed.RemoveListener(networkTakeAndDrop.TakeObject);
        _leftHand.Released.RemoveListener(networkTakeAndDrop.DropObject);

        _rightForceHand.Grabbed.RemoveListener(networkAuthority.SetAuthority);
        _rightForceHand.Grabbed.RemoveListener(networkTakeAndDrop.ForceGrabObject);
        
        _rightHand.Grabbed.RemoveListener(networkAuthority.SetAuthority);
        _rightHand.Grabbed.RemoveListener(networkTakeAndDrop.TakeObject);
        _rightHand.Released.RemoveListener(networkTakeAndDrop.DropObject);
    }

    public void TestDebug(string value)
    {
        Debug.Log(value);
    }
}