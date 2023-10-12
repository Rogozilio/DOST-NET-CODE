using DefaultNamespace;
using Mirror;
using NetworkAPI.Enums;
using UnityEngine;

namespace NetworkAPI
{
    public class NetworkSendTransform : NetworkBehaviour
    {
        /// <summary>
        /// holds previously synced values
        /// </summary>
        public class ClientSyncState
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public void Refresh(Transform target)
            {
                position = target.position;
                rotation = target.rotation;
                scale = target.localScale;
            }
        }

        public RecipientType recipient;
        public GameObject target;
        public bool syncTarget;
        public bool syncPosition;
        public bool syncRotation;
        public bool syncScale;

        public SendType sendType;
        public CycleSend cycleSend;
        public bool isRequiresAuthority = true;
        
        private bool _positionChanged;
        private bool _rotationChanged;
        private bool _scaleChanged;

        private Transform _targetTransform;
        private ChangeValue _changeValue;
        private ClientSyncState _previousValue;

        public GameObject SetTarget
        {
            set
            {
                if(target == value) return;
                target = value;
                _targetTransform = target.transform;
                _previousValue.Refresh(target.transform);
            }
        }

        private void Awake()
        {
            _changeValue = new ChangeValue();
            _previousValue = new ClientSyncState();
            if (!target) return;
            _targetTransform = target.transform;
            _previousValue.Refresh(target.transform);
        }

        private void Update()
        {
            if (cycleSend != CycleSend.Update) return;

            Send();
        }

        private void FixedUpdate()
        {
            if (cycleSend != CycleSend.FixedUpdate) return;

            Send();
        }

        public void Send()
        {
            if (recipient == RecipientType.ClientAndServer) return;

            _positionChanged
                = syncPosition && _changeValue.Check(_previousValue.position, _targetTransform.position, 0.001f);
            _rotationChanged
                = syncRotation && _changeValue.Check(_previousValue.rotation, _targetTransform.rotation);
            _scaleChanged
                = syncScale && _changeValue.Check(_previousValue.scale, _targetTransform.localScale);

            switch (sendType)
            {
                case SendType.FromClientToServer:
                    if (isServer || recipient == RecipientType.Client) return;
                    FromClientToServer(target);
                    break;
                case SendType.FromServerToClients:
                    if (!isServer) return;
                    FromServerToClients(netId);
                    break;
                case SendType.FromClientToServerToClients:
                    if (!isServer)
                    {
                        if (recipient == RecipientType.None) FromClientToServer(target);
                    }
                    else
                        FromServerToClients(netId);
                    break;
            }

            _previousValue.Refresh(_targetTransform);
        }

        [Client]
        private void FromClientToServer(GameObject target)
        {
            if (isRequiresAuthority)
            {
                if (syncTarget) CmdSendTarget(target);
                if (_positionChanged) CmdSendPosition(_targetTransform.position);
                if (_rotationChanged) CmdSendRotation(_targetTransform.rotation);
                if (_scaleChanged) CmdSendScale(_targetTransform.localScale);
            }
            else
            {
                if (syncTarget) CmdSendTargetWithoutAuthority(target);
                if (_positionChanged) CmdSendPositionWithoutAuthority(_targetTransform.position);
                if (_rotationChanged) CmdSendRotationWithoutAuthority(_targetTransform.rotation);
                if (_scaleChanged) CmdSendScaleWithoutAuthority(_targetTransform.localScale);
            }
        }

        [Server]
        private void FromServerToClients(uint netId)
        {
            if (!target)
            {
                Debug.LogError("target null");
                return;
            }
            
            if (sendType == SendType.FromClientToServerToClients)
            {
                SendToClients(netId);
            }
            else
            {
                SendToClients();
            }
        }

        private void SendToClients(uint exceptNetId = 0)
        {
            foreach (var connection in NetworkServer.connections.Values)
            {
                if (connection.identity && connection.identity.netId == exceptNetId) continue;
                if (syncTarget) TargetSendTarget(connection, target);
                if (_positionChanged) TargetSendPosition(connection, _targetTransform.position);
                if (_rotationChanged) TargetSendRotation(connection, _targetTransform.rotation);
                if (_scaleChanged) TargetSendScale(connection, _targetTransform.localScale);
            }
        }

