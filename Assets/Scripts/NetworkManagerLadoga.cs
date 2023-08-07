using System.Linq;
using Mirror;
using UnityEngine;

public class NetworkManagerLadoga : NetworkManager
{
    public GameObject playerOnline;
    public GameObject playerGhost;

    private DebugAuthority _debugAuthority;

    public override void Awake()
    {
        _debugAuthority = FindObjectOfType<DebugAuthority>();
    }

    public override void Start()
    {
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
        GameObject player = Instantiate(NetworkServer.activeHost ? playerOnline : playerGhost,
            startPositions[numPlayers].position, startPositions[numPlayers].rotation);

        NetworkServer.AddPlayerForConnection(conn, player, playerGhost.GetComponent<NetworkIdentity>().assetId);

        if (_debugAuthority)
        {
            _debugAuthority.AddPlayerDebug(NetworkServer.spawned.Last().Value, numPlayers - 1);
        }
    }
}