using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArenaManager : MonoBehaviour
{
    public List<Node> allNodes, loadedNodes;

    public UnityEvent OnStartArena;

    

    public void Awake()
    {
        LoadNodes();
        loadedNodes = EventManager.arena.ActiveNodes;
    }

    public void StartArena()
    {
        OnStartArena?.Invoke();
    }

    public void LoadNodes()
    {
        EventManager.arena.ActiveNodes = allNodes;
    }

    
}
