using System;
using System.Collections;
using System.Collections.Generic;
using DungeonGenerator;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject player;
    public BSPDungeonGenerator dungeonGenerator;
    private void Start()
    {
        Debug.Log("Привязываем экшн");
        dungeonGenerator.onGenerationEnd += MovePlayer;
    }

    void MovePlayer()
    {
        Debug.Log("Генерация закончена");
        player.transform.position = dungeonGenerator.rooms[0].rect.center;
    }

    
}
