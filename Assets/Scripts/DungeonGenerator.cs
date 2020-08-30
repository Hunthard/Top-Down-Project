using Constant;
using TempNamespace;

using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    private static Dungeon _dungeon;
    
    private int _width;
    private int _height;

    public GameObject shadowCasterGenerator;

    [Header("Игрок")]
    public GameObject player;

    public bool IsSpawnPlayer = false;
    public enum ValidWidth
    {
        [InspectorName("1")]
        One = 1,
        [InspectorName("3")]
        Three = 3
    }

    [Header("Параметры")]
    public ValidWidth PassageWidth;

    public ValidWidth GateWidth;

    // Структура для передачи параметров в конструктор Dungeon
    public Container.Parameter Parameters;
    
    // Количество блоков по горизонтали и вертикали
    public int horizontalNumBlocks = 33;
    public int verticalNumBlocks = 25;

    [Space(5f)]
    // Наличие тупиков
    public bool deadEnds = false;
    
    // Логирование
    public bool logEnable = false;

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

    [Header("Параметр случайно генерации")]
    public int seed = 0;
    
    private int wallExtraHeight = 1;

    [Header("Освещение")]
    public GameObject globalLight;
    
    // Работа с тайлами
    [Header("Работа с tilemap")]
    public Grid grid;
    
    [Space(5f)]
    public Tilemap baseLayerTilemap;
    public Tilemap colliderLayerTilemap;
    public Tilemap interactiveLayerTilemap;
    
    [FormerlySerializedAs("wallBrickTile")] [Space(5f)]
    public TileBase bricksTile;
    [FormerlySerializedAs("floorWoodTile")]
    public TileBase woodTile;

    [Space(5f)]
    public TileBase wallRuleTile;
    public TileBase wallTile;
    
    [Space(5f)]
    public TileBase topDoorTile;
    public TileBase bottomDoorTile;

    private void Awake()
    {
        Parameters.passageWidthInBlocks = (int) PassageWidth;
        Parameters.widthInBlocks = horizontalNumBlocks / 2;
        Parameters.heightInBlocks = verticalNumBlocks / 2;
        Parameters.borderWidthInBlocks = 1;
        Parameters.gateWidthInBlocks = (int) GateWidth;

        _width = Parameters.passageWidthInBlocks * Parameters.widthInBlocks + Parameters.widthInBlocks - 1 +
                 2 * Parameters.borderWidthInBlocks;
        _height = Parameters.passageWidthInBlocks * Parameters.heightInBlocks + Parameters.heightInBlocks - 1 +
                  2 * Parameters.borderWidthInBlocks;

        grid.GetComponent<PolygonCollider2D>().SetPath(0, new []
        {
            new Vector2(grid.transform.position.x, grid.transform.position.y),
            new Vector2(grid.transform.position.x, grid.transform.position.y + _height + 1),
            new Vector2(grid.transform.position.x + _width, grid.transform.position.y + _height + 1),
            new Vector2(grid.transform.position.x + _width, grid.transform.position.y)
        });
    }

    private void Start()
    {
        // кол-во блоков * ширину коридора + кол-во блоков - 1 + 2 блока для границ
        
        _dungeon = new Dungeon(Parameters, seed);
        
        _dungeon.Generate(attemptsToPlaceRooms, roomExtraSize, deadEnds, logEnable);
        
        DrawDungeon(_dungeon);
        //DrawRooms(dungeon);

        if (IsSpawnPlayer)
        {
            SpawnPlayer(_dungeon);
        }
        else
        {
            globalLight.GetComponent<Light2D>().intensity = 1;
        }
    }

    private IEnumerator ShadowCaster()
    {
        colliderLayerTilemap.SetTile(Vector3Int.zero, null);
        yield return null;
        //EditorApplication.ExecuteMenuItem("Tools/Generate Shadow Casters");
        ShadowCaster2DGenerator.GenerateShadowCasters();
        colliderLayerTilemap.SetTile(Vector3Int.zero, wallTile);

        // Костыли чтобы ShadowCaster2D не кидал тени на стены
        int layersCount = SortingLayer.layers.Length - 1;
        int[] layerWithoutWalls = new int[layersCount];

        for (int layerIndex = 0; layerIndex < layersCount; layerIndex++)
        {
            if (SortingLayer.layers[layerIndex].name != "Walls")
            {
                layerWithoutWalls[layerIndex] = SortingLayer.layers[layerIndex].id;
            }
        }

        var shadowCaster2D = colliderLayerTilemap.GetComponentInChildren<ShadowCaster2D>();
        
        // FieldInfo fieldInfo;
        // fieldInfo = shadowCaster2D.GetType().GetField("m_ApplyToSortingLayers", BindingFlags.Instance | BindingFlags.Instance);
        // fieldInfo.SetValue(shadowCaster2D, layerWithoutWalls);

        shadowCaster2D.GetType().GetField("m_ApplyToSortingLayers", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(shadowCaster2D, layerWithoutWalls);
    }

    private void DrawDungeon(Dungeon dungeon)
    {
        for (int x = dungeon.Width - 1; x >=0; x--)
        {
            for (int y = dungeon.Height - 1; y >= 0; y--)
            {
                if (Container.IsWall(dungeon[x, y]))
                {
                    colliderLayerTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                    colliderLayerTilemap.SetTile(new Vector3Int(x, y + wallExtraHeight, 0), wallRuleTile);
                    
                    baseLayerTilemap.SetTile(new Vector3Int(x, y, 0), null);
                    baseLayerTilemap.SetTile(new Vector3Int(x, y + wallExtraHeight, 0), null);
                }
                else
                {
                    baseLayerTilemap.SetTile(new Vector3Int(x, y, 0), bricksTile);
                }
            }
        }
        
        foreach (var door in dungeon.Doors)
        {
            if (Container.IsWall(dungeon[door.x + 1, door.y]) || Container.IsWall(dungeon[door.x - 1, door.y]))
            {
                baseLayerTilemap.SetTile(new Vector3Int(door.x, door.y, 0), bricksTile);
            }
            else
            {
                //colliderLayerTilemap.SetTile(new Vector3Int(door.x, door.y + wallExtraHeight, 0), null);
                colliderLayerTilemap.SetTile(new Vector3Int(door.x, door.y, 0), null);
                
                baseLayerTilemap.SetTile(new Vector3Int(door.x, door.y, 0), bricksTile);
            }
        }
        
        Debug.Log("Dungeon drawn");
        StartCoroutine(ShadowCaster());
    }

    private void DrawRooms(Dungeon dungeon)
    {
        //var tilePosition = new Vector3Int();
        
        foreach (var room in dungeon.Rooms)
        {
            /*for (int x = room.Area.xMin; x <= room.Area.xMax; x++)
            {
                // Текстура стену
                baseLayerTilemap.SetTile(new Vector3Int(x, room.Area.yMin - 1 + wallExtraHeight, 0), null);
                // Верхняя часть стены
                colliderLayerTilemap.SetTile(new Vector3Int(x, room.Area.yMin - 1 + wallExtraHeight, 0), horizontalWallTile);
                // Нижняя часть стены
                colliderLayerTilemap.SetTile(new Vector3Int(x, room.Area.yMin - 1, 0), bottomWallTile);
                
                //  Текстура стену
                baseLayerTilemap.SetTile(new Vector3Int(x, room.Area.yMax + 1 + wallExtraHeight, 0), null);
                // Верхняя часть стены
                colliderLayerTilemap.SetTile(new Vector3Int(x, room.Area.yMax + 1 + wallExtraHeight, 0), horizontalWallTile);
                // Нижняя часть стены
                colliderLayerTilemap.SetTile(new Vector3Int(x, room.Area.yMax + 1, 0), bottomWallTile);
            }

            for (int y = room.Area.yMin + wallExtraHeight; y <= room.Area.yMax + wallExtraHeight; y++)
            {
                // Текстура стены
                baseLayerTilemap.SetTile(new Vector3Int(room.Area.xMin - 1, y, 0), null);
                // Левая стена
                colliderLayerTilemap.SetTile(new Vector3Int(room.Area.xMin - 1, y, 0), verticalWallTile);
                
                // Текстура стены
                baseLayerTilemap.SetTile(new Vector3Int(room.Area.xMax + 1, y, 0), null);
                // Правая стена
                colliderLayerTilemap.SetTile(new Vector3Int(room.Area.xMax + 1, y, 0), verticalWallTile);
            }
            
            // Текстура стены
            baseLayerTilemap.SetTile(new Vector3Int(room.Area.xMin - 1, room.Area.yMin, 0),
                null);
            baseLayerTilemap.SetTile(new Vector3Int(room.Area.xMax + 1, room.Area.yMin, 0),
                null);
            
            // Левый нижний угол
            colliderLayerTilemap.SetTile(new Vector3Int(room.Area.xMin - 1, room.Area.yMin - 1, 0),
                bottomWallTile);
            colliderLayerTilemap.SetTile(new Vector3Int(room.Area.xMin - 1, room.Area.yMin - 1 + wallExtraHeight, 0),
                leftBottomCornerWallTile);
            
            // Правый нижний угол
            colliderLayerTilemap.SetTile(new Vector3Int(room.Area.xMax + 1, room.Area.yMin - 1, 0),
                bottomWallTile);
            colliderLayerTilemap.SetTile(new Vector3Int(room.Area.xMax + 1, room.Area.yMin - 1 + wallExtraHeight, 0),
                rightBottomCornerWallTile);
            
            // Текстура стены
            baseLayerTilemap.SetTile(new Vector3Int(room.Area.xMin - 1, room.Area.yMax + 1 + wallExtraHeight, 0),
                null);
            baseLayerTilemap.SetTile(new Vector3Int(room.Area.xMax + 1, room.Area.yMax + 1 + wallExtraHeight, 0),
                null);
            
            // Левый верхний угол
            colliderLayerTilemap.SetTile(new Vector3Int(room.Area.xMin - 1, room.Area.yMax + 1 + wallExtraHeight, 0),
                leftTopCornerWallTile);
            // Правый верхний угол
            colliderLayerTilemap.SetTile(new Vector3Int(room.Area.xMax + 1, room.Area.yMax + 1 + wallExtraHeight, 0),
                rightTopCornerWallTile);*/
        }
    }

    private void SpawnPlayer(Dungeon dungeon)
    {
        var rand = new System.Random();

        int index = rand.Next(0, dungeon.Rooms.Count);

        player.transform.position =
            new Vector3(dungeon.Rooms[index].Area.center.x, dungeon.Rooms[index].Area.center.y, 0);
        
        player.SetActive(true);
        //Instantiate(player, new Vector3(dungeon.Rooms[index].Area.center.x, dungeon.Rooms[index].Area.center.y, 0), Quaternion.identity);
    }
}
