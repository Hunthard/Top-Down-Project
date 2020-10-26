using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DungeonUtilities
{
    public static class Container
    {
        public static Vector2Int[] DIRECTIONS = {
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down
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

    public class DirectionList : IEnumerable
    {
        public DirectionNode Head { get; private set; }
        public int count;

        public DirectionList()
        {
            Head = null;
            count = 0;
        }
        
        public void Add(Vector2Int nextCell, Vector2Int nextPoint)
        {
            DirectionNode newNode = new DirectionNode();

            if (Head == null)
            {
                Head = newNode;
                Head.Next = newNode;
                Head.Prev = newNode;
            }
            else
            {
                newNode.Prev = Head.Prev;
                newNode.Next = Head;
                Head.Prev.Next = newNode;
                Head.Prev = newNode;
            }

            newNode.nextCell = nextCell;
            newNode.nextPoint = nextPoint;
            count++;
        }

        public void OrderPointDirections(Vector2Int direction)
        {
            var curDir = Head;
            
            for (int i = 0; i < count; i++)
            {
                if (curDir.nextPoint == direction)
                {
                    Head = curDir.Prev;
                    
                    break;
                }
                
                curDir = curDir.Next;
            }
        }

        public IEnumerator GetEnumerator()
        {
            DirectionNode current = Head;

            for (int i = 0; i < count; i++)
            {
                yield return current;
                current = current.Next;
            }
        }
    }

    public class DirectionNode
    {
        public Vector2Int nextCell;
        public Vector2Int nextPoint;
        public DirectionNode Next { get; set; }
        public DirectionNode Prev { get; set; }
    }
}
