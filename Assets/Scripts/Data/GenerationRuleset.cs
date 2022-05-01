using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct RoomGenParams
{
    [SerializeField] int neighbours; 
    [SerializeField] [Range(0,1)] float probability; 
    [SerializeField] int priority;

    public float Weight => Mathf.Clamp01(probability);
    public int Priority => priority;
    public int Neighbours => Math.Max(1, neighbours);

    public RoomGenParams(int n, float w, int p)
    {
        neighbours = n;
        probability = w;
        priority = p;
    }

}

[CreateAssetMenu(menuName = "Neighbour Ruleset")]
public class GenerationRuleset : ScriptableObject
{
    public bool PropagateHigher;
    private int MaxNeighbours;
    [SerializeField][Range(0, 1)] private float UniversalProbabilityModifier;
    [SerializeField] private List<RoomGenParams> neighbourParams;
    private Dictionary<int, RoomGenParams> neighbourParamDict;

    private void OnValidate()
    {
        MaxNeighbours = Math.Max(0, neighbourParams.Count);
        neighbourParamDict = new Dictionary<int, RoomGenParams>();
        List<RoomGenParams> modList = new List<RoomGenParams>(MaxNeighbours);
        for (int i = 1; i <= MaxNeighbours; i++)
        {
            var @params = neighbourParams.Count >= i ? neighbourParams[i - 1] : new RoomGenParams(i, 0, 0) ;
            var newParams = new RoomGenParams(i, @params.Weight, @params.Priority);
            modList.Add(newParams);
            neighbourParamDict[i] = newParams;
        }
    
        modList = modList.OrderBy(x => x.Neighbours).ToList();
        neighbourParams = modList;
    }

    public int MaxRepresentedNeighbours()
    {
        return neighbourParamDict.Keys.Max();
    }

    public float GetProbability(int neighbours)
    {
        int trueNeighbours = PropagateHigher ? Math.Min(neighbours, MaxRepresentedNeighbours()) : neighbours;
        return neighbourParams[neighbours].Weight;
    }
    
    public List<int> GetNeighbourPriority()
    {
        return neighbourParams.OrderByDescending(y => y.Priority).Select(x => x.Neighbours).ToList();
    }
}
