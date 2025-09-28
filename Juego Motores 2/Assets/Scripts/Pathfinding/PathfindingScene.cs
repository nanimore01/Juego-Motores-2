using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
public class PathfindingScene : MonoBehaviour, IPathfindingData
{
    [field: SerializeField]
    public LayerMask WallLayers { get; set; }

    [field: SerializeField]
    public List<Node> Nodes { get; set; } = new();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(WallLayers == 0)
            Debug.LogWarning("No se configuro las wallLayers", this);
    }
#endif
    

    private void Awake()
    {
        Pathfinding.SetPath(this);
    }
}


