using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Constant
{
    public static class Container
    {
        public static Vector2Int[] DIRECTIONS = { Vector2Int.up, Vector2Int.right,
                                                                   Vector2Int.down, Vector2Int.left
                                                                 };
        
        public struct Parameter
        {
            public int widthInBlocks;
            public int heightInBlocks;
            public int passageWidthInBlocks;
            public int borderWidthInBlocks;
            public double gateWidthInBlocks;
        }

        public enum Blocks : int
        {
            Wall = 0,
        }

        /// <summary>
        /// Возвращает true если ячейка является стеной
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>bool</returns>
        public static bool IsWall(int cell)
        {
            return cell == (int)Blocks.Wall;
        }
    }
}
