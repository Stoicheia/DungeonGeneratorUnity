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
    public struct CoordinatePair
    {
        public CoordinatePair((int,int) v, (int, int) u)
        {
            first = new Vector2Int(v.Item1, v.Item2);
            second = new Vector2Int(u.Item1, u.Item2);
        }
        public Vector2Int first;
        public Vector2Int second;

        public (Vector2Int, Vector2Int) Params => (first, second);
    }
    
    public const int GRID_SIZE = 63;
    public (int, int) Middle => (GRID_SIZE / 2, GRID_SIZE / 2);
    
    private TileInfo[,] _tiles = new TileInfo[GRID_SIZE, GRID_SIZE];
    private Dictionary<CoordinatePair, DoorInfo> _doors;
    private List<Room> _rooms = new List<Room>(GRID_SIZE);

    private int _activeTileCount = 0;

    public TileInfo[,] Tiles => _tiles;
    public Dictionary<CoordinatePair, DoorInfo> Doors => _doors;
    public List<Room> Rooms => _rooms;
    public int ActiveTileCount => _activeTileCount;

    public TileGrid()
    {
        Init();
    }

    public void Init()
    {
        _rooms = new List<Room>(GRID_SIZE);
        _doors = new Dictionary<CoordinatePair, DoorInfo>();
        _tiles = new TileInfo[GRID_SIZE, GRID_SIZE];
        _activeTileCount = 0;
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                _tiles[i, j] = TileInfo.Empty();
                /*if (j + 1 < GRID_SIZE)
                {
                    _doors[new CoordinatePair((i, j),(i, j + 1))] = DoorInfo.Empty();
                    _doors[new CoordinatePair((i, j + 1),(i, j))] = DoorInfo.Empty();
                }

                if (i + 1 < GRID_SIZE)
                {
                    _doors[new CoordinatePair((i, j),(i + 1, j))] = DoorInfo.Empty();
                    _doors[new CoordinatePair((i + 1, j),(i, j))] = DoorInfo.Empty();
                }*/
            }
        }
    }

    private TileInfo GetTile(Vector2Int coords)
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

    private DoorInfo GetDoorFromTiles(Vector2Int first, Vector2Int second)
    {
        var key = new CoordinatePair((first.x, first.y), (second.x, second.y));
        if (!_doors.ContainsKey(key)) return DoorInfo.Empty();
        return _doors[key];
    }

    private bool IsOccupied(Vector2Int coords)
    {
        return GetTile(coords).Active;
    }

    /// <summary>
    /// Get valid placements adjacent to a particular room.
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public List<(Vector2Int, Facing)> GetBoundaryRooms(Room room, bool prospective) => GetBoundaryRooms(room.TileCoords, prospective); 

    public List<(Vector2Int, Facing)> GetBoundaryRooms(List<Vector2Int> tiles, bool prospective)
    {
        HashSet<(Vector2Int, Facing)> result = new HashSet<(Vector2Int, Facing)>();

        foreach (var coord in tiles)
        {
            if(!tiles.Contains(coord + Vector2Int.down))
                result.Add((coord + Vector2Int.down, Facing.Down));
            if(!tiles.Contains(coord + Vector2Int.up))
                result.Add((coord + Vector2Int.up, Facing.Up));
            if(!tiles.Contains(coord + Vector2Int.left))
                result.Add((coord + Vector2Int.left, Facing.Left));
            if(!tiles.Contains(coord + Vector2Int.right))
                result.Add((coord + Vector2Int.right, Facing.Right));
        }
        
        List<(Vector2Int, Facing)> filteredResult = new List<(Vector2Int, Facing)>();

        foreach (var v in result)
        {
            var coord = v.Item1;
            if (coord.x < 0 || coord.x >= GRID_SIZE || coord.y < 0 || coord.y > GRID_SIZE) continue;
            if(IsOccupied(coord) && !prospective) continue;

            filteredResult.Add((coord, v.Item2));
        }

        return filteredResult;
    }

    /// <summary>
    /// How many active tiles will be adjacent to a prospective placement.
    /// </summary>
    /// <param name="at"></param>
    /// <param name="roomShape"></param>
    /// <param name="anchorArr"></param>
    /// <returns></returns>
    public int CountNeighbours(Vector2Int at, RoomShape roomShape, int[] anchorArr)
    {
        HashSet<Vector2Int> neighbours = new HashSet<Vector2Int>();
        bool[,] shape = roomShape.Shape;
        Vector2Int anchor = new Vector2Int(anchorArr[0], anchorArr[1]);
        if (!shape[anchor.x, anchor.y]) return -1;

        List<Vector2Int> gridCoords = new List<Vector2Int>();
        
        for (int i = 0; i < shape.GetLength(0); i++)
        {
            for (int j = 0; j < shape.GetLength(1); j++)
            {
                if (!shape[i, j]) continue;
                Vector2Int localCoord = new Vector2Int(i, j) - anchor;
                Vector2Int gridCoord = at + localCoord;
                gridCoords.Add(gridCoord);
                if (IsOccupied(gridCoord)) return -1;
            }
        }

        return GetBoundaryRooms(gridCoords, true).Count(x => IsOccupied(x.Item1));
    }

    public Room PlaceRoom((int, int) at, RoomShape roomShape, RoomType type)
    {
        List<int[]> validAnchors = roomShape.GetAllActive();
        foreach (var anchor in validAnchors)
        {
            var (item1, item2) = at;
            var room = PlaceRoom(new Vector2Int(item1, item2), roomShape, anchor, type);
            if (room != null)
            {
                return room;
            }
        }

        return null;
    }
    
    /// <summary>
    /// Attempts to place a room on the tile grid. Returns null if placement is invalid.
    /// </summary>
    /// <param name="at"></param>
    /// <param name="roomShape"></param>
    /// <param name="anchorArr"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public Room PlaceRoom(Vector2Int at, RoomShape roomShape, int[] anchorArr, RoomType type)
    {
        bool[,] shape = roomShape.Shape;
        Vector2Int anchor = new Vector2Int(anchorArr[0], anchorArr[1]);
        if (!shape[anchor.x, anchor.y]) return null;

        //Check validity of room placement
        for (int i = 0; i < shape.GetLength(0); i++)
        {
            for (int j = 0; j < shape.GetLength(1); j++)
            {
                if (!shape[i, j]) continue;
                Vector2Int localCoord = new Vector2Int(i, j) - anchor;
                Vector2Int gridCoord = at + localCoord;
                if (gridCoord.x >= GRID_SIZE || gridCoord.x < 0 || gridCoord.y >= GRID_SIZE || gridCoord.y < 0 
                    || IsOccupied(gridCoord)) return null;
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
                _tiles[gridCoord.x, gridCoord.y] = new TileInfo(room, gridCoord);
            }
        }
        
        _rooms.Add(room);
        _activeTileCount += room.TileCount;
        GenerateDoors(room);
        return room;

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

    /// <summary>
    /// Adjacent tiles belonging to different rooms are doors.
    /// </summary>
    private void GenerateDoors()
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
                        var info = new DoorInfo(
                            Room.MaxType(firstRoom, secondRoom), _tiles[i, j], _tiles[i, j + 1]
                        );
                        _doors[new CoordinatePair((i, j), (i, j + 1))] = info;
                        _doors[new CoordinatePair((i, j + 1), (i, j))] = info;
                    }
                }

                if (i + 1 < GRID_SIZE)
                {
                    var firstRoom = _tiles[i, j].Room;
                    var secondRoom = _tiles[i + 1, j].Room;
                    if (!firstRoom.Equals(secondRoom))
                    {
                        var info = new DoorInfo(
                            Room.MaxType(firstRoom, secondRoom), _tiles[i, j], _tiles[i + 1, j]
                        );
                        _doors[new CoordinatePair((i, j), (i + 1, j))] = info;
                        _doors[new CoordinatePair((i + 1, j), (i, j))] = info;
                    }
                }
            }
        }
    }
    
    private void GenerateDoors(Room r)
    {
        foreach (var coord in r.TileCoords)
        {
            var i = coord.x;
            var j = coord.y;
            if (j + 1 < GRID_SIZE)
            {
                var firstRoom = _tiles[i, j].Room;
                var secondRoom = _tiles[i, j + 1].Room;
                if (!firstRoom.Equals(secondRoom))
                {
                    var info = new DoorInfo(
                        Room.MaxType(firstRoom, secondRoom), _tiles[i, j], _tiles[i, j + 1]
                    );
                    _doors[new CoordinatePair((i, j), (i, j + 1))] = info;
                    _doors[new CoordinatePair((i, j + 1), (i, j))] = info;
                }
            }

            if (i + 1 < GRID_SIZE)
            {
                var firstRoom = _tiles[i, j].Room;
                var secondRoom = _tiles[i + 1, j].Room;
                if (!firstRoom.Equals(secondRoom))
                {
                    var info = new DoorInfo(
                        Room.MaxType(firstRoom, secondRoom), _tiles[i, j], _tiles[i + 1, j]
                    );
                    _doors[new CoordinatePair((i, j), (i + 1, j))] = info;
                    _doors[new CoordinatePair((i + 1, j), (i, j))] = info;
                }
            }
            
            if (j - 1 >= 0)
            {
                var firstRoom = _tiles[i, j].Room;
                var secondRoom = _tiles[i, j - 1].Room;
                if (!firstRoom.Equals(secondRoom))
                {
                    var info = new DoorInfo(
                        Room.MaxType(firstRoom, secondRoom), _tiles[i, j], _tiles[i, j - 1]
                    );
                    _doors[new CoordinatePair((i, j), (i, j - 1))] = info;
                    _doors[new CoordinatePair((i, j - 1), (i, j))] = info;
                }
            }

            if (i - 1 >= 0)
            {
                var firstRoom = _tiles[i, j].Room;
                var secondRoom = _tiles[i - 1, j].Room;
                if (!firstRoom.Equals(secondRoom))
                {
                    var info = new DoorInfo(
                        Room.MaxType(firstRoom, secondRoom), _tiles[i, j], _tiles[i - 1, j]
                    );
                    _doors[new CoordinatePair((i, j), (i - 1, j))] = info;
                    _doors[new CoordinatePair((i - 1, j), (i, j))] = info;
                }
            }
        }
    }

    public override string ToString()
    {
        return $"A dungeon with {_rooms.Count.ToString()} rooms.";
    }
}
