using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum RoomType
{
    Normal = 1, Start = 2, End = 100, None = -1
}

/// <summary>
/// Data pertaining to rooms which have been placed in the tile grid.
/// </summary>
public class Room
{
    private RoomType _type;
    private List<Vector2Int> _tiles;
    
    public RoomType Type => _type;
    public List<Vector2Int> TileCoords => _tiles;


    public Room(RoomType type = RoomType.None)
    {
        _type = type;
        _tiles = new List<Vector2Int>();
    }
    public Room(RoomType type, List<Vector2Int> extent)
    {
        _type = type;
        _tiles = extent;
    }

    public void AddTile(Vector2Int tile)
    {
        _tiles.Add(tile);
    }

    public static Room Empty()
    {
        return new Room();
    }

    public static RoomType MaxType(RoomType a, RoomType b)
    {
        return a >= b ? a : b;
    }
    
    public static RoomType MaxType(Room a, Room b)
    {
        return MaxType(a.Type, b.Type);
    }
}
