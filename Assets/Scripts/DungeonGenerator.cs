using Constant;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{    
    // Размеры подземелья
    public const int MAZE_HEIGHT = 75;
    public const int MAZE_WIDTH = 95;

    // Наличие тупиков
    public bool deadEnds = false;

    /// <summary>
    /// Количество попыток для размещения комнат
    /// </summary>
    [Header("Попытки разместить комнаты")]
    public int attemptsToPlaceRooms = 200;

    /// <summary>
    /// Добавочное значение к максимальному размеру комнаты
    /// </summary>
    [Header("Добавочный размер комнаты")]
    public int roomExtraSize = 2;

    // Работа с тайлами
    [Header("Работа с tilemap")]
    public Tilemap tilemap;
    public TileBase wallBrickTile;
    public TileBase floorWoodTile;

    private void Start()
    {
        Dungeon dungeon = new Dungeon(MAZE_WIDTH, MAZE_HEIGHT);

        dungeon.GenerateRooms();
        dungeon.GenerateMaze();
        dungeon.ConnectRegions();

        if (!deadEnds) dungeon.RemoveDeadEnds();

        DrawDungeon(dungeon);
    }

    public void DrawDungeon(Dungeon dungeon)
    {
        for (int x = 0; x < MAZE_WIDTH; x++)
        {
            for (int y = 0; y < MAZE_HEIGHT; y++)
            {
                if (Container.IsWall(dungeon[x, y]))
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), wallBrickTile);
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), floorWoodTile);
                }
            }
        }
    }

}
