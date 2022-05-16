using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

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
    public bool IgnoreConnections;
    private int MaxNeighbours;
    [SerializeField][Range(0, 1)] private float UniversalProbabilityModifier;
    [Tooltip("Probability multiplied by 0 at this distance from start. Set both mods to 0 to ignore.")]
    [SerializeField] [Range(0, 50)] private float _distanceModZero;
    [Tooltip("Probability multiplied by 0 at this distance from start. Set both mods to 0 to ignore.")]
    [SerializeField] [Range(0, 50)] private float _distanceModOne;
    [SerializeField] private List<RoomGenParams> _neighbourParams;
    private Dictionary<int, RoomGenParams> _neighbourParamDict;

    public float DistanceModZero => _distanceModZero;
    public float DistanceModOne => _distanceModOne;
    
    private void OnValidate()
    {
        MaxNeighbours = Math.Max(0, _neighbourParams.Count);
        _neighbourParamDict = new Dictionary<int, RoomGenParams>();
        List<RoomGenParams> modList = new List<RoomGenParams>(MaxNeighbours);
        for (int i = 1; i <= MaxNeighbours; i++)
        {
            var @params = _neighbourParams.Count >= i ? _neighbourParams[i - 1] : new RoomGenParams(i, 0, 0) ;
            var newParams = new RoomGenParams(i, @params.Weight, @params.Priority);
            modList.Add(newParams);
            _neighbourParamDict[i] = newParams;
        }
    
        modList = modList.OrderBy(x => x.Neighbours).ToList();
        _neighbourParams = modList;
    }

    public int MaxRepresentedNeighbours()
    {
        return _neighbourParamDict.Keys.Max();
    }
    
    public float GetProbability(int neighbours, float distance = -1)
    {
        int trueNeighbours = PropagateHigher ? Math.Min(neighbours, MaxRepresentedNeighbours()) : neighbours;
        if (MaxRepresentedNeighbours() < trueNeighbours)
        {
            return 0;
        }

        float distanceMod;
        if (distance < 0 || (FloatComparer.AreEqual(_distanceModZero, 0, 0.1f) && FloatComparer.AreEqual(_distanceModOne, 0, 0.1f) ))
        {
            distanceMod = 1;
        }
        else
        {
            distanceMod = Mathf.Lerp(0, 1, Mathf.InverseLerp(_distanceModZero, _distanceModOne, distance));
        }

        return UniversalProbabilityModifier * distanceMod * _neighbourParamDict[trueNeighbours].Weight;
    }
    
    public List<List<int>> GetNeighboursByPriority()
    {
        List<List<int>> toReturn = new List<List<int>>();
        List<int> withThisPriority = new List<int>();
        var descendingPriority = _neighbourParams.OrderByDescending(y => y.Priority).Select(x => x.Neighbours).ToList();
        int currentPriority = Int32.MaxValue;
        for (int i = 0; i < descendingPriority.Count; i++)
        {
            if (descendingPriority[i] < currentPriority && withThisPriority.Count > 0)
            {
                toReturn.Add(withThisPriority);
                withThisPriority = new List<int>();
            }
            withThisPriority.Add(descendingPriority[i]);
        }
        toReturn.Add(withThisPriority);

        return toReturn;
    }
}
