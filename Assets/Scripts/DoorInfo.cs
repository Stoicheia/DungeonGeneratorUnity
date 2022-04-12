using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorType
{
    Normal, None
}

public enum Orientation
{
    Horizontal, Vertical
}

public struct TilePair
{
    public TileInfo First;
    public TileInfo Second;
    
    public TilePair(TileInfo one, TileInfo two)
    {
        First = one;
        Second = two;
    }
}

public struct RoomPair
{
    public Room First;
    public Room Second;

    public RoomPair(TilePair tiles)
    {
        First = tiles.First.Room;
        Second = tiles.Second.Room;
    }
}

/// <summary>
/// Data pertaining to doors which have been placed in the tile grid.
/// </summary>
public struct DoorInfo
{
    private DoorType _type;
    private TilePair _tiles;

    public Orientation Orientation;
    public TilePair Tiles => _tiles;
    public RoomPair Rooms => new RoomPair(Tiles);
    public DoorInfo(DoorType type, TileInfo firstTile, TileInfo secondTile)
    {
        _type = type;
        _tiles = new TilePair(firstTile, secondTile);
        Orientation = firstTile.Coords.x == secondTile.Coords.x ? Orientation.Vertical : Orientation.Horizontal;
    }

    public static DoorInfo Empty()
    {
        return new DoorInfo(DoorType.None, TileInfo.Empty(), TileInfo.Empty());
    }
}
