using System.Collections;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Bags;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.Sockets;
using HurricaneVR.Framework.Core.Utils;
using Mirror;
using NetworkAPI;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace
{
    public class NetworkBackpack : NetworkBehaviour
    {
        public GameObject inventory;
        
        private HVRGrabbable _hvrGrabbable;
        private HVRSocketContainer _hvrSocketContainer;
        private GameObject _leftShoulder;
        
        public void ConnectWithLeftShoulderForBackpack()
        {
            _leftShoulder = GameObject.FindGameObjectsWithTag("LeftShoulder")[^1];
            
            if (!_leftShoulder)
            {
                Debug.LogError("LeftShoulder not found");
                return;
            }
            _hvrGrabbable = GetComponent<HVRGrabbable>();
            _hvrSocketContainer = GetComponent<HVRSocketContainer>();
            
            var hvrSocket = _leftShoulder.GetComponent<HVRShoulderSocket>();
            var hvrSocketFilter = _leftShoulder.GetComponent<HVRGrabbableSocketFilter>();
            var hvrShoulderGrabber = _leftShoulder.GetComponent<HVRShoulderGrabber>();
            
            hvrSocketFilter.ValidGrabbables.Add(_hvrGrabbable);
            hvrShoulderGrabber.SocketContainer = _hvrSocketContainer;
            
            _hvrGrabbable.StartingSocket = hvrSocket;
            transform.position = _leftShoulder.transform.position;

            _hvrGrabbable.LaunchStartEvent();
            
            _hvrGrabbable.Socketed.AddListener((arg1, arg2) => CmdInvokeSocketed(netId));
            _hvrGrabbable.UnSocketed.AddListener((arg1, arg2) => CmdInvokeUnSocketed(netId));
        }

        [Command(requiresAuthority = false)]
        private void CmdInvokeSocketed(uint exceptNetId)
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            inventory.SetActive(false);
            transform.position = default;

            foreach (var connection in NetworkServer.connections.Values)
            {
                if (connection.identity && connection.identity.netId == exceptNetId) continue;
                TargetInvokeSocketed(connection);
            }
        }
        
        [Command(requiresAuthority = false)]
        private void CmdInvokeUnSocketed(uint exceptNetId)
        {
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<Rigidbody>().isKinematic = false;
            inventory.SetActive(true);
            
            foreach (var connection in NetworkServer.connections.Values)
            {
                if (connection.identity && connection.identity.netId == exceptNetId) continue;
                TargetInvokeUnSocketed(connection);
            }
        }
        
        [TargetRpc]
        private void TargetInvokeSocketed(NetworkConnection conn)
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            inventory.SetActive(false);
            transform.position = default;
        }
        
        [TargetRpc]
        private void TargetInvokeUnSocketed(NetworkConnection conn)
        {
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<Rigidbody>().isKinematic = false;
            inventory.SetActive(true);
        }
    }
}