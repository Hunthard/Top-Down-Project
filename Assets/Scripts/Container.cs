using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Constant
{
    public static class Container
    {
        public static Vector2Int[] DIRECTIONS = new Vector2Int[] { Vector2Int.up, Vector2Int.right,
                                                                   Vector2Int.down, Vector2Int.left
                                                                 };

        public enum Blocks : int
        {
            Wall = 0,
        }

        public static bool IsWall(int cell)
        {
            return cell == (int)Blocks.Wall;
        }
    }

}
