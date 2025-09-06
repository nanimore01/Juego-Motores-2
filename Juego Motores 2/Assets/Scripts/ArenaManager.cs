using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArenaManager : MonoBehaviour
{
    public List<Node> allNodes;

    public UnityEvent OnStartArena;

    public void StartArena()
    {
        OnStartArena?.Invoke();
    }

    public void LoadNodes()
    {
        EventManager.arena.ActiveNodes = allNodes;
    }

    
}
