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
    [SerializeField] private List<RoomShape> allowedShapes;

    private void OnValidate()
    {
        allowedShapes.ForEach(x => x.ShapeInit());
        minSize = Math.Min(minSize, maxSize);
    }

    [ContextMenu("Prune Non-Contiguous")]
    public void PruneNonContiguous()
    {
        allowedShapes = allowedShapes.Where(x => x.Contiguous).ToList();
    }
}
