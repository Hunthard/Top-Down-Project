using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DungeonUtilities;

public class Dungeon
{
    /// <summary>
    /// Ширина
    /// </summary>
    private int _width;
    
    /// <summary>
    /// Высота
    /// </summary>
    private int _height;

    private int _widthInBlocks;
    private int _heightInBlocks;
    private int _borderWidthInBlocks;
    
    private int _passageWidthInBlocks;

    private double _gateWidthInBlocks;

    /// <summary>
    /// Количество изолированных регионов
    /// </summary>
    private int _numOfRegions;

    /// <summary>
    /// Список комнат в подземелье
    /// </summary>
    private List<Room> _rooms;

    /// <summary>
    /// Список дверей подземелья
    /// </summary>
    private List<Vector2Int> _doors;

    /// <summary>
    /// Список возможных соединений между соседними регионами
    /// </summary>
    //private List<Vector2Int> _regionConnectors;
    
    /// <summary>
    /// Список возможных направлений пассажа
    /// </summary>
    private List<Vector2Int> _passageDirections;

    /// <summary>
    /// Список ячеек в пассаже
    /// </summary>
    private List<Vector2Int> _cellList;

    private System.Random _random;
    
    public int Width
    { 
        get => _width;
        private set => _width = value;
    }
    
    public int Height
    {
        get => _height; 
        private set => _height = value;
    }
    
    public List<Room> Rooms
    {
        get => _rooms;
        private set => _rooms = value;
    }

    public List<Vector2Int> Doors
    {
        get => _doors;
        private set => _doors = value;
    }

    /// <summary>
    /// Массив значений ячеек для отрисовки подземелья
    /// </summary>
    private int[,] _cells;

    public Dungeon(int widthInBlocks, int heightInBlocks, int passageWidthInBlocks, int borderWidthInBlocks, int seed)
    {
        _widthInBlocks = widthInBlocks;
        _heightInBlocks = heightInBlocks;
        _borderWidthInBlocks = borderWidthInBlocks;
        _passageWidthInBlocks = passageWidthInBlocks;
        
        Width = _passageWidthInBlocks * _widthInBlocks + _widthInBlocks - 1 + 2 * _borderWidthInBlocks;
        Height = _passageWidthInBlocks * _heightInBlocks + _heightInBlocks - 1 + 2 * _borderWidthInBlocks;

        if (Height % 2 == 0 || Width % 2 == 0)
        {
            throw new Exception("Maze must be odd-size");
        }

        if (_passageWidthInBlocks % 2 == 0)
        {
            throw new Exception("Value must be odd");
        }

        _numOfRegions = 0;

        Rooms = new List<Room>(Width * Height / 2);
        
        _doors = new List<Vector2Int>(Width + Height);

        //_regionConnectors = new List<Vector2Int>(Width * Height / 2);

        _passageDirections = new List<Vector2Int>(4);

        _cellList = new List<Vector2Int>(Width * Height / 2);

        _random = new System.Random(seed);

        _cells = new int[Width, Height];
    }

    public Dungeon(Container.Parameter parameters, int seed)
    {
        _widthInBlocks = parameters.widthInBlocks;
        _heightInBlocks = parameters.heightInBlocks;
        _passageWidthInBlocks = parameters.passageWidthInBlocks;
        _borderWidthInBlocks = parameters.borderWidthInBlocks;
        _gateWidthInBlocks = parameters.gateWidthInBlocks;
        
        Width = _passageWidthInBlocks * _widthInBlocks + _widthInBlocks - 1 + 2 * _borderWidthInBlocks;
        Height = _passageWidthInBlocks * _heightInBlocks + _heightInBlocks - 1 + 2 * _borderWidthInBlocks;
        
        _numOfRegions = 0;

        Rooms = new List<Room>(Width * Height / 2);
        
        Doors = new List<Vector2Int>(Width + Height);

        //_regionConnectors = new List<Vector2Int>(Width * Height / 2);

        _passageDirections = new List<Vector2Int>(4);

        _cellList = new List<Vector2Int>(Width * Height / 2);

        _random = new System.Random(seed);

        _cells = new int[Width, Height];
    }

