using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Room Shapes")]
public class RoomShapeAsset : ScriptableObject
{
    [SerializeField] private List<RoomShape> allowedShapes;

    private void OnValidate()
    {
        allowedShapes.ForEach(x => x.ShapeInit());
        allowedShapes.ForEach(x => Debug.Log(x.NumberOfRooms));
    }
}
