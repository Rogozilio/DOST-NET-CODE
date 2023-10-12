using HurricaneVR.Framework.Core.Grabbers;
using Mirror;
using UnityEngine;

public class NetworkSocket : NetworkBehaviour
{
    public HVRSocket hvrSocket;
    private void Awake()
    {
        if (!hvrSocket)
        {
            Debug.LogError("HVRSocket not found");
            return;
        }
        
        hvrSocket.enabled = false;
    }

    public override void OnStartServer()
    {
        hvrSocket.enabled = true;
    }
}