    /// <summary>
    /// Обращение по индексу к значению ячейки массива _cells
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <returns>Значение элемента массива _cells</returns>
    public int this[int x, int y] => _cells[x, y];

    /// <summary>
    /// Добавить комнату и заполнить её область в массиве ячеек
    /// значением соответствующего региона
    /// </summary>
    /// <param name="newRoom">Новая комната</param>
    private void AddRoom(ref RectInt newRoom)
    {
        Rooms.Add(new Room(ref newRoom, _numOfRegions));

        _numOfRegions++;

        for (int x = newRoom.xMin; x <= newRoom.xMax; x++)
            for (int y = newRoom.yMin; y <= newRoom.yMax; y++)
                _cells[x, y] = _numOfRegions;
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
        // минимальное расстояние между комнатами равным ширине коридора
        checkingRoom.SetMinMax(new Vector2Int(checkingRoom.xMin - _passageWidthInBlocks,
                                              checkingRoom.yMin - _passageWidthInBlocks),
                               new Vector2Int(checkingRoom.xMax + _passageWidthInBlocks,
                                              checkingRoom.yMax + _passageWidthInBlocks)
                            );

        foreach (var room in otherRooms)
        {
            if (checkingRoom.Overlaps(room.Area)) return true;
        }

        return false;
    }

    /// <summary>
    /// Реализация рекурсивного алгоритма с возвратом
    /// </summary>
    /// <param name="startPosition">Начальная позиция</param>
    private void RecursiveBacktracking(Vector2Int startPosition)
    {
        _cellList.Clear();

        _cellList.Add(startPosition);
        Carve(startPosition, Vector2Int.zero);
        
        _numOfRegions++;

        while (_cellList.Count != 0)
        {
            var currentCell = _cellList.Last();
            
            FindNeighbors(currentCell);

            if (_passageDirections.Count != 0)
            {
                int index = _random.Next(_passageDirections.Count);
                
                Carve(currentCell, _passageDirections[index] * (_passageWidthInBlocks / 2 + 1));

                _cellList.Add(currentCell + _passageDirections[index] * (_passageWidthInBlocks * 2 - _passageWidthInBlocks / 2 * 2));

                Carve(currentCell,_passageDirections[index] * (_passageWidthInBlocks * 2 - _passageWidthInBlocks / 2 * 2));
            }
            else
            {
                _cellList.RemoveAt(_cellList.Count - 1);
            }
        }
    }
    
    //// <summary>
    //// Добавить ячейку к лабиринту и заполнить её значением соответсвующего региона
    //// </summary>
    //// <param name="position">Координаты ячейки</param>
    
    /// <summary>
    /// Добавить ячейку к лабиринту и заполнить её значением соответствующего региона
    /// </summary>
    /// <param name="position">Позиция ячейки</param>
    /// <param name="direction">Направление</param>
    private void Carve(Vector2Int position, Vector2Int direction)
    {
        FillPassage(position + direction);
    }

    private void FillPassage(Vector2Int position)
    {
        for (int x = -_passageWidthInBlocks / 2; x < (_passageWidthInBlocks / 2) + 1; x++)
        {
            for (int y = -_passageWidthInBlocks / 2; y < (_passageWidthInBlocks / 2) + 1; y++)
            {
                _cells[position.x + x, position.y + y] = _numOfRegions;
            }
        }
    }
    
    /// <summary>
    /// Найти свободных соседей ячейки
    /// </summary>
    /// <param name="_currentCell">Координаты текущей ячейки</param>
    private void FindNeighbors(Vector2Int _currentCell)
    {
        _passageDirections.Clear();

        foreach (var dir in Container.DIRECTIONS)    
        {
            var x = (_currentCell + dir * (_passageWidthInBlocks + 1)).x;
            var y = (_currentCell + dir * (_passageWidthInBlocks + 1)).y;

            if (x > 0 && x < Width && y > 0 && y < Height &&
                Container.IsWall(_cells[x, y])
            )
            {
                _passageDirections.Add(dir);
            }
        }
    }

