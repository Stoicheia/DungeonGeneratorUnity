using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pass
{
    private int _tilesPlaced;
    private List<RoomShape> _roomQueue;
    private RoomShapeAsset _parameters;
    
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
        foreach(var room in dungeon.Rooms)
            unexploredRooms.Enqueue(room);
        while (unexploredRooms.Count > 0)
        {
            Room currentRoom = unexploredRooms.Dequeue();
            List<Vector2Int> possiblePlacements = dungeon.GetBoundaryRooms(currentRoom);
        }
    }

    private void GenerateRoomQueue(RoomShapeAsset parameters, out List<RoomShape> queue)
    {
        int sizeFloor = UnityEngine.Random.Range(parameters.MinSize, parameters.MaxSize);
        int currentSize = 0;
        List<RoomShape> unsortedQueue = new List<RoomShape>();
        Dictionary<RoomShape, int> roomCount = new Dictionary<RoomShape, int>();

        var validRooms = parameters.GetValid();

        foreach (var room in validRooms)
        {
            roomCount.Add(room.Value, 0);
            for (int i = 0; i < room.Key.MinCount; i++)
            {
                unsortedQueue.Add(room.Value);
                roomCount[room.Value]++;
            }

            currentSize += room.Value.Size;
        }

        int tries = 0;
        while (currentSize < sizeFloor)
        {
            var toAdd = PickRandomRoom(parameters);
            if (roomCount[toAdd.Item2] > toAdd.Item1.MaxCount)
            {
                if (++tries > 1000000)
                    throw new ArgumentException("Max numbers of room shapes are likely inconsistent with the desired room count.");
                continue;
            }

            unsortedQueue.Add(toAdd.Item2);
            currentSize += toAdd.Item2.Size;
            roomCount[toAdd.Item2]++;
        }

        queue = unsortedQueue.OrderBy(x => UnityEngine.Random.Range(0f, 1f)).ToList();
    }

    private (RoomGenerationParameters, RoomShape) PickRandomRoom(RoomShapeAsset parameters)
    {
        var validRooms = parameters.GetValid();
        return WeightedRandom<(RoomGenerationParameters, RoomShape)>
            .Choose(validRooms.Select(x => (x.Key, x.Value)).ToList(),
            parameters.Rooms.Select(x => x.Weight).ToList());
    }
}