        [Command]
        private void CmdSendTarget(GameObject target)
        {
            this.SetTarget = target;
        }

        [Command]
        private void CmdSendPosition(Vector3 position)
        {
            _targetTransform.position = position;
        }

        [Command]
        private void CmdSendRotation(Quaternion rotation)
        {
            _targetTransform.rotation = rotation;
        }

        [Command]
        private void CmdSendScale(Vector3 scale)
        {
            _targetTransform.localScale = scale;
        }

        [Command(requiresAuthority = false)]
        private void CmdSendTargetWithoutAuthority(GameObject target)
        {
            this.target = target;
        }

        [Command(requiresAuthority = false)]
        private void CmdSendPositionWithoutAuthority(Vector3 position)
        {
            _targetTransform.position = position;
        }

        [Command(requiresAuthority = false)]
        private void CmdSendRotationWithoutAuthority(Quaternion rotation)
        {
            _targetTransform.rotation = rotation;
        }

        [Command(requiresAuthority = false)]
        private void CmdSendScaleWithoutAuthority(Vector3 scale)
        {
            _targetTransform.localScale = scale;
        }

        //TargetRpc

        [TargetRpc]
        private void TargetSendTarget(NetworkConnection conn, GameObject target)
        {
            SetTarget = target;
        }
        
        [TargetRpc]
        private void TargetSendPosition(NetworkConnection conn, Vector3 position)
        {
            _targetTransform.position = position;
        }

        [TargetRpc]
        private void TargetSendRotation(NetworkConnection conn, Quaternion rotation)
        {
            _targetTransform.rotation = rotation;
        }

        [TargetRpc]
        private void TargetSendScale(NetworkConnection conn, Vector3 scale)
        {
            _targetTransform.localScale = scale;
        }

