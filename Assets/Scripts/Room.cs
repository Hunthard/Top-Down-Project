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

    // public void AddRoom(Vector2Int pos)
    // {
    //     // Vector2Int direction = new Vector2Int();
    //     //
    //     // direction.x = pos.x - (int) Area.center.x;
    //     // if (direction.x != 0)
    //     // {
    //     //     direction.x /= Math.Abs(direction.x);
    //     // }
    //     //
    //     // var x = Area.Contains(new Vector2Int(pos.x - direction.x, pos.y)) ? pos.x - direction.x : pos.x;
    //     //
    //     // direction.y = pos.y - (int) Area.center.y;
    //     // if (direction.y != 0)
    //     // {
    //     //     direction.y /= Math.Abs(direction.y);
    //     // }
    //     //
    //     // var y = Area.Contains(new Vector2Int(pos.x, pos.y - direction.y)) ? pos.y - direction.y : pos.y;
    //     
    //     Doors.Add(new Vector2Int(pos.x, pos.y));
    // }
}
