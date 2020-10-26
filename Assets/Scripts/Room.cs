using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public RectInt Area;
    public List<Vector2Int> Connections;
    //public List<Vector2Int> Doors;
    public int Region;

    public Room(ref RectInt newRoom, int regionNum)
    {
        Area = newRoom;
        Connections = new List<Vector2Int>(Area.height + Area.width);
        //Doors = new List<Vector2Int>(Area.height * Area.width);
        Region = regionNum;
    }
}
