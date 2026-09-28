using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSpawn : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] NetworkPrefabRef _playerPrefab;

    [SerializeField] List<Transform> _spawnPoint;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            var spawnPoint = SetLocalView();

            Runner.Spawn(_playerPrefab, spawnPoint.position, spawnPoint.rotation);
            


        }
        if (Runner.ActivePlayers.Count() == 2 && Runner.IsSharedModeMasterClient)
        {
            DeckHandler.instance.InitiateDeck(Runner);
        }
    }

    private Transform SetLocalView()
    {
        GameManager.instance.localPoint = _spawnPoint[0];
        GameManager.instance.enemyPoint = _spawnPoint[1];

        return _spawnPoint[0];
    }
}


