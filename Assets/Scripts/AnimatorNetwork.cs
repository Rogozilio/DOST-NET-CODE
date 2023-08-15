using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class AnimatorNetwork : NetworkBehaviour
{
    public bool isRecipient;
    public Transform leftHand;
    public Transform rightHand;

    private List<Transform> _fingersLeftHand = new List<Transform>();
    private List<Transform> _fingersRightHand = new List<Transform>();
    private Quaternion[] _dataFingersLeftHand;
    private Quaternion[] _dataFingersRightHand;

    private void Awake()
    {
        if (!leftHand || !rightHand)
        {
            Debug.LogError("LeftHand or RightHand not init.");
            return;
        }

        GetAllChildTransform(leftHand.transform, ref _fingersLeftHand);
        GetAllChildTransform(rightHand.transform, ref _fingersRightHand);

        _dataFingersLeftHand = new Quaternion[_fingersLeftHand.Count];
        _dataFingersRightHand = new Quaternion[_fingersRightHand.Count];
    }

    private void Update()
    {
        if (isServer || isRecipient) return;

        for (var i = 0; i < _fingersLeftHand.Count; i++)
        {
            _dataFingersLeftHand[i] = _fingersLeftHand[i].localRotation;
            _dataFingersRightHand[i] = _fingersRightHand[i].localRotation;
        }

        CmdSendFingers(_dataFingersLeftHand, _dataFingersRightHand);
    }

    [Command]
    private void CmdSendFingers(Quaternion[] fingersLeft, Quaternion[] fingersRight)
    {
        SendFingers(fingersLeft, fingersRight);

        for (var i = 0; i < _fingersLeftHand.Count; i++)
        {
            _fingersLeftHand[i].localRotation = fingersLeft[i];
            _fingersRightHand[i].localRotation = fingersRight[i];
        }
    }

    [ClientRpc]
    private void SendFingers(Quaternion[] fingersLeft, Quaternion[] fingersRight)
    {
        for (var i = 0; i < _fingersLeftHand.Count; i++)
        {
            _fingersLeftHand[i].localRotation = fingersLeft[i];
            _fingersRightHand[i].localRotation = fingersRight[i];
        }
    }

    private void GetAllChildTransform(Transform value, ref List<Transform> list)
    {
        list.AddRange(value.GetComponentsInChildren<Transform>());
    }
}