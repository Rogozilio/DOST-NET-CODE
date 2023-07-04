using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DebugAuthority : NetworkBehaviour
{
    [Serializable]
    struct PlayerDebug
    {
        public NetworkIdentity Identity;
        public Color Color;

        public PlayerDebug(NetworkIdentity identity, Color color)
        {
            Identity = identity;
            Color = color;
        }
    }

    [Serializable]
    struct SpawnedDebug
    {
        public NetworkIdentity Identity;
        public MeshRenderer Renderer;
        public Color OriginColor;
        public SpawnedDebug(NetworkIdentity identity, MeshRenderer renderer)
        {
            Identity = identity;
            Renderer = renderer;
            OriginColor = renderer.material.color;
        }

        public void ResetColor()
        {
            Renderer.material.color = OriginColor;
        }
    }

    public Color[] colorAuthorityPlayers;

    private List<PlayerDebug> _playerIdentity;
    private List<SpawnedDebug> _spawnedIdentity;

    public void AddPlayerDebug(NetworkIdentity identity, int numPlayer)
    {
        _playerIdentity.Add(new PlayerDebug(identity, colorAuthorityPlayers[numPlayer]));
    }

    public void AddSpawnedDebug(NetworkIdentity identity, MeshRenderer meshRenderer)
    {
        _spawnedIdentity.Add(new SpawnedDebug(identity, meshRenderer));
    }

    private void Awake()
    {
        _playerIdentity = new List<PlayerDebug>();
        _spawnedIdentity = new List<SpawnedDebug>();
    }

    private void Update()
    {
        for (var i = 0; i < _spawnedIdentity.Count; i++)
        {
            for (var j = 0; j < _playerIdentity.Count; j++)
            {
                if (_spawnedIdentity[i].Identity.connectionToClient !=
                    _playerIdentity[j].Identity.connectionToClient)
                {
                    _spawnedIdentity[i].ResetColor();
                    continue;
                }

                ChangeColorObject(j, i);
                break;
            }
        }
    }

    private void ChangeColorObject(int indexPlayer, int indexSpawned)
    {
        if (isClient)
            CmdChangeColorObject(indexPlayer, indexSpawned);
        else
            ServerChangeColorObject(indexPlayer, indexSpawned);
    }

    [Command(requiresAuthority = false)]
    private void CmdChangeColorObject(int indexPlayer, int indexSpawned)
    {
        ServerChangeColorObject(indexPlayer, indexSpawned);
    }

    [Server]
    private void ServerChangeColorObject(int indexPlayer, int indexSpawned)
    {
        _spawnedIdentity[indexSpawned].Renderer.material.color = _playerIdentity[indexPlayer].Color;
    }
}