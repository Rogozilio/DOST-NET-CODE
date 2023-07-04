using System;
using System.Linq;
using Mirror;
using UnityEngine;

public class NetworkManagerTest : NetworkManager
{
    public GameObject playerEmpty;
    
    private DebugAuthority _debugAuthority;
    public override void Awake()
    {
        _debugAuthority = FindObjectOfType<DebugAuthority>();
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        foreach (var prefab in spawnPrefabs)
        {
            NetworkServer.Spawn(Instantiate(prefab));

            if (_debugAuthority)
            {
                var lastSpawned = NetworkServer.spawned.Last().Value;
                _debugAuthority.AddSpawnedDebug(lastSpawned, lastSpawned.GetComponentInChildren<MeshRenderer>());
            }
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject player = Instantiate(playerEmpty, startPositions[numPlayers].position,
            startPositions[numPlayers].rotation);
        NetworkServer.AddPlayerForConnection(conn, player, playerPrefab.GetComponent<NetworkIdentity>().assetId);

        if (_debugAuthority)
        {
            _debugAuthority.AddPlayerDebug(NetworkServer.spawned.Last().Value, numPlayers - 1);
        }
    }
}