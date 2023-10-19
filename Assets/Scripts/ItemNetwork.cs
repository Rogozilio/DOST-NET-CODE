using System.Collections.Generic;
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
    private List<int> _indexsTag;

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
        _indexsTag = new List<int>();
        _hvrTagSocketable = GetComponent<HVRTagSocketable>();

        if (_hvrTagSocketable)
            for (var i = 0; i < 31; i++)
            {
                if (!_hvrTagSocketable.Tags[i]) continue;
                _indexsTag.Add(i);
            }

        _grabbable.Grabbed.AddListener((a, b) =>
        {
            if(isServer) return;
            
            if (_hvrTagSocketable)
            {
                CmdChangeTagSocket(_indexsTag, false);
            }

            CmdSyncForceGrabbable(false);
        });

        _grabbable.Released.AddListener((a, b) => { CmdSyncForceGrabbable(true); });
        _grabbable.Released.AddListener(networkAuthority
            .RemoveAuthorityForFullReleased); //Для удаления прав при отпускании во время полета к руке

        _grabbable.HandGrabbed.AddListener((a, b) => { CmdSyncForceGrabbable(false); });

        _grabbable.HandReleased.AddListener((a, b) =>
        {
            CmdChangeTagSocket(_indexsTag, true);
            CmdSyncForceGrabbable(true);
        });

        _grabbable.Socketed.AddListener((a, b) =>
        {
            _hvrSocket = a;
            SyncForceGrabbable(false);
        });
        _grabbable.UnSocketed.AddListener((a, b) =>
        {
            SyncForceGrabbable(true);
        });
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeTagSocket(List<int> indexs, bool value)
    {
        if (!_hvrTagSocketable) return;

        foreach (var index in indexs)
        {
            _hvrTagSocketable.Tags[index] = value;
        }

        if (!_hvrSocket || value) return;
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
        Debug.Log(name + " isForceGrabbable = " + _grabbable.ForceGrabbable);
        if (_grabbable.ForceGrabbable && isServer)
        {
            if (networkSendTransform) networkSendTransform.Send();
            if (networkSendRigidbody) networkSendRigidbody.Send();
            Debug.Log("axaxaxaxa");
        }
    }

    public void OnDestroy()
    {
        _grabbable.Grabbed.RemoveListener((a, b) =>
        {
            if (_hvrTagSocketable)
            {
                CmdChangeTagSocket(_indexsTag, false);
            }

            CmdSyncForceGrabbable(false);
        });

        _grabbable.Released.RemoveListener((a, b) => { CmdSyncForceGrabbable(true); });
        _grabbable.Released.RemoveListener(networkAuthority.RemoveAuthorityForFullReleased);

        _grabbable.HandGrabbed.RemoveListener((a, b) => { CmdSyncForceGrabbable(false); });

        _grabbable.HandReleased.RemoveListener((a, b) =>
        {
            CmdChangeTagSocket(_indexsTag, true);
            CmdSyncForceGrabbable(true);
        });

        _grabbable.Socketed.RemoveListener((a, b) =>
        {
            _hvrSocket = a;
            SyncForceGrabbable(false);
        });
        _grabbable.UnSocketed.RemoveListener((a, b) =>
        {
            SyncForceGrabbable(true);
        });
    }

    [Command(requiresAuthority = false)]
    public void CmdSyncForceGrabbable(bool value)
    {
        SyncForceGrabbable(value);
    }

    private void SyncForceGrabbable(bool value)
    {
        if (!isSyncForceGrabbbale) return;

        _grabbable.ForceGrabbable = _forceGrabbable = value;
    }
}