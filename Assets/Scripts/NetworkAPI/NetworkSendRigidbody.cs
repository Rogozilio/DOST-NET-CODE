using Mirror;
using NetworkAPI.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace NetworkAPI
{
    public class NetworkSendRigidbody : NetworkBehaviour
    {
        /// <summary>
        /// holds previously synced values
        /// </summary>
        public class ClientSyncState
        {
            public Vector3 velocity;
            public Vector3 angularVelocity;
            public bool isKinematic;
            public bool useGravity;
            public float drag;
            public float angularDrag;
            public float mass;

            public void Refresh(Rigidbody target)
            {
                velocity = target.velocity;
                angularVelocity = target.angularVelocity;
                isKinematic = target.isKinematic;
                useGravity = target.useGravity;
                drag = target.drag;
                angularDrag = target.angularDrag;
                mass = target.mass;
            }
        }

        public bool isRecipient;
        public GameObject target;

        public bool syncTarget;
        public bool syncVelocity;
        public bool syncAngularVelocity;
        public bool syncMass;
        public bool syncDrag;
        public bool syncAngularDrag;
        public bool syncUseGravity;
        public bool syncIsKinematic;
        
        public SendType sendType;
        public CycleSend cycleSend;
        public bool isRequiresAuthority = true;

        private bool velocityChanged;
        private bool angularVelocityChanged;
        private bool massChanged;
        private bool dragChanged;
        private bool angularDragChanged;
        private bool useGravityChanged;
        private bool isKinematicChanged;

        private Rigidbody _targetRigidbody;
        private ClientSyncState _previousValue;

        private void Awake()
        {
            Debug.Log("Test commit");
            _targetRigidbody = target.GetComponent<Rigidbody>();
            _previousValue = new ClientSyncState();
            _previousValue.Refresh(_targetRigidbody);
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

        public bool IsChangeValue(bool prevValue, bool value)
        {
            return prevValue != value;
        }

        public bool IsChangeValue(float prevValue, float value, float delta = 0.01f)
        {
            return Mathf.Abs(value - prevValue) > delta;
        }

        public bool IsChangeValue(Vector3 prevValue, Vector3 value, float delta = 0.01f)
        {
            return (prevValue - value).sqrMagnitude > delta * delta;
        }

        public void Send()
        {
            if (isRecipient) return;
            
            velocityChanged 
                = syncVelocity && IsChangeValue(_previousValue.velocity, _targetRigidbody.velocity);
            angularVelocityChanged 
                = syncAngularVelocity && IsChangeValue(_previousValue.angularVelocity, _targetRigidbody.angularVelocity);
            massChanged 
                = syncMass && IsChangeValue(_previousValue.mass, _targetRigidbody.mass);
            dragChanged 
                = syncDrag && IsChangeValue(_previousValue.drag, _targetRigidbody.drag);
            angularDragChanged 
                = syncAngularDrag && IsChangeValue(_previousValue.angularDrag, _targetRigidbody.angularDrag);
            useGravityChanged 
                = syncUseGravity && IsChangeValue(_previousValue.useGravity, _targetRigidbody.useGravity);
            isKinematicChanged 
                = syncIsKinematic && IsChangeValue(_previousValue.isKinematic, _targetRigidbody.isKinematic);

            switch (sendType)
            {
                case SendType.FromClientToServer:
                    if(isServer) return;
                    FromClientToServer(target);
                    break;
                case SendType.FromServerToClients:
                    if(!isServer) return;
                    FromServerToClients(netId);
                    break;
                case SendType.FromClientToServerToClients:
                    if(!isServer)
                        FromClientToServer(target);
                    else
                        FromServerToClients(netId);
                    break;
            }
            
            _previousValue.Refresh(_targetRigidbody);
        }
        
        [Client]
        private void FromClientToServer(GameObject target)
        {
            if (isRequiresAuthority)
            {
                if (syncTarget) CmdSendTarget(target);
                if (velocityChanged) CmdSendVelocity(_targetRigidbody.velocity);
                if (angularVelocityChanged) CmdSendAngularVelocity(_targetRigidbody.angularVelocity);
                if (massChanged) CmdSendMass(_targetRigidbody.mass);
                if (dragChanged) CmdSendDrag(_targetRigidbody.drag);
                if (angularDragChanged) CmdSendAngularDrag(_targetRigidbody.angularDrag);
                if (useGravityChanged) CmdSendUseGravity(_targetRigidbody.useGravity);
                if (isKinematicChanged) CmdSendIsKinematic(_targetRigidbody.isKinematic); 
            }
            else
            {
                if (syncTarget) CmdSendTargetWithoutAuthority(target);
                if (velocityChanged) CmdSendVelocityWithoutAuthority(_targetRigidbody.velocity);
                if (angularVelocityChanged) CmdSendAngularVelocityWithoutAuthority(_targetRigidbody.angularVelocity);
                if (massChanged) CmdSendMassWithoutAuthority(_targetRigidbody.mass);
                if (dragChanged) CmdSendDragWithoutAuthority(_targetRigidbody.drag);
                if (angularDragChanged) CmdSendAngularDragWithoutAuthority(_targetRigidbody.angularDrag);
                if (useGravityChanged) CmdSendUseGravityWithoutAuthority(_targetRigidbody.useGravity);
                if (isKinematicChanged) CmdSendIsKinematicWithoutAuthority(_targetRigidbody.isKinematic);
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
                if (connection.identity && connection.identity.assetId == exceptNetId) continue;
                if (velocityChanged) TargetSendVelocity(connection, _targetRigidbody.velocity);
                if (angularVelocityChanged) TargetSendAngularVelocity(connection, _targetRigidbody.angularVelocity);
                if (massChanged) TargetSendMass(connection, _targetRigidbody.mass);
                if (dragChanged) TargetSendDrag(connection, _targetRigidbody.drag);
                if (angularDragChanged) TargetSendAngularDrag(connection, _targetRigidbody.angularDrag);
                if (useGravityChanged) TargetSendUseGravity(connection, _targetRigidbody.useGravity);
                if (isKinematicChanged) TargetSendIsKinematic(connection, _targetRigidbody.isKinematic);
            }
        }

        [Command]
        private void CmdSendTarget(GameObject target)
        {
            this.target = target;
        }
        [Command]
        private void CmdSendVelocity(Vector3 velocity)
        {
            _targetRigidbody.velocity = velocity;
        }

        [Command]
        private void CmdSendAngularVelocity(Vector3 angularVelocity)
        {
            _targetRigidbody.angularVelocity = angularVelocity;
        }

        [Command]
        private void CmdSendMass(float mass)
        {
            _targetRigidbody.mass = mass;
        }

        [Command]
        private void CmdSendDrag(float drag)
        {
            _targetRigidbody.drag = drag;
        }

        [Command]
        private void CmdSendAngularDrag(float angularDrag)
        {
            _targetRigidbody.angularDrag = angularDrag;
        }

        [Command]
        private void CmdSendUseGravity(bool useGravity)
        {
            _targetRigidbody.useGravity = useGravity;
        }

        [Command]
        private void CmdSendIsKinematic(bool isKinematic)
        {
            _targetRigidbody.isKinematic = isKinematic;
        }
        
        [Command(requiresAuthority = false)]
        private void CmdSendTargetWithoutAuthority(GameObject target)
        {
            this.target = target;
        }
        [Command(requiresAuthority = false)]
        private void CmdSendVelocityWithoutAuthority(Vector3 velocity)
        {
            _targetRigidbody.velocity = velocity;
        }

        [Command(requiresAuthority = false)]
        private void CmdSendAngularVelocityWithoutAuthority(Vector3 angularVelocity)
        {
            _targetRigidbody.angularVelocity = angularVelocity;
        }

        [Command(requiresAuthority = false)]
        private void CmdSendMassWithoutAuthority(float mass)
        {
            _targetRigidbody.mass = mass;
        }

        [Command(requiresAuthority = false)]
        private void CmdSendDragWithoutAuthority(float drag)
        {
            _targetRigidbody.drag = drag;
        }

        [Command(requiresAuthority = false)]
        private void CmdSendAngularDragWithoutAuthority(float angularDrag)
        {
            _targetRigidbody.angularDrag = angularDrag;
        }

        [Command(requiresAuthority = false)]
        private void CmdSendUseGravityWithoutAuthority(bool useGravity)
        {
            _targetRigidbody.useGravity = useGravity;
        }

        [Command(requiresAuthority = false)]
        private void CmdSendIsKinematicWithoutAuthority(bool isKinematic)
        {
            _targetRigidbody.isKinematic = isKinematic;
        }
        
        //TargetRPC
        
        [TargetRpc]
        private void TargetSendVelocity(NetworkConnection conn, Vector3 velocity)
        {
            _targetRigidbody.velocity = velocity;
        }

        [TargetRpc]
        private void TargetSendAngularVelocity(NetworkConnection conn, Vector3 angularVelocity)
        {
            _targetRigidbody.angularVelocity = angularVelocity;
        }

        [TargetRpc]
        private void TargetSendMass(NetworkConnection conn, float mass)
        {
            _targetRigidbody.mass = mass;
        }

        [TargetRpc]
        private void TargetSendDrag(NetworkConnection conn, float drag)
        {
            _targetRigidbody.drag = drag;
        }

        [TargetRpc]
        private void TargetSendAngularDrag(NetworkConnection conn, float angularDrag)
        {
            _targetRigidbody.angularDrag = angularDrag;
        }

        [TargetRpc]
        private void TargetSendUseGravity(NetworkConnection conn, bool useGravity)
        {
            _targetRigidbody.useGravity = useGravity;
        }

        [TargetRpc]
        private void TargetSendIsKinematic(NetworkConnection conn, bool isKinematic)
        {
            _targetRigidbody.isKinematic = isKinematic;
        }
    }
}