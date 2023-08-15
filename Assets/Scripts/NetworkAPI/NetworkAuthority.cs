using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using Mirror;
using UnityEngine;

namespace NetworkAPI
{
    public class NetworkAuthority : NetworkBehaviour
    {
        private NetworkIdentity _networkIdentity;

        public NetworkIdentity networkIdentity
        {
            set => _networkIdentity = value;
            get => _networkIdentity;
        }

        public uint NetId => _networkIdentity.netId;
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
            if (grabbable.IsLeftHandGrabbed || grabbable.IsRightHandGrabbed) return;

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

        [Command]
        void CmdSetAuthority(NetworkIdentity grabID)
        {
            grabID.AssignClientAuthority(_networkIdentity.connectionToClient);
        }
        [Server]
        void ServerSetAuthority(NetworkIdentity grabID)
        {
            grabID.AssignClientAuthority(_networkIdentity.connectionToClient);
        }

        [Command]
        void CmdRemoveAuthority(NetworkIdentity grabID)
        {
            grabID.RemoveClientAuthority();
        }
        [Server]
        void ServerRemoveAuthority(NetworkIdentity grabID)
        {
            grabID.RemoveClientAuthority();
        }
    }
}