        // public void OldSend()
        // {
        //     if (isRecipient) return;
        //
        //     switch (sendType)
        //     {
        //         case SendType.FromClientToServer when !isServer:
        //             if (syncTarget)
        //                 SendFromClientToServerWithTarget(target, target.transform.position, target.transform.rotation,
        //                     target.transform.localScale, netId);
        //             else
        //                 SendFromClientToServer(target.transform.position, target.transform.rotation,
        //                     target.transform.localScale, netId);
        //             break;
        //         case SendType.FromServerToClients when isServer:
        //             SendFromServerToClients(netId);
        //             break;
        //         case SendType.FromClientToServerToClients when !isServer:
        //             if (syncTarget)
        //                 SendFromClientToServerWithTarget(target, target.transform.position, target.transform.rotation,
        //                     target.transform.localScale, netId);
        //             else
        //                 SendFromClientToServer(target.transform.position, target.transform.rotation,
        //                     target.transform.localScale, netId);
        //             break;
        //     }
        // }
        //
        // [Command]
        // private void SendFromClientToServer(Vector3 position, Quaternion rotation, Vector3 scale, uint networkId)
        // {
        //     SendTransformFromClientToServer(position, rotation, scale, networkId);
        // }
        //
        // [Command]
        // private void SendFromClientToServerWithTarget(GameObject target, Vector3 position, Quaternion rotation,
        //     Vector3 scale, uint networkId)
        // {
        //     this.target = target;
        //     SendTransformFromClientToServer(position, rotation, scale, networkId);
        // }
        //
        // [Server]
        // private void SendFromServerToClientsWithTarget(GameObject target, uint clientId)
        // {
        //     SendTargetFromServerToClients(target);
        //     SendFromServerToClients(clientId);
        // }
        //
        // [Server]
        // private void SendFromServerToClients(uint clientId)
        // {
        //     // if (sendEveryoneExceptMe)
        //     // {
        //     //     SendTransformFromServerToClient(clientId, target.transform.position,
        //     //         target.transform.rotation, target.transform.localScale);
        //     // }
        //     // else
        //     // {
        //     //     SendTransformFromServerToClients(target.transform.position, target.transform.rotation,
        //     //         target.transform.localScale);
        //     // }
        // }
        //
        // [Server]
        // private void SendTransformFromClientToServer(Vector3 position, Quaternion rotation,
        //     Vector3 scale, uint networkId)
        // {
        //     if (syncPosition)
        //         SendPositionFromClientToServer(position);
        //     if (syncRotation && rotation != default)
        //         SendRotationFromClientToServer(rotation);
        //     if (syncScale && scale != default)
        //         SendScaleFromClientToServer(scale);
        //
        //     if (syncTarget)
        //         SendFromServerToClientsWithTarget(target, networkId);
        //     else
        //         SendFromServerToClients(networkId);
        // }
        //
        // [Server]
        // private void SendTransformFromServerToClients(Vector3 position, Quaternion rotation,
        //     Vector3 scale)
        // {
        //     if (syncPosition)
        //         SendPositionFromServerToClients(position);
        //     if (syncRotation && rotation != default)
        //         SendRotationFromServerToClients(rotation);
        //     if (syncScale && scale != default)
        //         SendScaleFromServerToClients(scale);
        // }
        //
        // [Server]
        // private void SendTransformFromServerToClient(uint clientId, Vector3 position,
        //     Quaternion rotation, Vector3 scale)
        // {
        //     foreach (var connection in NetworkServer.connections.Values)
        //     {
        //         if (connection.identity && connection.identity.netId == clientId) continue;
        //         if (syncPosition)
        //             SendPositionFromServerToClient(connection, position);
        //         if (syncRotation && rotation != default)
        //             SendRotationFromServerToClient(connection, rotation);
        //         if (syncScale && scale != default)
        //             SendScaleFromServerToClient(connection, scale);
        //     }
        // }
        //
        // [Server]
        // private void SendPositionFromClientToServer(Vector3 position)
        // {
        //     if (!target)
        //     {
        //         Debug.LogError("target null");
        //         return;
        //     }
        //
        //     target.transform.position = position;
        // }
        //
        // [Server]
        // private void SendRotationFromClientToServer(Quaternion rotation)
        // {
        //     if (!target)
        //     {
        //         Debug.LogError("target null");
        //         return;
        //     }
        //
        //     target.transform.rotation = rotation;
        // }
        //
        // [Server]
        // private void SendScaleFromClientToServer(Vector3 scale)
        // {
        //     if (!target)
        //     {
        //         Debug.LogError("target null");
        //         return;
        //     }
        //
        //     target.transform.localScale = scale;
        // }
        //
        // [ClientRpc]
        // private void SendPositionFromServerToClients(Vector3 position)
        // {
        //     if (!target)
        //     {
        //         Debug.LogError("target null");
        //         return;
        //     }
        //
        //     target.transform.position = position;
        // }
        //
        // [ClientRpc]
        // private void SendRotationFromServerToClients(Quaternion rotation)
        // {
        //     if (!target)
        //     {
        //         Debug.LogError("target null");
        //         return;
        //     }
        //
        //     target.transform.rotation = rotation;
        // }
        //
        // [ClientRpc]
        // private void SendScaleFromServerToClients(Vector3 scale)
        // {
        //     if (!target)
        //     {
        //         Debug.LogError("target null");
        //         return;
        //     }
        //
        //     target.transform.localScale = scale;
        // }
        //
        // [ClientRpc]
        // private void SendTargetFromServerToClients(GameObject target)
        // {
        //     this.target = target;
        // }
        //
        // [TargetRpc]
        // private void SendPositionFromServerToClient(NetworkConnection conn, Vector3 position)
        // {
        //     if (!target)
        //     {
        //         Debug.LogError("target null");
        //         return;
        //     }
        //
        //     target.transform.position = position;
        // }
        //
        // [TargetRpc]
        // private void SendRotationFromServerToClient(NetworkConnection conn, Quaternion rotation)
        // {
        //     if (!target)
        //     {
        //         Debug.LogError("target null");
        //         return;
        //     }
        //
        //     target.transform.rotation = rotation;
        // }
        //
        // [TargetRpc]
        // private void SendScaleFromServerToClient(NetworkConnection conn, Vector3 scale)
        // {
        //     if (!target)
        //     {
        //         Debug.LogError("target null");
        //         return;
        //     }
        //
        //     target.transform.localScale = scale;
        // }
    }
}