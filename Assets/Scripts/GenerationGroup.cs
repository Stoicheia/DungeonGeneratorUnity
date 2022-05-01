using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Generation Group")]
public class GenerationGroup : ScriptableObject
{
    [SerializeField] private List<RoomShapeAsset> passes;
}
