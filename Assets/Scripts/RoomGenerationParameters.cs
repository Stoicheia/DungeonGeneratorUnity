using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct RoomGenerationParameters
{
    public int Size;
    public List<bool> SquashedShape;

    public float Weight;
    public int MinCount;
    public int MaxCount;
    public bool OverrideGenerationRules;
    public GenerationRuleset NeighboursRuleset;
}
