using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Generation Group")]
public class GenerationGroup : ScriptableObject
{
    [SerializeField] private RoomShape startingRoom;
    [SerializeField] private List<RoomShapeAsset> passes;

    public RoomShape StartingRoom => startingRoom;
    public List<RoomShapeAsset> Passes => passes;
}
