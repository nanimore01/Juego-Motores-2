using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBasic : Entity, IGet<EnemyStats>, IGet<VoiceLines>
{
    
    public FSM _fsm;
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
    public UnityAction OnStopInspect;
    CountdownTimer _reactionTimer;
    public Rigidbody _rb;
    float _dampingFactor = 10;

    
    
    public void Awake()
    {
        ((IGet<EnemyStats>)this).Get();
        _rb = gameObject.GetComponent<Rigidbody>();

        _currHp = _maxHP;

        _fsm = new FSM();

        _fsm.CreateState("Patrol", new EnemyPatrolState(this, _fsm, stats, voiceLines));
        _fsm.CreateState("Sound Heard", new EnemyAlertedState(this, _fsm, stats, voiceLines));
        _fsm.CreateState("Inspect", new EnemyInspectState(this));

        _fsm.ChangeState("Patrol");
        EventManager.player.OnLastPositionHeard += Audition;

        _reactionTimer = new CountdownTimer(stats.reactionTime);

        _reactionTimer.OnTimerStop += OnSpotPlayer;

        
    }

    public void Update()
    {
        _fsm.Execute();
        _reactionTimer.Tick(Time.deltaTime);

        //SpeedControl();
    }

    public void Audition(Vector3 playerPosition)  
    {
        var dir = playerPosition - transform.position;

        if(dir.sqrMagnitude < _hearRadius * _hearRadius)
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
    public void Move(Vector3 direction)
    {
        _rb.AddForce(direction.normalized * stats.maxVelocity * 10f, ForceMode.Force);

        //stats.animator.SetFloat("Horizontal", Mathf.Clamp(direction.x, -1, 1));
        //stats.animator.SetFloat("Vertical", Mathf.Clamp(direction.z, -1, 1));

        SpeedControl();
    }

    private void SpeedControl()
    {
        
        Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        
        if (flatVel.magnitude > stats.maxVelocity)
        {
            Vector3 limitedVel = flatVel.normalized * stats.maxVelocity;
            _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
        }
        else
        {
            _rb.velocity = Vector3.Lerp(
                _rb.velocity,
                new Vector3(0, _rb.velocity.y, 0),
                Time.deltaTime * _dampingFactor
            );
        }
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

    public Node GetMinNode(Vector3 position)
    {
        print("Funciono");
        Node minNode = null;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < EventManager.arena.ActiveNodes?.Count; i++)
        {
            if (InLineOfSight(EventManager.arena.ActiveNodes[i].transform.position, position))
            {
                if (Vector3.Distance(EventManager.arena.ActiveNodes[i].transform.position, position) < minDist)
                {

                    minNode = EventManager.arena.ActiveNodes[i];
                    minDist = Vector3.Distance(EventManager.arena.ActiveNodes[i].transform.position, position);

                }
            }
        }

        return minNode;
    }

    EnemyStats IGet<EnemyStats>.Get()
    {
        return stats;
    }

    VoiceLines IGet<VoiceLines>.Get()
    {
        return voiceLines;
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
    public float inpectTime;
    public float avoidanceStrength;
    public float avoidanceDistance;
    public float spreadAngle; 

    [Header("Settings")]
    public Node[] nodePatrol;
    public Animator animator;
    public GameObject pointing;
    public LayerMask wallLayer;
    
}

[System.Serializable]
public struct VoiceLines
{
    [Header("Relaxed Voice Lines")]
    public AudioClip onHeardASound;
    
}
