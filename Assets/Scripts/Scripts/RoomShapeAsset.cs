using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "New Room Shapes")]
public class RoomShapeAsset : ScriptableObject
{
    [SerializeField][Range(1,256)] private int maxSize;
    [SerializeField][Range(1,256)] private int minSize;
    [SerializeField] private GenerationRuleset neighbourRuleset; 
    [SerializeField] private List<RoomGenerationParameters> parameters;
    private Dictionary<RoomGenerationParameters, RoomShape> allowedShapes;

    private void OnValidate()
    {
        allowedShapes = new Dictionary<RoomGenerationParameters, RoomShape>();
        foreach (var p in parameters)
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
}
