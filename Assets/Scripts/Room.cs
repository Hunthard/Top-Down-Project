using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    
    public RectInt area;
    public List<Vector2Int> connections;
    public Vector2Int door;
    public int region;

    public Room(ref RectInt newRoom, int regionNum)
    {
        area = newRoom;
        connections = new List<Vector2Int>();
        door = default;
        region = regionNum;
    }
}
