using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private GenerationGroup Instructions;
    [SerializeField] private bool LogExecutionReport;

    private Room _startingRoom;

    private RoomShape _starting;
    private List<RoomShapeAsset> _passes;
    private static List<(string, float)> _timing;
    private int _currentPassNumber = -1;

    public TileGrid Dungeon;

    public int EffectivePassNumber => Math.Max(_currentPassNumber, 0);

    public GenerationGroup INSTRUCTIONS
    {
        get
        {
            return Instructions;
        }
        set
        {
            Instructions = value;
            Init();
        }
    }

    private void OnEnable()
    {
        _currentPassNumber = -1;
    }

    private void OnValidate()
    {
        INSTRUCTIONS = Instructions;
    }


    /// <summary>
    /// A dungeon generation process consists of multiple passes. Each pass has independent rules and parameters.
    /// </summary>
    public TileGrid Generate()
    {
        ProcessPrePass();
        for(int i = 0; i <_passes.Count; i++)
        {
            ProcessPassIndex(i);
        }
        _timing.Add(("End", Time.realtimeSinceStartup));
        Log();
        return Dungeon;
    }
    
    /// <summary>
    /// A dungeon generation process consists of multiple passes. Each pass has independent rules and parameters.
    /// </summary>
    public TileGrid DoOnePass()
    {
        if (_currentPassNumber == -1) 
        { 
            ProcessPrePass(); 
            return Dungeon;
        }
        if (_currentPassNumber >= _passes.Count)
        {
            Debug.LogWarning("No more passes left!"); 
            return Dungeon;
        }
        ProcessPassIndex(_currentPassNumber);
        Log();
        return Dungeon;
    }

    private void ProcessPrePass()
    {
        _timing = new List<(string, float)>();
        _timing.Add(("Start", Time.realtimeSinceStartup));
        Init();
        _timing.Add(("Initialised", Time.realtimeSinceStartup));
        _startingRoom = PlaceStartingRoom();
        _currentPassNumber++;  
    }

    private void ProcessPassIndex(int i)
    {
        Pass p = new Pass(_passes[i], _startingRoom.TileCoords[0], i);
        p.DoPass(Dungeon);
        _currentPassNumber++;
        _timing.Add(($"Pass {i} Finished", Time.realtimeSinceStartup));
    }

    private void Init()
    {
        _passes = Instructions.Passes;
        _starting = Instructions.StartingRoom;
        _currentPassNumber = -1; 
        Dungeon = new TileGrid();
    }

    public void ResetPass()
    {
        _currentPassNumber = -1;
    }

    private Room PlaceStartingRoom()
    {
        return Dungeon.PlaceRoom(Dungeon.Middle, _starting, RoomType.Start);
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
