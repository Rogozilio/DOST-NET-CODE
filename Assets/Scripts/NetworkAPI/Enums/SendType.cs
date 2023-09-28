using UnityEngine;

namespace NetworkAPI.Enums
{
    public enum SendType
    {
        [InspectorName("Client -> Server")] FromClientToServer,
        [InspectorName("Server -> Clients")] FromServerToClients,
        [InspectorName("Client -> Server -> Clients")] FromClientToServerToClients
    }
}