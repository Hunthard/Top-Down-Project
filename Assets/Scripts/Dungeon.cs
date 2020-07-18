using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Constant;

public class Dungeon
{
    /// <summary>
    /// Высота
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Ширина
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Количество изолированных регионов
    /// </summary>
    private int regions;

    /// <summary>
    /// Список комнат в подземелье
    /// </summary>
    private List<Room> rooms;

    /// <summary>
    /// Список возможных соединений между соседними регионами
    /// </summary>
    private List<Vector2Int> regionConnectors;

    /// <summary>
    /// Массив значений ячеек для отрисовки подземелья
    /// </summary>
    private int[,] cells;

    public Dungeon(int width, int height)
    {
        Width = width;
        Height = height;

        if (Height % 2 == 0 || Width % 2 == 0)
        {
            throw new Exception("Maze must be odd-size");
        }

        regions = 0;

        rooms = new List<Room>();

        regionConnectors = new List<Vector2Int>();

        cells = new int[Width, Height];
    }

    public int this[int x, int y]
    {
        get
        {
            return cells[x, y];
        }
    }

    /// <summary>
    /// Добавить комнату и заполнить её область в массиве ячеек
    /// значением соответствующего региона
    /// </summary>
    /// <param name="newRoom"></param>
    private void AddRoom(ref RectInt newRoom)
    {
        rooms.Add(new Room(ref newRoom, regions));

        regions++;

        for (int x = newRoom.xMin; x <= newRoom.xMax; x++)
            for (int y = newRoom.yMin; y <= newRoom.yMax; y++)
                cells[x, y] = regions;
    }

    /// <summary>
    /// Проверить комнаты на пересечение
    /// </summary>
    /// <param name="checkingRoom">Проверяемая комната</param>
    /// <param name="otherRooms">Список имеющихся комнат</param>
    /// <returns></returns>
    private bool IsOverlaps(RectInt checkingRoom, List<Room> otherRooms)
    {
        // Искусственое увеличение размеров комнаты чтобы сохранить
        // минимальное расстояние между комнатами в 1 блок
        checkingRoom.SetMinMax(new Vector2Int(checkingRoom.xMin - 1, checkingRoom.yMin - 1),
                            new Vector2Int(checkingRoom.xMax + 1, checkingRoom.yMax + 1)
                            );

        foreach (var room in otherRooms)
        {
            if (checkingRoom.Overlaps(room.area)) return true;
        }

        return false;
    }

    /// <summary>
    /// Найти свободных соседей ячейки
    /// </summary>
    /// <param name="_currentCell">Координаты текущей ячейки</param>
    private List<Vector2Int> FindNeighbors(Vector2Int _currentCell)
    {
        List<Vector2Int> passageDirections = new List<Vector2Int>();

        foreach (var dir in Container.DIRECTIONS)
        {
            var _x = (_currentCell + dir * 2).x;
            var _y = (_currentCell + dir * 2).y;

            if (_x > 0 && _x < Width && _y > 0 && _y < Height &&
                Container.IsWall(cells[_x, _y])
                )
            {
                passageDirections.Add(dir);
            }
        }

        return passageDirections;
    }

    /// <summary>
    /// Добавить ячейку к лабиринту и заполнить её значением соответсвующего региона
    /// </summary>
    /// <param name="position">Координаты ячейки</param>
    private void Carve(Vector2Int position)
    {
        cells[position.x, position.y] = regions;
    }

    /// <summary>
    /// Реализация рекурсивного алгоритма с возвратом
    /// </summary>
    /// <param name="startPosition">Начальная позиция</param>
    private void RecursiveBacktracking(Vector2Int startPosition)
    {
        List<Vector2Int> cellList = new List<Vector2Int>();

        var rand = new System.Random();

        cellList.Add(startPosition);
        Carve(startPosition);

        regions++;

        while (cellList.Count != 0)
        {
            var currentCell = cellList.Last();

            List<Vector2Int> passageDirections = FindNeighbors(currentCell);

            if (passageDirections.Count != 0)
            {
                int index = rand.Next(passageDirections.Count);

                Carve(currentCell + passageDirections[index]);

                cellList.Add(currentCell + passageDirections[index] * 2);
                Carve(currentCell + passageDirections[index] * 2);
            }
            else
            {
                cellList.RemoveAt(cellList.Count - 1);
            }
        }
    }

