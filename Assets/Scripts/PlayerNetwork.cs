using System;
using System.Collections.Generic;
using HurricaneVR.Framework.ControllerInput;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.HandPoser;
using HurricaneVR.Framework.Core.Player;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerNetwork : NetworkBehaviour
{
    public GameObject headModel;

    private NetworkIdentity _networkIdentity;
    private HVRManager _hvrManager;

    private Dictionary<int, HVRHandPoser> _handPoses;

    private void Awake()
    {
        _networkIdentity = GetComponent<NetworkIdentity>();
        _hvrManager = FindObjectOfType<HVRManager>();

        _hvrManager.PlayerController ??= GetComponentInChildren<HVRPlayerController>();
    }

    public override void OnStartClient()
    {
        var input = GetComponentInChildren<HVRPlayerInputs>();
        if (input)
            input.enabled = isLocalPlayer;
        GetComponentInChildren<Camera>().enabled = isLocalPlayer;
        var audioListener = GetComponentInChildren<AudioListener>();
        if (audioListener)
            audioListener.enabled = isLocalPlayer;
        var tracPoseDriver = GetComponentInChildren<UnityEngine.SpatialTracking.TrackedPoseDriver>();
        if (tracPoseDriver)
            tracPoseDriver.enabled = isLocalPlayer;
        headModel.layer = LayerMask.NameToLayer("HideForPlayer");

        foreach (var trackedPoseDriver in GetComponentsInChildren<TrackedPoseDriver>())
        {
            trackedPoseDriver.enabled = isLocalPlayer;
        }

        // foreach (var handAnimator in GetComponentsInChildren<HVRHandAnimator>())
        // {
        //     handAnimator.enabled = isLocalPlayer;
        // }

        _handPoses = new Dictionary<int, HVRHandPoser>();
        var poses = FindObjectsOfType<HVRHandPoser>();

        for (var i = 0; i < poses.Length; i++)
        {
            _handPoses.Add(i, poses[i]);
        }
    }

    public void SetAuthority(HVRGrabberBase grabberBase, HVRGrabbable grabbable)
    {
        var networkIdentity = grabbable.GetComponent<NetworkIdentity>();

        if (networkIdentity.isClient)
        {
            CmdRemoveAuthority(networkIdentity);
            CmdSetAuthority(networkIdentity);
        }
        else
        {
            ServerRemoveAuthority(networkIdentity);
            ServerSetAuthority(networkIdentity);
        }
    }

    public void RemoveAuthorityForFullReleased(HVRGrabberBase grabberBase, HVRGrabbable grabbable)
    {
        if (grabbable.IsLeftHandGrabbed && grabbable.IsRightHandGrabbed) return;

        RemoveAuthority(grabberBase, grabbable);
    }

    public void RemoveAuthority(HVRGrabberBase grabberBase, HVRGrabbable grabbable)
    {
        var networkIdentity = grabbable.GetComponent<NetworkIdentity>();
        if (networkIdentity.isClient)
            CmdRemoveAuthority(networkIdentity);
        else
            ServerRemoveAuthority(networkIdentity);
    }

    public void TestDebug(string value)
    {
        Debug.Log(value);
    }

    [Command]
    void CmdSetAuthority(NetworkIdentity grabID)
    {
        grabID.AssignClientAuthority(_networkIdentity.connectionToClient);
    }

    [Command]
    void CmdRemoveAuthority(NetworkIdentity grabID)
    {
        grabID.RemoveClientAuthority();
    }

    [Server]
    void ServerSetAuthority(NetworkIdentity grabID)
    {
        grabID.AssignClientAuthority(_networkIdentity.connectionToClient);
    }

    [Server]
    void ServerRemoveAuthority(NetworkIdentity grabID)
    {
        grabID.RemoveClientAuthority();
    }
}