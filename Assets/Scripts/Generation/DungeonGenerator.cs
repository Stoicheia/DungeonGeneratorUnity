using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private GenerationGroup Instructions;
    [SerializeField] private bool LogExecutionReport;

    private RoomShape _starting;
    private List<RoomShapeAsset> _passes;
    private static List<(string, float)> _timing;

    public TileGrid Dungeon;
    
    
    /// <summary>
    /// A dungeon generation process consists of multiple passes. Each pass has independent rules and parameters.
    /// </summary>
    public TileGrid Generate()
    {
        _timing = new List<(string, float)>();
        _timing.Add(("Start", Time.realtimeSinceStartup));
        Init();
        _timing.Add(("Initialised", Time.realtimeSinceStartup));
        PlaceStartingRoom();
        for(int i = 0; i <_passes.Count; i++)
        {
            Pass p = new Pass(_passes[i]);
            p.DoPass(Dungeon);
            _timing.Add(($"Pass {i} Finished", Time.realtimeSinceStartup));
        }
        _timing.Add(("End", Time.realtimeSinceStartup));
        Log();
        return Dungeon;
    }

    private void Init()
    {
        _passes = Instructions.Passes;
        _starting = Instructions.StartingRoom;
        Dungeon = new TileGrid();
    }

    private void PlaceStartingRoom()
    {
        Dungeon.PlaceRoom(Dungeon.Middle, _starting, RoomType.Start);
    }

    public static void Record(string info)
    {
        _timing.Add((info, Time.realtimeSinceStartup));
    }

    private void Log()
    {
        if (!LogExecutionReport) return;
        for (int i = 1; i < _timing.Count - 1; i++)
        {
            var entry = _timing[i];
            var lastEntry = _timing[i - 1];
            Debug.Log($"{entry.Item1} ({entry.Item2 - lastEntry.Item2} seconds since last)");
        }
        Debug.Log($"Total execution time: {_timing.Last().Item2 - _timing.First().Item2} seconds");
    }
}
