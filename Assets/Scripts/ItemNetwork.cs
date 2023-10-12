using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.Sockets;
using Mirror;
using NetworkAPI;
using UnityEngine;

public class ItemNetwork : NetworkBehaviour
{
    public NetworkAuthority networkAuthority;
    public NetworkSendTransform networkSendTransform;
    public NetworkSendRigidbody networkSendRigidbody;
    [Space] public bool isSyncForceGrabbbale = true;

    private HVRGrabbable _grabbable;
    private HVRTagSocketable _hvrTagSocketable;
    private HVRSocket _hvrSocket;

    [SyncVar(hook = nameof(SyncForceGrabbable))]
    private bool _forceGrabbable;
    private void SyncForceGrabbable(bool oldValue, bool newValue)
    {
        _grabbable.ForceGrabbable = newValue;
    }

    private void Awake()
    {
        _grabbable = GetComponent<HVRGrabbable>();
        _forceGrabbable = _grabbable.ForceGrabbable;
    }
    
    public void Start()
    {
        _hvrTagSocketable = GetComponent<HVRTagSocketable>();
        
        _grabbable.Grabbed.AddListener((a, b) =>
        {
            CmdChangeTagSocket(1, false);
            CmdSyncForceGrabbable(false);
        });

        _grabbable.Released.AddListener((a, b) => { CmdSyncForceGrabbable(true); });
        _grabbable.Released.AddListener(networkAuthority.RemoveAuthorityForFullReleased);//Для удаления прав при отпускании во время полета к руке

        _grabbable.HandGrabbed.AddListener((a, b) => { CmdSyncForceGrabbable(false); });
        
        _grabbable.HandReleased.AddListener((a, b) => { CmdChangeTagSocket(1, true); CmdSyncForceGrabbable(true); });
        
        _grabbable.Socketed.AddListener((a, b) =>
        {
            _hvrSocket = a;
            _forceGrabbable = false;
        });
        _grabbable.UnSocketed.AddListener((a, b) => { _forceGrabbable = true; });
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeTagSocket(int index, bool value)
    {
        if(!_hvrTagSocketable) return;
        
        _hvrTagSocketable.Tags[index] = value;
        
        if(!_hvrSocket || value) return;
        _hvrSocket.ReleaseGrabbable(_grabbable);
        GetComponent<Rigidbody>().isKinematic = false;
        RpcIsKinematicValue(false);
        _hvrSocket = null;
    }

    [ClientRpc]
    private void RpcIsKinematicValue(bool value)
    {
        GetComponent<Rigidbody>().isKinematic = value;
    }
    
    private void Update()
    {
        //Debug.Log(name + " isForceGrabbable = " + _grabbable.ForceGrabbable);
        if (_grabbable.ForceGrabbable && isServer)
        {
            if(networkSendTransform) networkSendTransform.Send();
            if(networkSendRigidbody) networkSendRigidbody.Send();
            Debug.Log("axaxaxaxa");
        }
    }

    public void OnDestroy()
    {
        _grabbable.Grabbed.RemoveListener((a, b) => 
        { 
            CmdChangeTagSocket(1, false);
            CmdSyncForceGrabbable(false); 
        });
        
        _grabbable.Released.RemoveListener((a, b) => { CmdSyncForceGrabbable(true); });
        _grabbable.Released.RemoveListener(networkAuthority.RemoveAuthorityForFullReleased);
        
        _grabbable.HandGrabbed.RemoveListener((a, b) => {  CmdSyncForceGrabbable(false); });
        
        _grabbable.HandReleased.RemoveListener((a, b) =>
        {
            CmdChangeTagSocket(1, true);
            CmdSyncForceGrabbable(true);
        });
        
        _grabbable.Socketed.RemoveListener((a, b) =>
        {
            _hvrSocket = a;
            _forceGrabbable = false;
        });
        _grabbable.UnSocketed.RemoveListener((a, b) => { _forceGrabbable = true; });
    }

    [Command(requiresAuthority = false)]
    public void CmdSyncForceGrabbable(bool value)
    {
        if(!isSyncForceGrabbbale) return;
        
        _grabbable.ForceGrabbable = _forceGrabbable = value;
    }
}