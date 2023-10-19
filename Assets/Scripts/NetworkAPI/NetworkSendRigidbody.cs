using DefaultNamespace;
using Mirror;
using NetworkAPI.Enums;
using UnityEngine;

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

        private bool _velocityChanged;
        private bool _angularVelocityChanged;
        private bool _massChanged;
        private bool _dragChanged;
        private bool _angularDragChanged;
        private bool _useGravityChanged;
        private bool _isKinematicChanged;

        private Rigidbody _targetRigidbody;
        private ChangeValue _changeValue;
        private ClientSyncState _previousValue;
        
        public GameObject SetTarget
        {
            set
            {
                if(target == value) return;
                target = value;
                _targetRigidbody = target.GetComponent<Rigidbody>();
                _previousValue.Refresh(_targetRigidbody);
            }
        }

        private void Awake()
        {
            _changeValue = new ChangeValue();
            _previousValue = new ClientSyncState();
            if (!target) return;
            _targetRigidbody = target.GetComponent<Rigidbody>();
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

        public void Send(bool isSyncAll = false)
        {
            if (isRecipient) return;
            
            _velocityChanged 
                = isSyncAll || syncVelocity && _changeValue.Check(_previousValue.velocity, _targetRigidbody.velocity);
            _angularVelocityChanged 
                = isSyncAll || syncAngularVelocity && _changeValue.Check(_previousValue.angularVelocity, _targetRigidbody.angularVelocity);
            _massChanged 
                = isSyncAll || syncMass && _changeValue.Check(_previousValue.mass, _targetRigidbody.mass);
            _dragChanged 
                = isSyncAll || syncDrag && _changeValue.Check(_previousValue.drag, _targetRigidbody.drag);
            _angularDragChanged 
                = isSyncAll || syncAngularDrag && _changeValue.Check(_previousValue.angularDrag, _targetRigidbody.angularDrag);
            _useGravityChanged 
                = isSyncAll || syncUseGravity && _changeValue.Check(_previousValue.useGravity, _targetRigidbody.useGravity);
            _isKinematicChanged 
                = isSyncAll || syncIsKinematic && _changeValue.Check(_previousValue.isKinematic, _targetRigidbody.isKinematic);

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
                if (_velocityChanged) CmdSendVelocity(_targetRigidbody.velocity);
                if (_angularVelocityChanged) CmdSendAngularVelocity(_targetRigidbody.angularVelocity);
                if (_massChanged) CmdSendMass(_targetRigidbody.mass);
                if (_dragChanged) CmdSendDrag(_targetRigidbody.drag);
                if (_angularDragChanged) CmdSendAngularDrag(_targetRigidbody.angularDrag);
                if (_useGravityChanged) CmdSendUseGravity(_targetRigidbody.useGravity);
                if (_isKinematicChanged) CmdSendIsKinematic(_targetRigidbody.isKinematic); 
            }
            else
            {
                if (syncTarget) CmdSendTargetWithoutAuthority(target);
                if (_velocityChanged) CmdSendVelocityWithoutAuthority(_targetRigidbody.velocity);
                if (_angularVelocityChanged) CmdSendAngularVelocityWithoutAuthority(_targetRigidbody.angularVelocity);
                if (_massChanged) CmdSendMassWithoutAuthority(_targetRigidbody.mass);
                if (_dragChanged) CmdSendDragWithoutAuthority(_targetRigidbody.drag);
                if (_angularDragChanged) CmdSendAngularDragWithoutAuthority(_targetRigidbody.angularDrag);
                if (_useGravityChanged) CmdSendUseGravityWithoutAuthority(_targetRigidbody.useGravity);
                if (_isKinematicChanged) CmdSendIsKinematicWithoutAuthority(_targetRigidbody.isKinematic);
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
                if (_velocityChanged) TargetSendVelocity(connection, _targetRigidbody.velocity);
                if (_angularVelocityChanged) TargetSendAngularVelocity(connection, _targetRigidbody.angularVelocity);
                if (_massChanged) TargetSendMass(connection, _targetRigidbody.mass);
                if (_dragChanged) TargetSendDrag(connection, _targetRigidbody.drag);
                if (_angularDragChanged) TargetSendAngularDrag(connection, _targetRigidbody.angularDrag);
                if (_useGravityChanged) TargetSendUseGravity(connection, _targetRigidbody.useGravity);
                if (_isKinematicChanged) TargetSendIsKinematic(connection, _targetRigidbody.isKinematic);
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