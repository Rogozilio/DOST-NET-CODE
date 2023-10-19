using DefaultNamespace;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.Player;
using Mirror;
using NetworkAPI;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkAuthority networkAuthority;
    public NetworkTakeAndDrop networkTakeAndDrop;
    public NetworkSendTransform sendTransformItemForceGrab;
    [Space][Header("Backpack")] 
    public GameObject prefabLeftShoulderForBackpack;
    public Transform leftShoulderPlayer;
    
    [SyncVar]
    private uint _backpackNetId;

    private HVRManager _hvrManager;

    private HVRHandGrabber _leftHand;
    private HVRHandGrabber _rightHand;
    private HVRForceGrabber _leftForceHand;
    private HVRForceGrabber _rightForceHand;

    public uint SetBackpackNetId
    {
        set => _backpackNetId = value;
    }

    private void Awake()
    {
        networkAuthority.networkIdentity = GetComponent<NetworkIdentity>();
        var leftShoulderForBackpack = Instantiate(prefabLeftShoulderForBackpack);
        leftShoulderForBackpack.GetComponent<ConnectTransform>().target = leftShoulderPlayer;
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
        _leftHand.GrabbedFinish.AddListener(networkTakeAndDrop.RefreshJoint);
        _leftHand.Released.AddListener(networkTakeAndDrop.DropObject);
        
        _rightForceHand.Grabbed.AddListener(networkAuthority.SetAuthority);
        _rightForceHand.Grabbed.AddListener(networkTakeAndDrop.ForceGrabObject);
        
        _rightHand.Grabbed.AddListener(networkAuthority.SetAuthority);
        _rightHand.Grabbed.AddListener(networkTakeAndDrop.TakeObject);
        _rightHand.GrabbedFinish.AddListener(networkTakeAndDrop.RefreshJoint);
        _rightHand.Released.AddListener(networkTakeAndDrop.DropObject);

        var backpack = FindBackpack(_backpackNetId);
        
        if (!backpack)
        {
            Debug.LogError("Backpack not found");
            return;
        }
        
        backpack.GetComponent<NetworkBackpack>().playerId = netId;
        backpack.GetComponent<NetworkBackpack>().ConnectWithLeftShoulderForBackpack();
    }

    private void Update()
    {
        //Send transform item from origin point to left hand
        if (networkTakeAndDrop.IsForceLeftHand)
        {
            sendTransformItemForceGrab.SetTarget = networkTakeAndDrop.GetItemInLeftHand;
            sendTransformItemForceGrab.Send();
        }
        //Send transform item from origin point to right hand
        if (networkTakeAndDrop.IsForceRightHand)
        {
            sendTransformItemForceGrab.SetTarget = networkTakeAndDrop.GetItemInRightHand;
            sendTransformItemForceGrab.Send();
        }
    }

    public override void OnStopLocalPlayer()
    {
        _leftForceHand.Grabbed.RemoveListener(networkAuthority.SetAuthority);
        _leftForceHand.Grabbed.RemoveListener(networkTakeAndDrop.ForceGrabObject);

        _leftHand.Grabbed.RemoveListener(networkAuthority.SetAuthority);
        _leftHand.Grabbed.RemoveListener(networkTakeAndDrop.TakeObject);
        _leftHand.GrabbedFinish.RemoveListener(networkTakeAndDrop.RefreshJoint);
        _leftHand.Released.RemoveListener(networkTakeAndDrop.DropObject);

        _rightForceHand.Grabbed.RemoveListener(networkAuthority.SetAuthority);
        _rightForceHand.Grabbed.RemoveListener(networkTakeAndDrop.ForceGrabObject);

        _rightHand.Grabbed.RemoveListener(networkAuthority.SetAuthority);
        _rightHand.Grabbed.RemoveListener(networkTakeAndDrop.TakeObject);
        _rightHand.GrabbedFinish.RemoveListener(networkTakeAndDrop.RefreshJoint);
        _rightHand.Released.RemoveListener(networkTakeAndDrop.DropObject);
    }

    private GameObject FindBackpack(uint backpackNetId)
    {
        foreach (var backpack in GameObject.FindGameObjectsWithTag("Backpack"))
        {
            if(backpack.GetComponent<NetworkIdentity>().netId != backpackNetId) continue;
            return backpack;
        }

        return null;
    }

    public void TestDebug(string value)
    {
        Debug.Log(value);
    }
}