using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using Random = System.Random;

public class Pass
{
    private const int MAX_PLACEMENT_ATTEMPTS = 20;
    private const int PLACEMENT_GUARANTEE_THRESHOLD = 16;
    private const int MAX_QUEUE_TRAVERSAL_FACTOR = 10;
    private const int TRIES_PER_PRIORITY = 4;

    private Vector2Int _startingLocation;
    private int _passIndex;

    private int _tilesPlaced;
    private Queue<(RoomGenerationParameters, RoomShape)> _roomQueue;
    private RoomShapeAsset _parameters;

    private RoomGenerationParameters _toPlaceParams;
    private RoomShape _toPlaceShape;

    /// <summary>
    /// A pass is a container for a set of generation rules. Call DoPass(dungeon) to write to a dungeon based on these rules.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="startingLocation"></param>
    /// <param name="index"></param>
    public Pass(RoomShapeAsset parameters, Vector2Int startingLocation, int index)
    {
        _tilesPlaced = 0;
        _parameters = parameters;
        _startingLocation = startingLocation;
        _passIndex = index;
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
            
            var priorityList = rules.GetNeighboursByPriority();
            int priorityIndex = 0;
            int priorityModTracker = 0;
            List<int> allowedNeighbours = priorityList[0];

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

                if (unexploredRooms.Count == 0) //refresh unexplored room list if we run out
                {
                    placementAttempts++;
                    unexploredRooms = new Queue<Room>(allRooms.OrderBy(x => UnityEngine.Random.Range(0f, 1f)));
                    if (++priorityModTracker % TRIES_PER_PRIORITY == 0)
                    {
                        allowedNeighbours = priorityList[(++priorityIndex % priorityList.Count)];
                        priorityModTracker = 0;
                    }
                }

                Room currentRoom = unexploredRooms.Dequeue();
                var boundaryInfo = dungeon.GetBoundaryTilesAndRooms(currentRoom, false);
                List<(Vector2Int, Facing)> possiblePlacements = boundaryInfo.Item1
                    .OrderBy(x => UnityEngine.Random.Range(0f, 1f)).ToList();

                foreach (var p in possiblePlacements)
                {
                    var (coord, facing) = p;
                    int[] anchor = shape.GetRandomOnBoundaryReverse(facing);
                    var neighbourInfo = dungeon.CountNeighbours(coord, _toPlaceShape, anchor);
                    var neighbours = neighbourInfo.Item1;

                    List<Room> adjacentRooms = neighbourInfo.Item2;
                    
                    if (neighbours < 0 || !allowedNeighbours.Contains(neighbours)) //invalid placement or not looking here yet
                    {
                        continue;
                    }

                    var connectionViolation = false;
                    foreach (var r in adjacentRooms)
                    {
                        if (r.CurrentConnections + adjacentRooms.Count(x => x == r) > r.Connections)
                        {
                            connectionViolation = true;
                        }
                    }

                    if (connectionViolation && !rules.IgnoreConnections) 
                    {
                        continue;
                    }

                    float distance = (_startingLocation - coord).magnitude;
                    
                    float random = UnityEngine.Random.Range(0f, 1f);
                    float probability = _passIndex == 0 ? rules.GetProbability(neighbours) : rules.GetProbability(neighbours, distance);
                    if (placementAttempts >= PLACEMENT_GUARANTEE_THRESHOLD) probability = 1;
                    if (priorityModTracker % TRIES_PER_PRIORITY == TRIES_PER_PRIORITY - 1 && priorityList.Count > 1) probability = 1;
                    if(_passIndex == 0 
                       &&!(FloatComparer.AreEqual(rules.DistanceModZero, 0, 0.1f) 
                       && FloatComparer.AreEqual(rules.DistanceModOne, 0, 0.1f)))
                    {
                        Debug.LogWarning("You cannot have distance rules for the first pass! These will be ignored.");
                    }

                    if (random >= probability)
                    {
                        continue;
                    }

                    Room placed = dungeon.PlaceRoom(coord, shape, anchor, RoomType.Normal, _toPlaceParams.Connections);
                    if (placed != null)
                    {
                        placementSuccessful = true;
                        unexploredRooms.Enqueue(placed);
                        allRooms.Add(placed);

                        if (rules.IgnoreConnections) break;
                        
                        placed.CurrentConnections += neighbours;
                        foreach (var r in adjacentRooms)
                        {
                            r.CurrentConnections++;
                        }
                        break;
                    }
                }
            }

        }
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
