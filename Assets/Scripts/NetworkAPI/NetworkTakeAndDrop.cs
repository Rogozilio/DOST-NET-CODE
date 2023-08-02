using System;
using System.Collections;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Shared;
using Mirror;
using UnityEngine;

namespace NetworkAPI
{
    public class NetworkTakeAndDrop : NetworkBehaviour
    {
        public struct DataJoint
        {
            public Vector3 handPosition;
            public Quaternion handRotation;
            public Vector3 itemPosition;
            public Quaternion itemRotation;
            public Vector3 anchor;
            public Vector3 axis;
            public Vector3 connectedAnchor;
            public Vector3 secondaryAxis;
        }

        public Rigidbody leftHandRigidbody;
        public Rigidbody rightHandRigidbody;

        private bool _isForceLeftHand;
        private bool _isForceRightHand;

        private GameObject _itemInLeftHand;
        private GameObject _itemInRightHand;

        public GameObject GetItemInLeftHand => _itemInLeftHand;
        public GameObject GetItemInRightHand => _itemInRightHand;
        public bool IsForceLeftHand => _isForceLeftHand;
        public bool IsForceRightHand => _isForceRightHand;

        public void ForceGrabObject(HVRGrabberBase grabberBase, HVRGrabbable grabbable)
        {
            if (IsLeftHand(grabberBase))
            {
                _isForceLeftHand = true;
                _itemInLeftHand = grabbable.gameObject;
            }
            else
            {
                _isForceLeftHand = true;
                _itemInRightHand = grabbable.gameObject;
            }
        }

        [Client]
        public void TakeObject(HVRGrabberBase grabberBase, HVRGrabbable grabbable)
        {
            var isLeftHand = IsLeftHand(grabberBase);
            if (isLeftHand)
            {
                _isForceLeftHand = false;
                _itemInLeftHand = grabbable.gameObject;
            }
            else
            {
                _isForceLeftHand = false;
                _itemInRightHand = grabbable.gameObject;
            }

            if (DropObjectPastOwner(grabberBase, grabbable)) { Debug.Log("DropPastObject"); return; }

            Debug.Log("TakeObject");
            StartCoroutine(DelayTakeObject(grabberBase, grabbable, 0.1f));
        }

        private IEnumerator DelayTakeObject(HVRGrabberBase grabberBase, HVRGrabbable grabbable, float second)
        {
            yield return new WaitForSeconds(second);

            var joint = grabbable.gameObject.GetComponents<ConfigurableJoint>()[^1];
            var dataJoint = new DataJoint()
            {
                handPosition = grabberBase.transform.position,
                handRotation = grabberBase.transform.rotation,
                itemPosition = grabbable.transform.position,
                itemRotation = grabbable.transform.rotation,
                anchor = joint.anchor,
                axis = joint.axis,
                connectedAnchor = joint.connectedAnchor,
                secondaryAxis = joint.secondaryAxis
            };
            var isLeftHand = grabberBase.GetComponent<HVRHandGrabber>().HandSide == HVRHandSide.Left;

            var jointWithHand = grabbable.gameObject.AddComponent<ConfigurableJointWithHand>();
            jointWithHand.isLeftHand = isLeftHand;
            jointWithHand.joint = joint;

            CmdTakeObject(netId, isLeftHand, grabbable.gameObject, dataJoint);
        }

