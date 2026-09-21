using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    private Transform SetLocalView()
    {
      
        int miIdDeRed = Runner.LocalPlayer.PlayerId;

        Debug.Log($"Mi ID de red real es: {miIdDeRed}");

      
        foreach (var player in Runner.ActivePlayers)
        {
            

            if (player == Runner.LocalPlayer)
            {

              return _spawnPoint[0];
                
            }
            else
            {

                return _spawnPoint[1];

            }
        }

        return null;
    }
}


