using HurricaneVR.Framework.Core;
using Mirror;
using NetworkAPI;
using UnityEngine;

public class ItemNetwork : NetworkBehaviour
{
    public NetworkAuthority networkAuthority;
    public NetworkSendTransform networkSendTransform;

    private HVRGrabbable _grabbable;

    [SyncVar(hook = nameof(SyncForceGrabbable))]
    private bool _forceGrabbable;
    private void SyncForceGrabbable(bool oldValue, bool newValue)
    {
        _grabbable.ForceGrabbable = newValue;
    }

    private void Awake()
    {
        _grabbable = GetComponent<HVRGrabbable>();
    }
    
    public void Start()
    {
        _forceGrabbable = _grabbable.ForceGrabbable;
        
        _grabbable.Grabbed.AddListener((a, b) => { SyncForceGrabbable(false); });

        _grabbable.Released.AddListener((a, b) => { SyncForceGrabbable(true); });
        _grabbable.Released.AddListener(networkAuthority.RemoveAuthorityForFullReleased);//Для удаления прав при отпускании во время полета к руке

        _grabbable.HandGrabbed.AddListener((a, b) => { SyncForceGrabbable(false); });
        
        _grabbable.HandReleased.AddListener((a, b) => { SyncForceGrabbable(true); });
    }

    private void Update()
    {
        Debug.Log(name + " isForceGrabbable = " + _grabbable.ForceGrabbable);
        if (_grabbable.ForceGrabbable && isServer)
        {
            networkSendTransform.Send();
        }
    }

    public void OnDestroy()
    {
        _grabbable.Grabbed.RemoveListener((a, b) => { SyncForceGrabbable(false); });
        
        _grabbable.Released.RemoveListener((a, b) => { SyncForceGrabbable(true); });
        _grabbable.Released.RemoveListener(networkAuthority.RemoveAuthorityForFullReleased);
        
        _grabbable.HandGrabbed.RemoveListener((a, b) => { SyncForceGrabbable(false); });
        
        _grabbable.HandReleased.RemoveListener((a, b) => { SyncForceGrabbable(true); });
    }

    [Command(requiresAuthority = false)]
    public void SyncForceGrabbable(bool value)
    {
        _grabbable.ForceGrabbable = _forceGrabbable = value;
    }
}