using Mirror;
using UnityEngine;

namespace NetworkAPI
{
    public class NetworkSendTransform : NetworkBehaviour
    {
        public enum SendType
        {
            [InspectorName("Client -> Server")] 
            FromClientToServer,
            [InspectorName("Server -> Clients")] 
            FromServerToClients,
            [InspectorName("Client -> Server -> Clients")]
            FromClientToServerToClients
        }

        public bool syncPosition;
        public bool syncRotation;
        public bool syncScale;

        public bool sendEveryoneExceptMe;
        public SendType sendType;

        public void Send(uint clientId, GameObject target, Vector3 position = default, Quaternion rotation = default,
            Vector3 scale = default)
        {
            switch (sendType)
            {
                case SendType.FromClientToServer:
                    SendFromClientToServer(target, position, rotation, scale);
                    break;
                case SendType.FromServerToClients:
                    SendFromServerToClients(clientId, target);
                    break;
                case SendType.FromClientToServerToClients:
                    SendFromClientToServer(target, position, rotation, scale);
                    SendFromServerToClients(clientId, target);
                    break;
            }
        }

        [Command]
        private void SendFromClientToServer(GameObject target, Vector3 position, Quaternion rotation,
            Vector3 scale)
        {
            SendTransformFromClientToServer(target, position, rotation, scale);
        }
        
        [Server]
        private void SendFromServerToClients(uint clientId, GameObject target)
        {
            if(!isServer) return;
            
            if (sendEveryoneExceptMe)
            {
                SendTransformFromServerToClient(clientId, target, target.transform.position,
                    target.transform.rotation, target.transform.localScale);
            }
            else
            {
                SendTransformFromServerToClients(target, target.transform.position, target.transform.rotation,
                    target.transform.localScale);
            }
        }

        [Server]
        private void SendTransformFromClientToServer(GameObject target, Vector3 position, Quaternion rotation,
            Vector3 scale)
        {
            if (syncPosition)
                SendPositionFromClientToServer(position, target);
            if (syncRotation && rotation != default)
                SendRotationFromClientToServer(rotation, target);
            if (syncScale && scale != default)
                SendScaleFromClientToServer(scale, target);
        }
        [Server]
        private void SendTransformFromServerToClients(GameObject target, Vector3 position, Quaternion rotation,
            Vector3 scale)
        {
            if (syncPosition)
                SendPositionFromServerToClients(position, target);
            if (syncRotation && rotation != default)
                SendRotationFromServerToClients(rotation, target);
            if (syncScale && scale != default)
                SendScaleFromServerToClients(scale, target);
        }

        [Server]
        private void SendTransformFromServerToClient(uint clientId, GameObject target, Vector3 position,
            Quaternion rotation, Vector3 scale)
        {
            foreach (var connection in NetworkServer.connections.Values)
            {
                if (connection.identity.netId == clientId) continue;
                if (syncPosition)
                    SendPositionFromServerToClient(connection, position, target);
                if (syncRotation && rotation != default)
                    SendRotationFromServerToClient(connection, rotation, target);
                if (syncScale && scale != default)
                    SendScaleFromServerToClient(connection, scale, target);
            }
        }

        [Server]
        private void SendPositionFromClientToServer(Vector3 position, GameObject target)
        {
            if (target)
                target.transform.position = position;
            else
                transform.position = position;
        }

        [Server]
        private void SendRotationFromClientToServer(Quaternion rotation, GameObject target)
        {
            if (target)
                target.transform.rotation = rotation;
            else
                transform.rotation = rotation;
        }

        [Server]
        private void SendScaleFromClientToServer(Vector3 scale, GameObject target)
        {
            if (target)
                target.transform.localScale = scale;
            else
                transform.localScale = scale;
        }

        [ClientRpc]
        private void SendPositionFromServerToClients(Vector3 position, GameObject target)
        {
            if (target)
                target.transform.position = position;
            else
                transform.position = position;
        }

        [ClientRpc]
        private void SendRotationFromServerToClients(Quaternion rotation, GameObject target)
        {
            if (target)
                target.transform.rotation = rotation;
            else
                transform.rotation = rotation;
        }

        [ClientRpc]
        private void SendScaleFromServerToClients(Vector3 scale, GameObject target)
        {
            if (target)
                target.transform.localScale = scale;
            else
                transform.localScale = scale;
        }

        [TargetRpc]
        private void SendPositionFromServerToClient(NetworkConnection conn, Vector3 position, GameObject target)
        {
            if (target)
                target.transform.position = position;
            else
                transform.position = position;
        }

        [TargetRpc]
        private void SendRotationFromServerToClient(NetworkConnection conn, Quaternion rotation, GameObject target)
        {
            if (target)
                target.transform.rotation = rotation;
            else
                transform.rotation = rotation;
        }

        [TargetRpc]
        private void SendScaleFromServerToClient(NetworkConnection conn, Vector3 scale, GameObject target)
        {
            if (target)
                target.transform.localScale = scale;
            else
                transform.localScale = scale;
        }
    }
}