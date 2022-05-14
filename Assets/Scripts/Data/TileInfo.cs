using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

/// <summary>
/// Data pertaining to a tile which has been placed in the tile grid.
/// </summary>
public struct TileInfo
{
    private Room _room;
    private Vector2Int _coords;
    public bool Active => _room != null && _room.Type != RoomType.None;

    public Room Room => _room;
    public Vector2Int Coords => _coords;

    public TileInfo(Room room, Vector2Int coords)
    {
        _room = room;
        _coords = coords;
    }

    public static TileInfo Empty(Vector2Int coords)
    {
        return new TileInfo(null, coords);
    }
    
    public static TileInfo Empty(int a = -1, int b = -1)
    {
        return new TileInfo(Room.Empty(), new Vector2Int(a, b));
    }
}
