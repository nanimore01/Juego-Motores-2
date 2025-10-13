using System;
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

    [SerializeField] float _viewRadius;
    [SerializeField] float _viewAngle, _hearRadius;
    public UnityAction OnHeardPlayer;
    public UnityAction<float> progressOfSpotReaction;
    public UnityAction OnSpotedPlayer;
    public UnityAction OnStopInspect;
    CountdownTimer _reactionTimer;
    public Rigidbody _rb;
    [SerializeField]private float _dampingFactor = 10;

    [SerializeField]
    private float _dotViewAngle;

    
#if UNITY_EDITOR
    private void OnValidate()
    {
        var vecAngle = Quaternion.Euler(0, 0, _viewAngle / 2) * Vector3.up;

        _dotViewAngle = Vector3.Dot(vecAngle, Vector3.up);
        
        _rb = gameObject.GetComponent<Rigidbody>();
    }
#endif

    public void Awake()
    {
        ((IGet<EnemyStats>)this).Get();

        _currHp = _maxHP;

        _fsm = new FSM();

        _fsm.CreateState("Patrol", new EnemyPatrolState(this, _fsm, stats, voiceLines));
        _fsm.CreateState("Sound Heard", new EnemyAlertedState(this, _fsm, stats, voiceLines));
        _fsm.CreateState("Inspect", new EnemyInspectState(this));
        _fsm.CreateState("Attack", new EnemyAttackPlayerState(this));


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

        OnSpotedPlayer?.Invoke();
        _fsm.ChangeState("Attack");
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
            _rb.velocity = Vector3.Lerp(_rb.velocity, new Vector3(0, _rb.velocity.y, 0), Time.deltaTime * _dampingFactor);
        }
    }



    public bool InFOV(Vector3 obj)
    {
        var dir = obj - transform.position;
        
        if (dir.sqrMagnitude < _viewRadius * _viewRadius)
        {
            if (_dotViewAngle < Vector3.Dot(dir.normalized, transform.forward))
            {
                return Pathfinding.InLineOfSight(POV.transform.position, obj);
            }
        }

        return false;
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
    public float minDistanceToPlayer;

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