        [Command]
        private void CmdTakeObject(uint id, bool isLeftHand, GameObject item, DataJoint dataJoint)
        {
            if (isOwned) return;
            
            CreateJoint(isLeftHand, item, dataJoint);
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn.identity.netId == id) continue;
                RpcTakeObject(conn, isLeftHand, item, dataJoint);
            }
        }

        [TargetRpc]
        private void RpcTakeObject(NetworkConnection conn, bool isLeftHand, GameObject item, DataJoint dataJoint)
        {
            CreateJoint(isLeftHand, item, dataJoint);
        }

        private void CreateJoint(bool isLeftHand, GameObject item, DataJoint dataJoint)
        {
            item.transform.localRotation = dataJoint.itemRotation;
            item.transform.position = dataJoint.itemPosition;
            if (isLeftHand)
            {
                leftHandRigidbody.transform.position = dataJoint.handPosition;
                leftHandRigidbody.transform.rotation = dataJoint.handRotation;
            }
            else
            {
                rightHandRigidbody.transform.position = dataJoint.handPosition;
                rightHandRigidbody.transform.rotation = dataJoint.handRotation;
            }

            var jointWithHand = item.AddComponent<ConfigurableJointWithHand>();
            jointWithHand.Init();
            jointWithHand.isLeftHand = isLeftHand;
            jointWithHand.joint.anchor = dataJoint.anchor;
            jointWithHand.joint.axis = dataJoint.axis;
            jointWithHand.joint.autoConfigureConnectedAnchor = false;
            jointWithHand.joint.connectedAnchor = dataJoint.connectedAnchor;
            jointWithHand.joint.secondaryAxis = dataJoint.secondaryAxis;
            jointWithHand.joint.xMotion = ConfigurableJointMotion.Locked;
            jointWithHand.joint.yMotion = ConfigurableJointMotion.Locked;
            jointWithHand.joint.zMotion = ConfigurableJointMotion.Locked;
            jointWithHand.joint.rotationDriveMode = RotationDriveMode.Slerp;
            jointWithHand.joint.projectionMode = JointProjectionMode.PositionAndRotation;
            var jointDrive = new JointDrive()
            {
                positionSpring = 100000,
                positionDamper = 1000,
                maximumForce = 100000
            };
            jointWithHand.joint.angularXDrive = jointDrive;
            jointWithHand.joint.angularYZDrive = jointDrive;
            jointWithHand.joint.slerpDrive = jointDrive;
            jointWithHand.joint.projectionDistance = 0.01f;
            jointWithHand.joint.projectionAngle = 0.01f;

            jointWithHand.joint.connectedBody = isLeftHand ? leftHandRigidbody : rightHandRigidbody;
        }

        [Client]
        public void DropObject(HVRGrabberBase grabberBase, HVRGrabbable grabbable)
        {
            var isLeftHand = grabberBase.GetComponent<HVRHandGrabber>().HandSide == HVRHandSide.Left;

            RemoveJoint(isLeftHand, grabbable.gameObject);
            CmdDropObject(netId, isLeftHand, grabbable.gameObject);
            if (isLeftHand)
                _itemInLeftHand = null;
            else
                _itemInRightHand = null;
        }

        [Command]
        private void CmdDropObject(uint id, bool isLeftHand, GameObject item)
        {
            RemoveJoint(isLeftHand, item);
            
            ServerSendOtherPlayers(id, (conn) =>
            {
                RpcDropObject(conn, isLeftHand, item);
            });
        }

        [TargetRpc]
        private void RpcDropObject(NetworkConnection conn, bool isLeftHand, GameObject item)
        {
            RemoveJoint(isLeftHand, item);
            
            var player = GameObject.FindGameObjectWithTag("Player");

            foreach (var handGrabber in player.GetComponentsInChildren<HVRHandGrabberNetwork>())
            {
                if (handGrabber.HandSide == HVRHandSide.Left && isLeftHand && handGrabber.IsWaitDrop ||
                    handGrabber.HandSide == HVRHandSide.Right && !isLeftHand)
                {
                    Debug.Log("Drop -> Take");
                    handGrabber.TryGrab(item.GetComponent<HVRGrabbable>());
                }
            }
        }

        [Client]
        private bool DropObjectPastOwner(HVRGrabberBase grabberBase, HVRGrabbable grabbable)
        {
            var isLeftHand = IsLeftHand(grabberBase);
            foreach (var jointWithHand in grabbable.GetComponents<ConfigurableJointWithHand>())
            {
                if (jointWithHand.isLeftHand != isLeftHand) continue;
                var pastOwnerPlayer = jointWithHand.joint.connectedBody.transform.parent;
                var netIdPastOwner = pastOwnerPlayer.GetComponent<NetworkIdentity>().netId;
                CmdDropObjectPastOwner(netIdPastOwner, isLeftHand, grabbable.gameObject);
                return true;
            }

            return false;
        }

        [Command]
        private void CmdDropObjectPastOwner(uint netIdPastOwner, bool isLeftHand, GameObject item)
        {
            ServerSendPlayer(netIdPastOwner, (conn) =>
            {
                RpcDropObjectPastOwner(conn, isLeftHand, item);
            });
        }

        [TargetRpc]
        private void RpcDropObjectPastOwner(NetworkConnection conn, bool isLeftHand,
            GameObject item)
        {
            var player = GameObject.FindGameObjectWithTag("Player");

            foreach (var handGrabber in player.GetComponentsInChildren<HVRHandGrabber>())
            {
                if (handGrabber.HandSide == HVRHandSide.Left && isLeftHand ||
                    handGrabber.HandSide == HVRHandSide.Right && !isLeftHand)
                {
                    Debug.Log(name + " DropObjectPastOwner " + item.name);
                    handGrabber.ForceRelease();
                }
            }
        }

        private void RemoveJoint(bool isLeftHand, GameObject item)
        {
            foreach (var joint in item.GetComponents<ConfigurableJointWithHand>())
            {
                if (isLeftHand == joint.isLeftHand)
                {
                    DestroyImmediate(joint);
                    Debug.Log(name + " Drop " + item.name);
                }
            }
        }
        
        private bool IsLeftHand(HVRGrabberBase grabberBase)
        {
            if (!grabberBase.TryGetComponent(out HVRHandGrabber handGrabber))
                handGrabber = grabberBase.transform.parent.GetComponent<HVRHandGrabber>();

            return handGrabber.HandSide == HVRHandSide.Left;
        }

        private void ServerSendOtherPlayers(uint exceptLocalPlayerNetId, Action<NetworkConnectionToClient> action)
        {
            ServerSend(exceptLocalPlayerNetId, action, Send.AllExceptSelf);
        }
        
        private void ServerSendPlayer(uint playerNetId, Action<NetworkConnectionToClient> action)
        {
            ServerSend(playerNetId, action, Send.Single);
        }

        private enum Send
        {
            Single,
            AllExceptSelf
        }

        private void ServerSend(uint netId, Action<NetworkConnectionToClient> action, Send send)
        {
            if (!isServer)
            {
                Debug.LogError("ServerSend is only called on the server");
                return;
            }
            
            foreach (var conn in NetworkServer.connections.Values)
            {
                switch (send)
                {
                    case Send.Single:
                        if (conn.identity.netId != netId) continue;
                        break;
                    case Send.AllExceptSelf:
                        if (conn.identity.netId == netId) continue;
                        break;
                }
                action?.Invoke(conn);
            }
        }
    }
}