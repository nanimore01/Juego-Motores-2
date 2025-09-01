using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBasic : Entity
{
    private FSM _fsm;
    [SerializeField] EnemyStats stats;
    [SerializeField] VoiceLines voiceLines;
    public GameObject POV;

    public float Horizontal, Vertical;


    public List<Node> path;

    [SerializeField] float _viewRadius;
    [SerializeField] float _viewAngle, _hearRadius;
    public UnityAction OnHeardPlayer;
    public UnityAction<float> progressOfSpotReaction;
    public UnityAction OnSpotedPlayer;
    CountdownTimer _reactionTimer;
    public void Awake()
    {
        _currHp = _maxHP;

        _fsm = new FSM();

        _fsm.CreateState("Patrol", new EnemyPatrolState(this, _fsm, stats));

        _fsm.ChangeState("Patrol");
        EventManager.player.OnLastPositionHeard += Audition;

        _reactionTimer = new CountdownTimer(stats.reactionTime);

        _reactionTimer.OnTimerStop += OnSpotPlayer;
    }

    public void Update()
    {
        _fsm.Execute();
        _reactionTimer.Tick(Time.deltaTime);
    }

    public void Audition(Vector3 playerPosition)  
    {
        var dir = playerPosition - transform.position;

        if(dir.magnitude < _hearRadius)
        {
            OnHeardPlayer?.Invoke();
        }
    }

    public void DisableAudition()
    {

    }

    public void StartReactionTime()
    {
        _reactionTimer.Start();
    }

    public void ResetReactionTime()
    {
        _reactionTimer.Reset();
    }

    public void OnSpotPlayer()
    {
        _reactionTimer.Stop();

        OnSpotedPlayer.Invoke();
    }

    public bool InLineOfSight(Vector3 start, Vector3 end)
    {
        var dir = end - start;

        return !Physics.Raycast(start, dir, dir.magnitude, stats.wallLayer);
    }

    public bool InFOV(Vector3 obj)
    {
        var dir = obj - transform.position;

        if (dir.magnitude < _viewRadius)
        {
            if (Vector3.Angle(transform.forward, dir) <= _viewAngle * 0.5f)
            {
                return InLineOfSight(POV.transform.position, obj);
            }
        }

        return false;
    }

    public List<Node> CalculateAStar(Node startingNode, Node goalNode)
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

    public List<Node> CalculateThetaStar(Node startingNode, Node goalNode)
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

    public void SetPath(List<Node> newPath)
    {

        path.Clear();

        foreach (var item in newPath)
        {
            print("Agrego item a la lista");
            path.Add(item);
        }

        
    }
}

[System.Serializable]
public struct EnemyStats
{
    [Header("Speed Stats")]
    public float maxForce;
    public float maxVelocity;
    public float rotationSpeed;
    
    
    [Header("Detection Stats")]
    public float hearRadius;
    public float reactionTime;
    public float detectionDistance;

    [Header("Behavior settings")]
    public float avoidanceStrength;

    [Header("Settings")]
    public Node[] nodePatrol;
    public Animator animator;
    public GameObject pointing;
    public LayerMask wallLayer;
    
}

[System.Serializable]
public struct VoiceLines
{

}