    /// <summary>
    /// Найти все возможные соединения между всеми регионами
    /// </summary>
    private void FindConnections()
    {
        foreach (var _room in rooms)
        {
            _room.connections.Clear();

            int _x;
            int _y;

            for (_x = _room.area.xMin; _x <= _room.area.xMax; _x++)
            {
                _y = _room.area.yMin - 1;
                if (_y > 0 && cells[_x, _y - 1] != (int)Container.Blocks.Wall)
                {
                    _room.connections.Add(new Vector2Int(_x, _y));

                    regionConnectors.Add(new Vector2Int(_x, _y));
                }

                _y = _room.area.yMax + 1;
                if (_y < Height - 1 && cells[_x, _y + 1] != (int)Container.Blocks.Wall)
                {
                    _room.connections.Add(new Vector2Int(_x, _y));

                    regionConnectors.Add(new Vector2Int(_x, _y));
                }
            }

            for (_y = _room.area.yMin; _y <= _room.area.yMax; _y++)
            {
                _x = _room.area.xMin - 1;
                if (_x > 0 && cells[_x - 1, _y] != (int)Container.Blocks.Wall)
                {
                    _room.connections.Add(new Vector2Int(_x, _y));

                    regionConnectors.Add(new Vector2Int(_x, _y));
                }

                _x = _room.area.xMax + 1;
                if (_x < Width - 1 && cells[_x + 1, _y] != (int)Container.Blocks.Wall)
                {
                    _room.connections.Add(new Vector2Int(_x, _y));

                    regionConnectors.Add(new Vector2Int(_x, _y));
                }
            }
        }
    }

    /// <summary>
    /// Объединить все регионы в один чтобы избавиться от изолированных участков
    /// </summary>
    private void MergeRegions()
    {
        while (regionConnectors.Count > 0)
        {
            var rand = UnityEngine.Random.Range(0, regionConnectors.Count);

            var _connector = regionConnectors[rand];

            if (cells[_connector.x + 1, _connector.y] == (int)Container.Blocks.Wall || cells[_connector.x - 1, _connector.y] == (int)Container.Blocks.Wall)
            {
                if (cells[_connector.x, _connector.y + 1] != cells[_connector.x, _connector.y - 1])
                {
                    var oldRegion = Math.Max(cells[_connector.x, _connector.y - 1], cells[_connector.x, _connector.y + 1]);
                    var newRegion = Math.Min(cells[_connector.x, _connector.y - 1], cells[_connector.x, _connector.y + 1]);

                    cells[_connector.x, _connector.y] = newRegion;

                    ConnectToRegion(newRegion, oldRegion);
                }
            }
            else
            {
                if (cells[_connector.x + 1, _connector.y] != cells[_connector.x - 1, _connector.y])
                {
                    var oldRegion = Math.Max(cells[_connector.x - 1, _connector.y], cells[_connector.x + 1, _connector.y]);
                    var newRegion = Math.Min(cells[_connector.x - 1, _connector.y], cells[_connector.x + 1, _connector.y]);

                    cells[_connector.x, _connector.y] = newRegion;

                    ConnectToRegion(newRegion, oldRegion);
                }
            }

            regionConnectors.RemoveAt(rand);
        }
    }

