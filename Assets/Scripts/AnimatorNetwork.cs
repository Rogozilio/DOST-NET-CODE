using System.Collections.Generic;
using System.Linq;
using HurricaneVR.Framework.Core.HandPoser;
using Mirror;
using UnityEngine;

public class AnimatorNetwork : NetworkBehaviour
{
    public Transform leftHand;
    public Transform rightHand;

    private List<Transform> _fingersLeftHand;
    private List<Transform> _fingersRightHand;

    private readonly SyncList<Quaternion> _syncFingersLeftHand = new SyncList<Quaternion>();
    private readonly SyncList<Quaternion> _syncFingersRightHand = new SyncList<Quaternion>();

    private void Awake()
    {
        if (!leftHand || !rightHand)
        {
            Debug.LogError("LeftHand or RightHand not init.");
            return;
        }
        
        _fingersLeftHand ??= new List<Transform>();
        _fingersRightHand ??= new List<Transform>();

        GetAllChildTransform(leftHand.transform, ref _fingersLeftHand);
        GetAllChildTransform(rightHand.transform, ref _fingersRightHand);
    }

    public override void OnStartClient()
    {
        _syncFingersLeftHand.Callback += OnSyncFingersLeftHandUpdated;
        _syncFingersRightHand.Callback += OnSyncFingersRightHandUpdated;
    }

    private void Update()
    {
        if(isServer) return;

        var fingersLeft = new Quaternion[_fingersLeftHand.Count];
        var fingersRight = new Quaternion[_fingersRightHand.Count];
        for (var i = 0; i < _fingersLeftHand.Count; i++)
        {
            fingersLeft[i] = _fingersLeftHand[i].rotation;
            fingersRight[i] = _fingersRightHand[i].rotation;
        }

        if (isClient)
        {
            CmdSetFingers(fingersLeft, fingersRight);
        }
        else
        {
            ServerSetFingers(fingersLeft, fingersRight);
        }
    }

    private void GetAllChildTransform(Transform value, ref List<Transform> list)
    {
        foreach (Transform nextTransform in value)
        {
            GetAllChildTransform(nextTransform, ref list);
            list.Add(nextTransform);
        } 
    }

    [Server]
    private void ServerSetFingers(Quaternion[] fingersLeft, Quaternion[] fingersRight)
    {
        for (var i = 0; i < _fingersLeftHand.Count; i++)
        {
            _fingersLeftHand[i].rotation = fingersLeft[i];
            _fingersRightHand[i].rotation = fingersRight[i];
        }
        
        if(_syncFingersLeftHand.Count == 0)
            _syncFingersLeftHand.AddRange(new Quaternion[_fingersLeftHand.Count]);
        if(_syncFingersRightHand.Count == 0)
            _syncFingersRightHand.AddRange(new Quaternion[_fingersRightHand.Count]);
        
        for (var i = 0; i < fingersLeft.Length; i++)
        {
            _syncFingersLeftHand[i] = fingersLeft[i];
        }
        for (var i = 0; i < fingersLeft.Length; i++)
        {
            _syncFingersRightHand[i] = fingersRight[i];
        }
    }
    
    [Command(requiresAuthority = false)]
    private void CmdSetFingers(Quaternion[] fingersLeft, Quaternion[] fingersRight)
    {
        ServerSetFingers(fingersLeft, fingersRight);
    }
    
    void OnSyncFingersLeftHandUpdated(SyncList<Quaternion>.Operation op, int index, Quaternion oldValue, Quaternion newValue)
    {
        switch (op)
        {
            case SyncList<Quaternion>.Operation.OP_ADD:
                break;
            case SyncList<Quaternion>.Operation.OP_INSERT:
                break;
            case SyncList<Quaternion>.Operation.OP_REMOVEAT:
                break;
            case SyncList<Quaternion>.Operation.OP_SET:
                _fingersLeftHand[index].rotation = newValue;
                break;
            case SyncList<Quaternion>.Operation.OP_CLEAR:
                break;
        }
    }

    void OnSyncFingersRightHandUpdated(SyncList<Quaternion>.Operation op, int index, Quaternion oldValue, Quaternion newValue)
    {
        switch (op)
        {
            case SyncList<Quaternion>.Operation.OP_ADD:
                break;
            case SyncList<Quaternion>.Operation.OP_INSERT:
                break;
            case SyncList<Quaternion>.Operation.OP_REMOVEAT:
                break;
            case SyncList<Quaternion>.Operation.OP_SET:
                _fingersRightHand[index].rotation = newValue;
                break;
            case SyncList<Quaternion>.Operation.OP_CLEAR:
                break;
        }
    }
}