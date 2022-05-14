using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pass
{
    private int _tilesPlaced;
    private Queue<(RoomGenerationParameters, RoomShape)> _roomQueue;
    private RoomShapeAsset _parameters;

    private RoomGenerationParameters _currentRoomParameters;
    private RoomShape _currentRoomShape;

    /// <summary>
    /// A pass is a container for a set of generation rules. Call DoPass(dungeon) to write to a dungeon based on these rules.
    /// </summary>
    /// <param name="parameters"></param>
    public Pass(RoomShapeAsset parameters)
    {
        _tilesPlaced = 0;
        _parameters = parameters;
    }

    /// <summary>
    /// The core dungeon generation algorithm. Places rooms on a TileGrid based on given rules.
    /// </summary>
    /// <param name="dungeon"></param>
    public void DoPass(TileGrid dungeon)
    {
        Queue<Room> unexploredRooms = new Queue<Room>();
        GenerateRoomQueue(_parameters, out _roomQueue);
        DungeonGenerator.Record("Generated Room Queue");
        foreach (var room in dungeon.Rooms)
        {
            Debug.Log(room);
            unexploredRooms.Enqueue(room);
        }

        (_currentRoomParameters, _currentRoomShape) = _roomQueue.Dequeue();
        while (unexploredRooms.Count > 0 && _roomQueue.Count > 0)
        {
            Room currentRoom = unexploredRooms.Dequeue();
            List<Vector2Int> possiblePlacements = dungeon.GetBoundaryRooms(currentRoom);
        }
    }

    private void GenerateRoomQueue(RoomShapeAsset parameters, out Queue<(RoomGenerationParameters, RoomShape)> queue)
    {
        int sizeFloor = UnityEngine.Random.Range(parameters.MinSize, parameters.MaxSize);
        int currentSize = 0;
        List<(RoomGenerationParameters, RoomShape)> unsortedQueue = new List<(RoomGenerationParameters, RoomShape)>();
        Dictionary<RoomShape, int> roomCount = new Dictionary<RoomShape, int>();

        var validRooms = parameters.GetValid();
        
        foreach (var room in validRooms)
        {
            roomCount.Add(room.Value, 0);
        }

        foreach (var room in validRooms)
        {
            for (int i = roomCount[room.Value]; i < room.Key.MinCount; i++)
            {
                unsortedQueue.Add((room.Key, room.Value));
                roomCount[room.Value]++;
                foreach (var t in room.Key.SubordinateRoomIndices)
                {
                    if (t < 0 || t >= parameters.RoomsWithShapes.Count) continue;
                    roomCount[parameters.RoomsWithShapes[t].Item2]++;
                }
                currentSize += room.Value.Size;
            }
        }

        int tries = 0;
        while (currentSize < sizeFloor)
        {
            var toAdd = PickRandomRoom(parameters);
            int maxCount = toAdd.Item1.TrueMax;
            foreach (var t in toAdd.Item1.SubordinateRoomIndices)
            {
                if (t < 0 || t >= parameters.Rooms.Count) continue;
                maxCount = Math.Min(maxCount, parameters.Rooms[t].TrueMax);
            }
            if (roomCount[toAdd.Item2] > maxCount)
            {
                if (++tries > 35481) //magic number
                    throw new ArgumentException("Max numbers of room shapes are likely inconsistent with the desired room count.");
                continue;
            }

            unsortedQueue.Add(toAdd);
            currentSize += toAdd.Item2.Size;
            roomCount[toAdd.Item2]++;
            
            foreach (var t in toAdd.Item1.SubordinateRoomIndices)
            {
                if (t < 0 || t >= parameters.RoomsWithShapes.Count) continue;
                roomCount[parameters.RoomsWithShapes[t].Item2]++;
            }
        }
        
        queue = new Queue<(RoomGenerationParameters, RoomShape)>(unsortedQueue.OrderBy(x => UnityEngine.Random.Range(0f, 1f)).ToList());
    }

    private (RoomGenerationParameters, RoomShape) PickRandomRoom(RoomShapeAsset parameters)
    {
        var validRooms = parameters.GetValid();
        return WeightedRandom<(RoomGenerationParameters, RoomShape)>
            .Choose(validRooms.Select(x => (x.Key, x.Value)).ToList(),
            parameters.Rooms.Select(x => x.Weight).ToList());
    }
}