    /// <summary>
    /// Найти все возможные соединения между всеми регионами
    /// </summary>
    private void FindConnections()
    {
        int x;
        int y;

        int gateBounds = 1;
        
        foreach (var _room in Rooms)
        {
            _room.Connections.Clear();

            for (x = _room.Area.xMin + gateBounds; x <= _room.Area.xMax - gateBounds; x++)
            {
                y = _room.Area.yMin - 1;
                if (y > 0 && _cells[x, y - 1] != (int)Container.Blocks.Wall)
                {
                    _room.Connections.Add(new Vector2Int(x, y));

                    //_regionConnectors.Add(new Vector2Int(x, y));
                }

                y = _room.Area.yMax + 1;
                if (y < Height - 1 && _cells[x, y + 1] != (int)Container.Blocks.Wall)
                {
                    _room.Connections.Add(new Vector2Int(x, y));

                    //_regionConnectors.Add(new Vector2Int(x, y));
                }
            }

            for (y = _room.Area.yMin + gateBounds; y <= _room.Area.yMax - gateBounds; y++)
            {
                x = _room.Area.xMin - 1;
                if (x > 0 && _cells[x - 1, y] != (int)Container.Blocks.Wall)
                {
                    _room.Connections.Add(new Vector2Int(x, y));

                    //_regionConnectors.Add(new Vector2Int(x, y));
                }

                x = _room.Area.xMax + 1;
                if (x < Width - 1 && _cells[x + 1, y] != (int)Container.Blocks.Wall)
                {
                    _room.Connections.Add(new Vector2Int(x, y));

                    //_regionConnectors.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    /// <summary>
    /// Объединить все регионы в один чтобы избавиться от изолированных участков
    /// </summary>
    private void MergeRegions()
    {
        foreach (var room in Rooms)
        {
            while (room.Connections.Count > 0)
            {
                var rand = _random.Next(0, room.Connections.Count);

                var _connector = room.Connections[rand];

                if (_cells[_connector.x + 1, _connector.y] == (int)Container.Blocks.Wall || _cells[_connector.x - 1, _connector.y] == (int)Container.Blocks.Wall)
                {
                    if (_cells[_connector.x, _connector.y + 1] != _cells[_connector.x, _connector.y - 1])
                    {
                        var oldRegion = Math.Max(_cells[_connector.x, _connector.y - 1], _cells[_connector.x, _connector.y + 1]);
                        var newRegion = Math.Min(_cells[_connector.x, _connector.y - 1], _cells[_connector.x, _connector.y + 1]);

                        _cells[_connector.x, _connector.y] = newRegion;

                        //room.AddRoom(room.Connections[rand]);
                        AddDoor(room.Connections[rand]);
                        ConnectToRegion(newRegion, oldRegion);
                    }
                }
                else
                {
                    if (_cells[_connector.x + 1, _connector.y] != _cells[_connector.x - 1, _connector.y])
                    {
                        var oldRegion = Math.Max(_cells[_connector.x - 1, _connector.y], _cells[_connector.x + 1, _connector.y]);
                        var newRegion = Math.Min(_cells[_connector.x - 1, _connector.y], _cells[_connector.x + 1, _connector.y]);

                        _cells[_connector.x, _connector.y] = newRegion;

                        //room.AddRoom(room.Connections[rand]);
                        AddDoor(room.Connections[rand]);
                        ConnectToRegion(newRegion, oldRegion);
                    }
                }

                room.Connections.RemoveAt(rand);
            }
        }

        /*if (_regions > 1)
        {
            for (int x = _passageWidth - _passageWidth / 2; x < Width; x += _passageWidth * 2)
            {
                for (int y = _passageWidth - _passageWidth / 2; y < Height; y += _passageWidth * 2)
                {
                    if (!Container.IsWall(_cells[x, y]) &&
                        _cells[x, y] > 1
                    )
                    {
                        FindNeighbors(new Vector2Int(x, y));
                    }
                }
            }
        }*/
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
                if (_cells[_x, _y] == oldRegion)
                {
                    _cells[_x, _y] = newRegion;
                }
            }
        }

        _numOfRegions--;
    }

    /// <summary>
    /// Создать и разместить комнаты
    /// </summary>
    /// <param name="logEnable">Логирование</param>
    private void GenerateRooms(int attemptsToPlaceRooms, int roomExtraSize, bool logEnable = false)
    {
        // Коэффициент для определения количества
        // блоков минимального размера в ширине и высоте комнаты
        int horizontalBlockCoefficient;
        int verticalBlockCoefficient;
        
        // Ширина и высота комнаты
        int roomWidth;
        int roomHeight;

        // Количество блоков для отступа координат
        int numBlocks;
        
        for (var i = 0; i < attemptsToPlaceRooms; i++)
        {
            horizontalBlockCoefficient = _random.Next(2, 5 + roomExtraSize);
            roomWidth = _passageWidthInBlocks * horizontalBlockCoefficient + horizontalBlockCoefficient - 2;
            
            verticalBlockCoefficient = _random.Next(2, 5 + roomExtraSize);
            roomHeight = _passageWidthInBlocks * verticalBlockCoefficient + verticalBlockCoefficient - 2;
            
            numBlocks = _random.Next(0, _widthInBlocks - horizontalBlockCoefficient);
            var x =  _passageWidthInBlocks * numBlocks + numBlocks + 1;
            numBlocks = _random.Next(0, _heightInBlocks - verticalBlockCoefficient);
            var y = _passageWidthInBlocks * numBlocks + numBlocks + 1;

            var newRoom = new RectInt(x, y, roomWidth, roomHeight);

            // Проверка на пересечение новой комнаты с уже имеющимися
            if (IsOverlaps(newRoom, Rooms)) continue;

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
    private void GenerateMaze(bool logEnable = false)
    {
        for (int x = _passageWidthInBlocks - _passageWidthInBlocks / 2; x < Width; x += _passageWidthInBlocks + 1)
        {
            for (int y = _passageWidthInBlocks - _passageWidthInBlocks / 2; y < Height; y += _passageWidthInBlocks + 1)
            {
                if (Container.IsWall(_cells[x, y]))
                {
                    RecursiveBacktracking(new Vector2Int(x, y));
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
    private void ConnectRegions(bool logEnable = false)
    {
        FindConnections();
        
        MergeRegions();

        if (logEnable)
        {
            Debug.Log($"{_numOfRegions} regions left");
            Debug.Log("Connecting regions is done");
        }
    }

    /// <summary>
    /// Избавиться от тупиков
    /// </summary>
    /// <param name="logEnable">Логирование</param>
    private void RemoveDeadEnds(bool logEnable = false)
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
                    if (_cells[_x, _y] == (int)Container.Blocks.Wall) continue;

                    int exits = 0;

                    foreach (var dir in Container.DIRECTIONS)
                    {
                        if (_cells[_x + dir.x, _y + dir.y] != (int)Container.Blocks.Wall) exits++;
                    }

                    if (exits > 1) continue;

                    done = false;
                    _cells[_x, _y] = (int)Container.Blocks.Wall;
                }
            }
        }

        if (logEnable)
        {
            Debug.Log($"{stop} ITERATIONS");
            Debug.Log("Dead ends was removed");
        }
    }

    private void AddDoor(Vector2Int newDoor)
    {
        Doors.Add(newDoor);
    }

    /// <summary>
    /// Генерация подземелья
    /// </summary>
    /// <param name="attemptsToPlaceRooms">Количество попыток для спавна комнат</param>
    /// <param name="roomExtraSize">Добавочный размер комнаты</param>
    /// <param name="deadEndsEnable">Наличие тупиков</param>
    /// <param name="logEnable">Логирование</param>
    public void Generate(int attemptsToPlaceRooms = 200, int roomExtraSize = 0, bool deadEndsEnable = false, bool logEnable = false)
    {
        GenerateRooms(attemptsToPlaceRooms: attemptsToPlaceRooms, roomExtraSize: roomExtraSize, logEnable: logEnable);
        GenerateMaze(logEnable);
        ConnectRegions(logEnable);

        if (!deadEndsEnable)
        {
            RemoveDeadEnds(logEnable);
        }

        if (logEnable)
        {
            Debug.Log($"\n Width: {Width} \n Height: {Height}");
        }
    }
}
