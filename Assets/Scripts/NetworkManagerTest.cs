using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkManagerTest : NetworkManager
{
    public override void OnServerSceneChanged(string sceneName)
    {
        foreach (var prefab in spawnPrefabs)
        {
            NetworkServer.Spawn(Instantiate(prefab));
        }
    }
}