    /// <summary>
    /// Соединить два региона
    /// </summary>
    /// <param name="newRegion">Новый регион</param>
    /// <param name="oldRegion">Старый регион</param>
    private void ConnectToRegion(in int newRegion, in int oldRegion)
    {
        for (int _x = 1; _x < Width; _x++)
        {
            for (int _y = 1; _y < Height; _y++)
            {
                if (cells[_x, _y] == oldRegion)
                {
                    cells[_x, _y] = newRegion;
                }
            }
        }

        regions--;
    }

    /// <summary>
    /// Создать и разместить комнаты
    /// </summary>
    /// <param name="logEnable">Логирование</param>
    public void GenerateRooms(int attemptsToPlaceRooms = 200, int roomExtraSize = 0, bool logEnable = false)
    {
        for (int i = 0; i < attemptsToPlaceRooms; i++)
        {
            Debug.Log("Attempt to spawn room");

            // Высота и ширина комнаты
            var size = UnityEngine.Random.Range(1, 3 + roomExtraSize) * 2 + 2;

            // Значение, добавляющееся к ширине или высоте комнаты,
            // чтобы спавнить не только квадратные комнаты
            var rectangularity = UnityEngine.Random.Range(0, (size / 2)) * 2;

            var _width = size;
            var _height = size;

            if (UnityEngine.Random.Range(0f, 2f) <= 1.0)
            {
                _width += rectangularity;
            }
            else
            {
                _height += rectangularity;
            }

            // Костыли из чисел в правой части нужны для сохранения
            // расстояния между комнатами равному нечётному кол-ву блоков 
            var x = UnityEngine.Random.Range(0, (Width - _width) - 2) / 2 * 2 + 1;
            var y = UnityEngine.Random.Range(0, (Height - _height) - 2) / 2 * 2 + 1;

            var newRoom = new RectInt(x, y, _width, _height);

            // Проверка на пересечение новой комнаты с уже имеющимеся
            if (IsOverlaps(newRoom, rooms)) continue;

            AddRoom(ref newRoom);
        }

        if (logEnable)
        {
            Debug.Log("Room spawn is done");
        }
    }

    /// <summary>
    /// Сгенерировать лабиринты
    /// </summary>
    /// <param name="logEnable">Логирование</param>
    public void GenerateMaze(bool logEnable = false)
    {
        for (int _x = 1; _x < Width; _x += 2)
        {
            for (int _y = 1; _y < Height; _y += 2)
            {
                if (cells[_x, _y] == (int)Container.Blocks.Wall)
                {
                    RecursiveBacktracking(new Vector2Int(_x, _y));
                }
            }
        }

        if (logEnable)
        {
            Debug.Log("Maze generation is done");
        }
    }

    /// <summary>
    /// Объединить регионы
    /// </summary>
    /// <param name="logEnable">Логирование</param>
    public void ConnectRegions(bool logEnable = false)
    {
        FindConnections();

        MergeRegions();

        if (logEnable)
        {
            Debug.Log($"{regions} regions left");
            Debug.Log("Connecting regions is done");
        }
    }

    /// <summary>
    /// Избавиться от тупиков
    /// </summary>
    /// <param name="logEnable">Логирование</param>
    public void RemoveDeadEnds(bool logEnable = false)
    {
        var done = false;
        int stop = 0;

        while (!done)
        {
            if (stop++ > Height * Width * 5) break;

            done = true;

            for (int _x = 1; _x < Width; _x++)
            {
                for (int _y = 1; _y < Height; _y++)
                {
                    if (cells[_x, _y] == (int)Container.Blocks.Wall) continue;

                    int exits = 0;

                    foreach (var dir in Container.DIRECTIONS)
                    {
                        if (cells[_x + dir.x, _y + dir.y] != (int)Container.Blocks.Wall) exits++;
                    }

                    if (exits > 1) continue;

                    done = false;
                    cells[_x, _y] = (int)Container.Blocks.Wall;
                }
            }
        }

        if (logEnable)
        {
            Debug.Log($"{stop} ITERATIONS");
            Debug.Log("Dead ends was removed");
        }
    }
}
