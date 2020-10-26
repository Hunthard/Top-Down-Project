using System;
using DungeonUtilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    private static Dungeon _dungeon;
    
    private int _width;
    private int _height;

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
        Debug.LogError("Awake");
    }

    public void Generate()
    {
        Debug.LogError("Generate");
        globalLight = GameObject.Find("Global Light 2D");
        
        Prepare();
        // кол-во блоков * ширину коридора + кол-во блоков - 1 + 2 блока для границ
        
        _dungeon = new Dungeon(Parameters, seed);
        
        _dungeon.Generate(attemptsToPlaceRooms, roomExtraSize, deadEnds, logEnable);
        
        DrawDungeon(_dungeon);
    }
    
    void Prepare()
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
        Debug.LogError("Start");
        Debug.Log($"Math.Round(12.3, ToEven) = {Math.Round(12.3, MidpointRounding.ToEven)})");
        Debug.Log($"Math.Round(12.7, ToEven) = {Math.Round(12.7, MidpointRounding.ToEven)})");
    }

    private bool a = true;
    private bool b = true;
    private bool c = true;
    private void Update()
    {
        if (a)
        {
            a = false;
            Debug.LogError("Update");
        }
        
    }
    
    private void FixedUpdate()
    {
        if (b)
        {
            b = false;
            Debug.LogError("FixedUpdate");
        }
    }
    
    private void LateUpdate()
    {
        if (c)
        {
            c = false;
            Debug.LogError("LateUpdate");
        }
    }
    
    private IEnumerator ShadowCaster()
    {
        colliderLayerTilemap.gameObject.GetComponent<CompositeCollider2D>().GenerateGeometry();
        Debug.LogError("1 " + colliderLayerTilemap.gameObject.GetComponent<CompositeCollider2D>().pathCount);
        colliderLayerTilemap.SetTile(Vector3Int.zero, null);
        colliderLayerTilemap.gameObject.GetComponent<CompositeCollider2D>().GenerateGeometry();
        Debug.LogError("2 " + colliderLayerTilemap.gameObject.GetComponent<CompositeCollider2D>().pathCount);
        yield return new WaitForEndOfFrame();
        colliderLayerTilemap.gameObject.GetComponent<CompositeCollider2D>().GenerateGeometry();
        Debug.LogError("3 " + colliderLayerTilemap.gameObject.GetComponent<CompositeCollider2D>().pathCount);
        yield return new WaitForEndOfFrame();
        colliderLayerTilemap.gameObject.GetComponent<CompositeCollider2D>().GenerateGeometry();
        Debug.LogError("4 " + colliderLayerTilemap.gameObject.GetComponent<CompositeCollider2D>().pathCount);
        ShadowCaster2DGenerator.GenerateShadowCasters();
        colliderLayerTilemap.gameObject.GetComponent<CompositeCollider2D>().GenerateGeometry();
        Debug.LogError("5 " + colliderLayerTilemap.gameObject.GetComponent<CompositeCollider2D>().pathCount);
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
                colliderLayerTilemap.SetTile(new Vector3Int(door.x, door.y, 0), null);
                
                baseLayerTilemap.SetTile(new Vector3Int(door.x, door.y, 0), bricksTile);
            }
        }
        
        Debug.Log("Dungeon drawn");
        //StartCoroutine(ShadowCaster());
        SetWallShadows();
    }

    public Vector3 GetRandomPoint
    {
        get
        {
            var rand = new System.Random();
            int index = rand.Next(0, _dungeon.Rooms.Count);
            return new Vector3(_dungeon.Rooms[index].Area.center.x, _dungeon.Rooms[index].Area.center.y, 0);
        }
    }

    private List<Vector3> _pointsInPath;
    private List<Vector2Int> _cellList;
    public void SetWallShadows()
    {
        _pointsInPath = new List<Vector3>(_width * _height / 2);
        
        var startPoint = new Vector2Int(1, 2);
        var currentPoint = new Vector2Int(1,1);
        
        var currentDirection = Vector2Int.right;

        _pointsInPath.Add(new Vector3(
            currentPoint.x - Vector2Int.up.x * 0.3f,
            currentPoint.y - Vector2Int.up.y * 0.3f)
            );
        
        var directions = new DirectionList();

        directions.Add(new Vector2Int(0, 0), Vector2Int.up); // up
        directions.Add(new Vector2Int(0, -1), Vector2Int.right); // right
        directions.Add(new Vector2Int(-1,-1), Vector2Int.down); // down
        directions.Add(new Vector2Int(-1, 0), Vector2Int.left); // left

        int temp = 0;
        do
        {
            directions.OrderPointDirections(currentDirection);

            foreach (DirectionNode direction in directions)
            {
                var nextCell = currentPoint + direction.nextCell;
                if (_dungeon[nextCell.x, nextCell.y] == 0)
                {
                    currentPoint = currentPoint + direction.nextPoint;

                    _pointsInPath.Add(new Vector3(
                        currentPoint.x,
                        currentPoint.y)
                    );
                    currentDirection = direction.nextPoint;
                    break;
                }
            }

            if (temp > 500) break;
            temp++;

        } while (currentPoint != startPoint);
        
        _pointsInPath.Add(new Vector3(0, 2));
        _pointsInPath.Add(new Vector3(0, _height));
        _pointsInPath.Add(new Vector3(_width, _height));
        _pointsInPath.Add(new Vector3(_width, 0));
        _pointsInPath.Add(new Vector3(0, 0));
        _pointsInPath.Add(new Vector3(0, 2));
        _pointsInPath.Add(new Vector3(1, 2));

        TempNamespace.ShadowCaster2DGenerator.GenerateTilemapShadowCasters(_pointsInPath);
    }

    public void AddToPath(Vector2Int currentCell)
    {
        foreach (var direction in Container.DIRECTIONS)
        {
            var x = currentCell.x + direction.x;
            var y = currentCell.y + direction.y;
            
            if (x > 0 && x < _width && y > 0 && y < _height &&
                _dungeon[x, y] == 0)
            {
                _cellList.Add(new Vector2Int(x, y));

                if (direction == Vector2Int.up)
                {
                    
                }
            }
        }
    }
}
