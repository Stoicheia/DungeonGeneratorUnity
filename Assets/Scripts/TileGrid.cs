using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Tile lattice partitioned into rooms.
/// </summary>
public class TileGrid
{
    private const int GRID_SIZE = 128;
    
    private TileInfo[,] _tiles = new TileInfo[GRID_SIZE, GRID_SIZE];
    private DoorInfo[,] _verticalDoors = new DoorInfo[GRID_SIZE, GRID_SIZE - 1];
    private DoorInfo[,] _horizontalDoors = new DoorInfo[GRID_SIZE - 1, GRID_SIZE];
    private List<Room> _rooms = new List<Room>(GRID_SIZE);

    public void Init()
    {
        _rooms = new List<Room>(GRID_SIZE);
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                _tiles[i, j] = TileInfo.Empty();
                if(j + 1 < GRID_SIZE) _verticalDoors[i, j] = DoorInfo.Empty();
                if(i + 1 < GRID_SIZE) _horizontalDoors[i, j] = DoorInfo.Empty();
            }
        }
    }

    public TileInfo GetTile(Vector2Int coords)
    {
        try
        {
            return _tiles[coords.x, coords.y];
        }
        catch
        {
            return TileInfo.Empty(coords);
        }
    }

    public DoorInfo GetDoorFromTiles(TileInfo first, TileInfo second)
    {
        return GetDoorFromTiles(first.Coords, second.Coords);
    }

    public DoorInfo GetDoorFromTiles(Vector2Int first, Vector2Int second)
    {
        Vector2Int difference = second - first;
        try
        {
            return (difference.x, difference.y) switch
            {   
                (1, 0) => _horizontalDoors[first.x, first.y],
                (-1, 0) => _horizontalDoors[second.x, second.y],
                (0, 1) => _verticalDoors[first.x, first.y],
                (0, -1) => _verticalDoors[second.x, second.y],
                _ => DoorInfo.Empty()
            };
        }
        catch
        {
            return DoorInfo.Empty();
        }
    }

    public bool IsOccupied(Vector2Int coords)
    {
        return GetTile(coords).Active;
    }

    private static Vector2Int[] DoorTileCoordTransform(Vector2Int door, bool horizontal)
    {
        if (horizontal)
        {
            return new[] {door, door + Vector2Int.right };
        }
        return new[] {door + Vector2Int.up, door};
    }
    
    private bool PlaceRoom(Vector2Int from, Vector2Int door, Orientation doorOrientation, bool[,] shape, int[] anchorArr, RoomType type)
    {
        Vector2Int anchor = new Vector2Int(anchorArr[0], anchorArr[1]);
        if (!shape[anchor.x, anchor.y]) return false;
        
        Vector2Int[] between = DoorTileCoordTransform(door, doorOrientation == Orientation.Horizontal);
        Vector2Int placeAt = from == between[0] ? between[1] : between[0];
        Vector2Int difference = placeAt - from;
        Facing facing = (difference.x, difference.y) switch
        {
            (1, 0) => Facing.Right,
            (-1, 0) => Facing.Left,
            (0, 1) => Facing.Up,
            (0, -1) => Facing.Down,
            _ => throw new ArgumentOutOfRangeException()
        };

        //Check validity of room placement
        for (int i = 0; i < shape.GetLength(0); i++)
        {
            for (int j = 0; j < shape.GetLength(1); j++)
            {
                if (!shape[i, j]) continue;
                Vector2Int localCoord = new Vector2Int(i, j) - anchor;
                Vector2Int gridCoord = placeAt + localCoord;
                if (IsOccupied(gridCoord)) return false;
            }
        }
        
        //Overwrite existing data with new room placement
        Room room = new Room(type);
        
        for (int i = 0; i < shape.GetLength(0); i++)
        {
            for (int j = 0; j < shape.GetLength(1); j++)
            {
                if (!shape[i, j]) continue;
                Vector2Int localCoord = new Vector2Int(i, j) - anchor;
                Vector2Int gridCoord = placeAt + localCoord;
                room.AddTile(gridCoord);
                _tiles[i, j] = new TileInfo(room, gridCoord);
            }
        }

        return true;
    }

}
