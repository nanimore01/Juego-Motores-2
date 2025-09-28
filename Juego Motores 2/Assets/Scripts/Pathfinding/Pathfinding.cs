using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

public static class Pathfinding
{
    private static IPathfindingData s_data;
    
    public static LayerMask WallLayers => s_data.WallLayers;

    public static List<Node> Nodes => s_data.Nodes;
    
#if UNITY_EDITOR
    
    [InitializeOnLoadMethod]
    [DidReloadScripts]
    private static void InitInScene()
    {
        var nodes = Object.FindObjectsByType<Node>(FindObjectsSortMode.None);
        
        if(nodes.Length==0)
            return;
        
        PathfindingScene pathfindingScene;
        
        if ((pathfindingScene = Object.FindObjectOfType<PathfindingScene>()) == null)
        {
            var go = new GameObject("Pathfinding Wrapper");
            pathfindingScene = go.AddComponent<PathfindingScene>();
        }
        
        pathfindingScene.Nodes.Clear();
        
        pathfindingScene.Nodes.AddRange(nodes);
    }
#endif
    
    public static List<Node> CalculateAStar(Node startingNode, Node goalNode)
    {
        PriorityQueue<Node> frontier = new PriorityQueue<Node>();
        frontier.Enqueue(startingNode, 0);

        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        cameFrom.Add(startingNode, null);

        Dictionary<Node, int> costSoFar = new Dictionary<Node, int>();
        costSoFar.Add(startingNode, 0);

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();

            if (current == goalNode)
            {
                List<Node> path = new List<Node>();

                while (current != startingNode)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }

                path.Reverse();
                return path;
            }

            foreach (var item in current.neighbour)
            {
                int newCost = costSoFar[current] + item.cost;
                float priority = newCost + Vector3.Distance(item.transform.position, goalNode.transform.position);

                if (!costSoFar.ContainsKey(item))
                {
                    if (!frontier.ContainsKey(item))
                        frontier.Enqueue(item, priority);
                    cameFrom.Add(item, current);
                    costSoFar.Add(item, newCost);
                }
                else if (costSoFar[item] > newCost)
                {
                    if (!frontier.ContainsKey(item))
                        frontier.Enqueue(item, priority);
                    cameFrom[item] = current;
                    costSoFar[item] = newCost;
                }
            }
        }
        return new List<Node>();
    }

    public static List<Node> CalculateThetaStar(Node startingNode, Node goalNode)
    {
        var listNode = CalculateAStar(startingNode, goalNode);

        int current = 0;

        while (current + 2 < listNode.Count)
        {
            if (InLineOfSight(listNode[current].transform.position, listNode[current + 2].transform.position))
            {
                listNode.RemoveAt(current + 1);
            }
            else
                current++;
        }

        return listNode;
    }

    public static bool InLineOfSight(Vector3 start, Vector3 end)
    {
        var dir = end - start;

        return !Physics.Raycast(start, dir, dir.magnitude, WallLayers);
    }

    public static Node GetMinNode(Vector3 position)
    {
        Node minNode = null;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < Nodes?.Count; i++)
        {
            if (InLineOfSight(Nodes[i].transform.position, position))
            {
                if (Vector3.Distance(Nodes[i].transform.position, position) < minDist)
                {
                    minNode = Nodes[i];
                    minDist = Vector3.Distance(Nodes[i].transform.position, position);
                }
            }
        }

        return minNode;
    }

    public static void SetPath(IPathfindingData pathfindingData)
    {
        s_data = pathfindingData;
    }
}

public interface IPathfindingData
{
    public LayerMask WallLayers { get; }

    public List<Node> Nodes { get; }
}

