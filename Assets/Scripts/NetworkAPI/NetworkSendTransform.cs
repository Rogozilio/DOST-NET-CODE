using Mirror;
using NetworkAPI.Enums;
using UnityEngine;

namespace NetworkAPI
{
    public class NetworkSendTransform : NetworkBehaviour
    {
        public bool isRecipient;
        public GameObject target;
        public bool syncTarget;
        public bool syncPosition;
        public bool syncRotation;
        public bool syncScale;

        public bool sendEveryoneExceptMe;
        public SendType sendType;
        public CycleSend cycleSend;

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
            if(isRecipient) return;
            
            switch (sendType)
            {
                case SendType.FromClientToServer when !isServer:
                    if (syncTarget)
                        SendFromClientToServerWithTarget(target, target.transform.position, target.transform.rotation,
                            target.transform.localScale, netId);
                    else
                        SendFromClientToServer(target.transform.position, target.transform.rotation,
                            target.transform.localScale, netId);
                    break;
                case SendType.FromServerToClients when isServer:
                    SendFromServerToClients(netId);
                    break;
                case SendType.FromClientToServerToClients when !isServer:
                    if (syncTarget)
                        SendFromClientToServerWithTarget(target, target.transform.position, target.transform.rotation,
                            target.transform.localScale, netId);
                    else
                        SendFromClientToServer(target.transform.position, target.transform.rotation,
                            target.transform.localScale, netId);
                    break;
            }
        }

        [Command]
        private void SendFromClientToServer(Vector3 position, Quaternion rotation, Vector3 scale, uint networkId)
        {
            SendTransformFromClientToServer(position, rotation, scale, networkId);
        }

        [Command]
        private void SendFromClientToServerWithTarget(GameObject target, Vector3 position, Quaternion rotation,
            Vector3 scale, uint networkId)
        {
            this.target = target;
            SendTransformFromClientToServer(position, rotation, scale, networkId);
        }

        [Server]
        private void SendFromServerToClientsWithTarget(GameObject target, uint clientId)
        {
            SendTargetFromServerToClients(target);
            SendFromServerToClients(clientId);
        }
        
        [Server]
        private void SendFromServerToClients(uint clientId)
        {
            if (sendEveryoneExceptMe)
            {
                SendTransformFromServerToClient(clientId, target.transform.position,
                    target.transform.rotation, target.transform.localScale);
            }
            else
            {
                SendTransformFromServerToClients(target.transform.position, target.transform.rotation,
                    target.transform.localScale);
            }
        }
        
        [Server]
        private void SendTransformFromClientToServer(Vector3 position, Quaternion rotation,
            Vector3 scale, uint networkId)
        {
            if (syncPosition)
                SendPositionFromClientToServer(position);
            if (syncRotation && rotation != default)
                SendRotationFromClientToServer(rotation);
            if (syncScale && scale != default)
                SendScaleFromClientToServer(scale);
            
            if(syncTarget)
                SendFromServerToClientsWithTarget(target, networkId);
            else
                SendFromServerToClients(networkId);
        }

        [Server]
        private void SendTransformFromServerToClients(Vector3 position, Quaternion rotation,
            Vector3 scale)
        {
            if (syncPosition)
                SendPositionFromServerToClients(position);
            if (syncRotation && rotation != default)
                SendRotationFromServerToClients(rotation);
            if (syncScale && scale != default)
                SendScaleFromServerToClients(scale);
        }

        [Server]
        private void SendTransformFromServerToClient(uint clientId, Vector3 position,
            Quaternion rotation, Vector3 scale)
        {
            foreach (var connection in NetworkServer.connections.Values)
            {
                if (connection.identity && connection.identity.netId == clientId) continue;
                if (syncPosition)
                    SendPositionFromServerToClient(connection, position);
                if (syncRotation && rotation != default)
                    SendRotationFromServerToClient(connection, rotation);
                if (syncScale && scale != default)
                    SendScaleFromServerToClient(connection, scale);
            }
        }

        [Server]
        private void SendPositionFromClientToServer(Vector3 position)
        {
            if (!target)
            {
                Debug.LogError("target null");
                return;
            }

            target.transform.position = position;
        }

        [Server]
        private void SendRotationFromClientToServer(Quaternion rotation)
        {
            if (!target)
            {
                Debug.LogError("target null");
                return;
            }

            target.transform.rotation = rotation;
        }

        [Server]
        private void SendScaleFromClientToServer(Vector3 scale)
        {
            if (!target)
            {
                Debug.LogError("target null");
                return;
            }

            target.transform.localScale = scale;
        }

        [ClientRpc]
        private void SendPositionFromServerToClients(Vector3 position)
        {
            if (!target)
            {
                Debug.LogError("target null");
                return;
            }

            target.transform.position = position;
        }

        [ClientRpc]
        private void SendRotationFromServerToClients(Quaternion rotation)
        {
            if (!target)
            {
                Debug.LogError("target null");
                return;
            }

            target.transform.rotation = rotation;
        }

        [ClientRpc]
        private void SendScaleFromServerToClients(Vector3 scale)
        {
            if (!target)
            {
                Debug.LogError("target null");
                return;
            }

            target.transform.localScale = scale;
        }

        [ClientRpc]
        private void SendTargetFromServerToClients(GameObject target)
        {
            this.target = target;
        }

        [TargetRpc]
        private void SendPositionFromServerToClient(NetworkConnection conn, Vector3 position)
        {
            if (!target)
            {
                Debug.LogError("target null");
                return;
            }

            target.transform.position = position;
        }

        [TargetRpc]
        private void SendRotationFromServerToClient(NetworkConnection conn, Quaternion rotation)
        {
            if (!target)
            {
                Debug.LogError("target null");
                return;
            }

            target.transform.rotation = rotation;
        }

        [TargetRpc]
        private void SendScaleFromServerToClient(NetworkConnection conn, Vector3 scale)
        {
            if (!target)
            {
                Debug.LogError("target null");
                return;
            }

            target.transform.localScale = scale;
        }
    }
}