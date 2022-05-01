using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Tile lattice partitioned into rooms.
/// </summary>
public class TileGrid
{
    struct CoordinatePair
    {
        public CoordinatePair((int,int) v, (int, int) u)
        {
            first = new Vector2Int(v.Item1, v.Item2);
            second = new Vector2Int(u.Item1, v.Item2);
        }
        public Vector2Int first;
        public Vector2Int second;
    }
    
    private const int GRID_SIZE = 128;
    
    private TileInfo[,] _tiles = new TileInfo[GRID_SIZE, GRID_SIZE];
    private Dictionary<CoordinatePair, DoorInfo> _doors;
    private List<Room> _rooms = new List<Room>(GRID_SIZE);

    public void Init()
    {
        _rooms = new List<Room>(GRID_SIZE);
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                _tiles[i, j] = TileInfo.Empty();
                if(j + 1 < GRID_SIZE) _doors[new CoordinatePair((i, j),(i, j + 1))] = DoorInfo.Empty();
                if(i + 1 < GRID_SIZE) _doors[new CoordinatePair((i, j),(i + 1, j))] = DoorInfo.Empty();
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
        var key = new CoordinatePair((first.x, first.y), (second.x, second.y));
        if (!_doors.ContainsKey(key)) return DoorInfo.Empty();
        return _doors[key];
    }

    public bool IsOccupied(Vector2Int coords)
    {
        return GetTile(coords).Active;
    }

    public bool PlaceRoom(Vector2Int at, RoomShape roomShape, int[] anchorArr, RoomType type)
    {
        bool[,] shape = roomShape.Shape;
        Vector2Int anchor = new Vector2Int(anchorArr[0], anchorArr[1]);
        if (!shape[anchor.x, anchor.y]) return false;

        //Check validity of room placement
        for (int i = 0; i < shape.GetLength(0); i++)
        {
            for (int j = 0; j < shape.GetLength(1); j++)
            {
                if (!shape[i, j]) continue;
                Vector2Int localCoord = new Vector2Int(i, j) - anchor;
                Vector2Int gridCoord = at + localCoord;
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
                Vector2Int gridCoord = at + localCoord;
                room.AddTile(gridCoord);
                _tiles[i, j] = new TileInfo(room, gridCoord);
            }
        }

        return true;

        /*//Generate doors
        Dictionary<Facing, List<int[]> > roomFacings = new Dictionary<Facing, List<int[]>>()
        {
            {Facing.Left, roomShape.GetAllOnBoundary(Facing.Left)},
            {Facing.Right, roomShape.GetAllOnBoundary(Facing.Right)},
            {Facing.Up, roomShape.GetAllOnBoundary(Facing.Up)},
            {Facing.Down, roomShape.GetAllOnBoundary(Facing.Down)}
        };

        foreach (var f in roomFacings.Keys)
        {
            foreach (var coord in roomFacings[f])
            {
                Vector2Int localCoord = new Vector2Int(coord[0], coord[1]) - anchor;
                Vector2Int gridCoord = placeAt + localCoord;
                (Vector2Int, Orientation) doorInfo = TileDoorCoordTransform(gridCoord, f);
                if (doorInfo.Item2 == Orientation.Horizontal)
                {
                    List<TileInfo> tilesBetween =
                        DoorTileCoordTransform(doorInfo.Item1, true).ToList().Select(x => GetTile(x)).ToList();
                    if (!tilesBetween[0].Active || !tilesBetween[1].Active) continue;
                    RoomType[] typesBetween = tilesBetween
                        .Select(x => x.Room.Type).ToArray();
                    RoomType priorityType = typesBetween[0] > typesBetween[1] ? typesBetween[0] : typesBetween[1];
                    _horizontalDoors[doorInfo.Item1.x, doorInfo.Item1.y] 
                        = new DoorInfo(priorityType, tilesBetween[0], tilesBetween[1]);
                }
                else
                {
                    List<TileInfo> tilesBetween =
                        DoorTileCoordTransform(doorInfo.Item1, false).ToList().Select(x => GetTile(x)).ToList();
                    if (!tilesBetween[0].Active || !tilesBetween[1].Active) continue;
                    RoomType[] typesBetween = tilesBetween
                        .Select(x => x.Room.Type).ToArray();
                    RoomType priorityType = typesBetween[0] > typesBetween[1] ? typesBetween[0] : typesBetween[1];
                    _verticalDoors[doorInfo.Item1.x, doorInfo.Item1.y]
                        = new DoorInfo(priorityType, tilesBetween[0], tilesBetween[1]);
                }
            }
        }
        return true;*/
    }

    public void GenerateDoors()
    {
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (j + 1 < GRID_SIZE)
                {
                    var firstRoom = _tiles[i, j].Room;
                    var secondRoom = _tiles[i, j + 1].Room;
                    if (!firstRoom.Equals(secondRoom))
                    {
                        _doors[new CoordinatePair((i, j), (i, j + 1))] = new DoorInfo(
                            Room.MaxType(firstRoom, secondRoom), _tiles[i, j], _tiles[i, j + 1]
                            );
                    }
                }

                if (i + 1 < GRID_SIZE)
                {
                    var firstRoom = _tiles[i, j].Room;
                    var secondRoom = _tiles[i + 1, j].Room;
                    if (!firstRoom.Equals(secondRoom))
                    {
                        _doors[new CoordinatePair((i, j), (i + 1, j))] = new DoorInfo(
                            Room.MaxType(firstRoom, secondRoom), _tiles[i, j], _tiles[i + 1, j]
                        );
                    }
                }
            }
        }
    }

}
