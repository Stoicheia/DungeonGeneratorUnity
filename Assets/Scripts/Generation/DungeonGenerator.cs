using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private GenerationGroup Instructions;

    private RoomShape _starting;
    private List<RoomShapeAsset> _passes;
    private float _startTime;
    private float _endTime;
    public float TimeTaken => _endTime - _startTime;

    public TileGrid Dungeon;
    
    
    /// <summary>
    /// A dungeon generation process consists of multiple passes. Each pass has independent rules and parameters.
    /// </summary>
    public void Generate()
    {
        _startTime = Time.time;
        Init();
        PlaceStartingRoom();
        foreach (var pass in _passes)
        {
            Pass p = new Pass(pass);
            p.DoPass(Dungeon);
        }
        _endTime = Time.time;
    }

    private void Init()
    {
        _passes = Instructions.Passes;
        _starting = Instructions.StartingRoom;
        Dungeon.Init();
    }

    private void PlaceStartingRoom()
    {
        Dungeon.PlaceRoom(Dungeon.Middle, _starting, RoomType.Start);
    }
}
