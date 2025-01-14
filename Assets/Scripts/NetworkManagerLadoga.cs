using System.Collections.Generic;
using System.Linq;
using HurricaneVR.Framework.Core;
using Mirror;
using UnityEngine;

public class NetworkManagerLadoga : NetworkManager
{
    public GameObject playerOnline;
    public GameObject playerGhost;
    public GameObject backpack;

    private DebugAuthority _debugAuthority;

    public override void Awake()
    {
        _debugAuthority = FindObjectOfType<DebugAuthority>();
    }

    public override void Start()
    {
        NetworkClient.RegisterPrefab(backpack);
        NetworkClient.UnregisterPrefab(playerPrefab);
        NetworkClient.RegisterPrefab(playerPrefab, SpawnPlayer, obj => Destroy(obj));
        base.Start();
    }

    private GameObject SpawnPlayer(SpawnMessage msg)
    {
        return Instantiate(msg.isLocalPlayer ? playerOnline : playerGhost, msg.position, msg.rotation);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        NetworkClient.UnregisterSpawnHandler(1);
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
        var backpack = Instantiate(this.backpack);
        NetworkServer.Spawn(backpack);

        var player = Instantiate(NetworkServer.activeHost ? playerOnline : playerGhost,
            startPositions[numPlayers].position, startPositions[numPlayers].rotation);
        player.GetComponent<PlayerNetwork>().SetBackpackNetId = backpack.GetComponent<NetworkIdentity>().netId;
        NetworkServer.AddPlayerForConnection(conn, player, playerGhost.GetComponent<NetworkIdentity>().assetId);

        if (_debugAuthority)
        {
            _debugAuthority.AddPlayerDebug(NetworkServer.spawned.Last().Value, numPlayers - 1);
        }
        
        foreach (var item in FindObjectsByType<ItemNetwork>(FindObjectsSortMode.None))
        {
            item.GetComponent<ItemNetwork>().networkSendTransform?.Send(true);
            item.GetComponent<ItemNetwork>().networkSendRigidbody?.Send(true);
        }
    }
}