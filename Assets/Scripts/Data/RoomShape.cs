using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Numerics;
using UnityEngine;

public enum Facing
{
    Up, Down, Left, Right
}

/// <summary>
/// Unique definition for a group of tiles to be placed in the grid, representing a room.
/// </summary>
[Serializable] 
public class RoomShape
{
    public int Size;
    public List<bool> SquashedShape;
    public bool[,] Shape;

    public RoomShape(int size, List<bool> ss)
    {
        Size = size;
        SquashedShape = ss;
        ShapeInit();
    }
    
    public bool GetAt(int x, int y)
    {
        return SquashedShape[x * Size + y];
    }

    public void ShapeInit()
    {
        Shape = new bool[Size, Size];
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Shape[i, j] = SquashedShape[i * Size + j];
            }
        }
    }
    
    public int NumberOfRooms => RoomCount(Shape);

    public List<bool[,]> Orientations()
    {
        List<bool[,]> orientations = new List<bool[,]>(){Shape};
        bool[,] flip = new bool[Shape.GetLength(1),Shape.GetLength(0)];
        for (int i = 0; i < Shape.GetLength(0); i++)
        {
            for (int j = 0; j < Shape.GetLength(1); j++)
            {
                flip[j, i] = Shape[i, j];
            }
        }
        orientations.Add(flip);
        
        bool[,] reverse = new bool[Shape.GetLength(0),Shape.GetLength(1)];
        for (int i = 0; i < Shape.GetLength(0); i++)
        {
            for (int j = 0; j < Shape.GetLength(1); j++)
            {
                reverse[Shape.GetLength(0)-i-1, Shape.GetLength(1)-j-1] = Shape[i, j];
            }
        }
        orientations.Add(reverse);
        
        bool[,] flipReverse = new bool[Shape.GetLength(1),Shape.GetLength(0)];
        for (int i = 0; i < Shape.GetLength(0); i++)
        {
            for (int j = 0; j < Shape.GetLength(1); j++)
            {
                flipReverse[Shape.GetLength(0)-j-1, Shape.GetLength(1)-i-1] = Shape[i, j];
            }
        }
        orientations.Add(flipReverse);

        return orientations;
    }

    private static int RoomCount(bool[,] grid)
    {
        int count = 0;
        foreach (var b in grid)
        {
            count += b ? 1 : 0;
        }

        return count;
    }
    
    public static List<int[]> GetAllActive(bool[,] grid)
    {
        List<int[]> toReturn = new List<int[]>();
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if(grid[i, j]) toReturn.Add(new []{i, j});
            }
        }

        return toReturn;
    }

    public List<int[]> GetAllActive()
    {
        return GetAllActive(Shape);
    }

    public static int[] PickRandomActive(bool[,] grid)
    {
        int rand = UnityEngine.Random.Range(0, RoomCount(grid));
        int c = 0;
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                c += grid[i, j] ? 1 : 0;
                if (c > rand) return new[] {i, j};
            }
        }
        
        throw new ArgumentOutOfRangeException();
    }
    
    public int[] PickRandomActive()
    {
        return PickRandomActive(Shape);
    }

    private static Vector2Int FacingDirection(Facing facing)
    {
        return facing switch
        {
            Facing.Up => Vector2Int.up,
            Facing.Down => Vector2Int.down,
            Facing.Left => Vector2Int.left,
            Facing.Right => Vector2Int.right,
            _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, null)
        };
    }

    public List<int[]> GetAllOnBoundary(Facing facing)
    {
        List<int[]> toReturn = new List<int[]>();
        for (int i = 0; i < Shape.GetLength(0); i++)
        {
            for (int j = 0; j < Shape.GetLength(1); j++)
            {
                if (!Shape[i, j]) continue;
                Vector2Int fDir = FacingDirection(facing);
                if (!Shape[i + fDir.x, j + fDir.y]) toReturn.Add(new []{i, j});
            }
        }
        
        return toReturn;
    }

    public int[] GetRandomOnBoundary(Facing facing)
    {
        var all = GetAllOnBoundary(facing);
        return all[UnityEngine.Random.Range(0, all.Count)];
    }

    public bool Contiguous => IsContiguous(Shape);

    //Contiguity, diagonals don't count, empty counts as false.
    public static bool IsContiguous(bool[,] shape)
    {
        bool[,] connectedComponent = new bool[shape.GetLength(0),shape.GetLength(1)];
        for (int i = 0; i < shape.GetLength(0); i++)
        {
            for (int j = 0; j < shape.GetLength(1); j++)
            {
                connectedComponent[i, j] = false;
            }
        }
        (int, int) starting = (0, 0);
        bool anyFilled = false;
        Queue<(int, int)> q = new Queue<(int, int)>();
        
        //Choose basepoint of a connected component, return false immediately if empty.
        for (int i = 0; i < shape.GetLength(0) && !anyFilled; i++)
        {
            for (int j = 0; j < shape.GetLength(1) && !anyFilled; j++)
            {
                if (shape[i, j])
                {
                    starting = (i, j);
                    connectedComponent[i, j] = true;
                    q.Enqueue((i,j));
                    anyFilled = true;
                }
            }
        }

        if (!anyFilled) return false;

        bool ConnectedComponentIndex(int i, int j)
        {
            if (i < 0 || i >= connectedComponent.GetLength(0)) return false;
            if (j < 0 || j >= connectedComponent.GetLength(1)) return false;
            return connectedComponent[i, j];
        }
        
        bool ShapeIndex(int i, int j)
        {
            if (i < 0 || i >= shape.GetLength(0)) return false;
            if (j < 0 || j >= shape.GetLength(1)) return false;
            return shape[i, j];
        }

        
        
        //Fill out this connected component
        while (q.Count > 0)
        {
            (int, int) coord = q.Dequeue();
            if (ShapeIndex(coord.Item1 + 1, coord.Item2) && !ConnectedComponentIndex(coord.Item1 + 1, coord.Item2))
            {
                connectedComponent[coord.Item1 + 1, coord.Item2] = true;
                q.Enqueue((coord.Item1 + 1, coord.Item2));
            }
            if (ShapeIndex(coord.Item1, coord.Item2 + 1) && !ConnectedComponentIndex(coord.Item1, coord.Item2 + 1))
            {
                connectedComponent[coord.Item1, coord.Item2 + 1] = true;
                q.Enqueue((coord.Item1, coord.Item2 + 1));
            }
            if (ShapeIndex(coord.Item1 - 1, coord.Item2) && !ConnectedComponentIndex(coord.Item1 - 1, coord.Item2))
            {
                connectedComponent[coord.Item1 - 1, coord.Item2] = true;
                q.Enqueue((coord.Item1 - 1, coord.Item2));
            }
            if (ShapeIndex(coord.Item1, coord.Item2 - 1) && !ConnectedComponentIndex(coord.Item1, coord.Item2 - 1))
            {
                connectedComponent[coord.Item1, coord.Item2 - 1] = true;
                q.Enqueue((coord.Item1, coord.Item2 - 1));
            }
        }
        
        //Calculates whether the connected component is the whole shape
        bool result = true;
        
        for (int i = starting.Item1; i < shape.GetLength(0); i++)
        {
            for (int j = starting.Item2; j < shape.GetLength(1); j++)
            {
                result &= connectedComponent[i, j] == shape[i, j];
            }
        }
        
        return result;
    }
}
