using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "New Pass")]
public class RoomShapeAsset : ScriptableObject
{
    [SerializeField][Range(1,256)] private int maxSize;
    [SerializeField][Range(1,256)] private int minSize;
    [SerializeField] private GenerationRuleset neighbourRuleset; 
    [SerializeField] private List<RoomGenerationParameters> rooms;
    private Dictionary<RoomGenerationParameters, RoomShape> allowedShapes;

    public int MaxSize => maxSize;

    public int MinSize => minSize;

    public GenerationRuleset NeighbourRuleset => neighbourRuleset;

    public List<RoomGenerationParameters> Rooms => rooms;

    public List<(RoomGenerationParameters, RoomShape)> RoomsWithShapes
    {
        get
        {
            List<RoomGenerationParameters> keys = allowedShapes.Keys.ToList();
            List<RoomShape> values = allowedShapes.Values.ToList();
            List<(RoomGenerationParameters, RoomShape)> res = new List<(RoomGenerationParameters, RoomShape)>();
            for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
            {
                res.Add((keys[i], values[i]));
            }

            return res;
        }
    }

    public RoomShape GetRoom(RoomGenerationParameters param) => allowedShapes[param];

    private void OnValidate()
    {
        allowedShapes = new Dictionary<RoomGenerationParameters, RoomShape>();
        foreach (var p in rooms)
        {
            allowedShapes[p] = new RoomShape(p.Size, p.SquashedShape);
        }
        minSize = Math.Min(minSize, maxSize);
    }
    public Dictionary<RoomGenerationParameters, RoomShape> GetValid()
    {
        Dictionary<RoomGenerationParameters, RoomShape> validShapes = new Dictionary<RoomGenerationParameters, RoomShape>();
        foreach (var s in allowedShapes)
        {
            if(s.Value.Contiguous) validShapes.Add(s.Key, s.Value);
        }

        return validShapes;
    }

    public GenerationRuleset GetNeighboursRuleset(RoomGenerationParameters ofParameters)
    {
        if (ofParameters.OverrideGenerationRules) return ofParameters.NeighboursRuleset;
        return neighbourRuleset;
    }
}
