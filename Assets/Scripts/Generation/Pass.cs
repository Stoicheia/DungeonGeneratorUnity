using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class Pass
{
    private const int MAX_PLACEMENT_ATTEMPTS = 100;
    private const int MAX_QUEUE_TRAVERSAL_FACTOR = 10;

    private int _tilesPlaced;
    private Queue<(RoomGenerationParameters, RoomShape)> _roomQueue;
    private RoomShapeAsset _parameters;

    private RoomGenerationParameters _toPlaceParams;
    private RoomShape _toPlaceShape;

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
        //Initialisation and room queueing
        Queue<Room> unexploredRooms = new Queue<Room>();
        List<Room> allRooms = new List<Room>();
        if (dungeon.Rooms.Count == 0)
        {
            Debug.LogError("ルームのないダンジョンはPassをすることができません。設定でスターティング・ルームをつけておいてください。");
        }
        GenerateRoomQueue(_parameters, out _roomQueue);
        DungeonGenerator.Record("Generated Room Queue");
        foreach (var room in dungeon.Rooms)
        {
            Debug.Log(room);
            unexploredRooms.Enqueue(room);
            allRooms.Add(room);
        }
        
        
        //Core placement algorithm
        int queueTraversalIndex = 0;
        int maxQueueTraversal = _roomQueue.Count * MAX_QUEUE_TRAVERSAL_FACTOR;
        while (_roomQueue.Count > 0)
        {
            queueTraversalIndex++;
            if (queueTraversalIndex >= maxQueueTraversal)
            {
                Debug.LogError($"Dungeon did not finish generating! {_roomQueue.Count} rooms were not placed.");
                break;
            }
            (_toPlaceParams, _toPlaceShape) = _roomQueue.Dequeue();
            GenerationRuleset rules = _parameters.GetNeighboursRuleset(_toPlaceParams);
            bool placementSuccessful = false;
            int placementAttempts = 0;

            RoomShape shape = new RoomShape(_toPlaceShape.RandomOrientation());

            while (!placementSuccessful)
            {
                if (placementAttempts >= MAX_PLACEMENT_ATTEMPTS - 1)
                {
                    Debug.LogWarning($"Failed to place this room of size " +
                                     $"{_toPlaceShape.NumberOfRooms} with probability " +
                                     $"{_toPlaceParams.Weight}. " +
                                     $"Moved to back of the queue. " +
                                     $"If you see no errors, this shouldn't be a problem.");
                    _roomQueue.Enqueue((_toPlaceParams, _toPlaceShape));
                    break;
                }
                
                int belowPriority = Int32.MaxValue;
                
                if (unexploredRooms.Count == 0) //refresh unexplored room list if we run out
                {
                    unexploredRooms = new Queue<Room>(allRooms.OrderBy(x => UnityEngine.Random.Range(0f, 1f)));
                }

                Room currentRoom = unexploredRooms.Dequeue();
                List<(Vector2Int, Facing)> possiblePlacements = dungeon.GetBoundaryRooms(currentRoom, false)
                    .OrderBy(x => UnityEngine.Random.Range(0f, 1f)).ToList();
                List<(Vector2Int, int)> neighbourCount = new List<(Vector2Int, int)>();
                foreach (var p in possiblePlacements)
                {
                    var (coord, facing) = p;
                    int[] anchor = shape.GetRandomOnBoundaryReverse(facing);
                    int neighbours = dungeon.CountNeighbours(coord, _toPlaceShape, anchor);
                    if (neighbours < 0)
                    {
                        placementAttempts++;
                        continue;
                    }
                    float random = UnityEngine.Random.Range(0f, 1f);
                    float probability = rules.GetProbability(neighbours);

                    if (random >= probability)
                    {
                        placementAttempts++;
                        continue;
                    }

                    Room placed = dungeon.PlaceRoom(coord, shape, anchor, RoomType.Normal);
                    if (placed != null)
                    {
                        placementSuccessful = true;
                        unexploredRooms.Enqueue(placed);
                        allRooms.Add(placed);
                        break;
                    }
                    placementAttempts++;
                }
            }

        }
        
        Debug.Log($"{dungeon.Rooms.Count} rooms");
    }

    private void GenerateRoomQueue(RoomShapeAsset parameters, out Queue<(RoomGenerationParameters, RoomShape)> queue)
    {
        int sizeFloor = UnityEngine.Random.Range(parameters.MinSize, parameters.MaxSize);
        int currentSize = 0;
        List<(RoomGenerationParameters, RoomShape)> unsortedQueue = new List<(RoomGenerationParameters, RoomShape)>();
        Dictionary<RoomShape, int> roomCount = new Dictionary<RoomShape, int>();

        var validRooms = parameters.GetValid();
        
        foreach (var room in validRooms.OrderBy(x => UnityEngine.Random.Range(0f, 1f)))
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
                currentSize += room.Value.NumberOfRooms;
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
            if (roomCount[toAdd.Item2] >= maxCount)
            {
                if (++tries > 35481) //magic number
                    throw new ArgumentException("Max numbers of room shapes are likely inconsistent with the desired room count.");
                continue;
            }

            unsortedQueue.Add(toAdd);
            currentSize += toAdd.Item2.NumberOfRooms;
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
