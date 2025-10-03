using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyInspectState : IState
{
    FSM _fsm;
    EnemyBasic _me;

    IGet<EnemyStats> _getStats;
    EnemyStats _stats => _getStats.Get();
    Animator _animator => _stats.animator;
    Rigidbody _rb;
    Vector3 _pj;
    VoiceLines _voiceLines;

    float _rotationSpeed => _stats.rotationSpeed;
    

    CountdownTimer _inpectTimer;


    UnityAction update;

    BehaviorAvoidance _behaviorAvoidance;
    BehaviourOnPath _behaviourOnPath;
    public EnemyInspectState(EnemyBasic me)
    {
        _fsm = me._fsm;
        _me = me;
        _getStats = me.GetComponent<IGet<EnemyStats>>();

        _inpectTimer = new CountdownTimer(_stats.inpectTime);

        _behaviourOnPath = new BehaviourOnPath(me.transform);
        _behaviorAvoidance = new BehaviorAvoidance(me.transform, me);
        _rb = _me.gameObject.GetComponent<Rigidbody>();
        EventManager.player.PlayerPosition += GetPlayerPosition;
    }



    public void OnEnter()
    {
        Debug.Log("Inspect Mode");
        _inpectTimer.Reset();
        _inpectTimer.Start();

        update = OnInspect;
        
        _inpectTimer.OnTimerStop += OnStopInspect;

        EventManager.player.PlayerPosition += GetPlayerPosition;
        _me.OnHeardPlayer += OnHeardPlayer;
        _me.OnSpotedPlayer += OnSpotPlayer;

        
    }

    public void OnExit()
    {
        _me.OnHeardPlayer -= OnHeardPlayer;
        _me.OnSpotedPlayer -= OnSpotPlayer;
        EventManager.player.PlayerPosition -= GetPlayerPosition;
        _inpectTimer.OnTimerStop -= OnStopInspect;
    }

    public void OnUpdate()
    {
        update.Invoke(); 
    }

    public void OnInspect()
    {
        _inpectTimer.Tick(Time.deltaTime);
        _animator.SetFloat("Horizontal", 0);
        _animator.SetFloat("Vertical", 0);
        _me._rb.velocity = Vector3.zero;
        
    }

    public void OnStopInspect()
    {
        update = OnReturnPatrol;
        _me.OnStopInspect?.Invoke();
        _behaviourOnPath.SetPath(Pathfinding.CalculateThetaStar(Pathfinding.GetMinNode(_me.transform.position), Pathfinding.GetMinNode(_stats.nodePatrol[0].transform.position)));
    }

    public void OnReturnPatrol()
    {
        _behaviourOnPath.PathBehaviour();
        Vector3 finalDir = _behaviorAvoidance.GetAvoidanceDirection(_behaviourOnPath.dir.normalized);
        if (_behaviourOnPath.dir.sqrMagnitude > .01f)
        {
            //Vector3 finalDir = GetAvoidanceDirection(dir.normalized);

            Quaternion targetRot = Quaternion.LookRotation(finalDir);
            _me.transform.rotation = Quaternion.Slerp(
                _me.transform.rotation,
                targetRot,
                _rotationSpeed * Time.deltaTime
            );

            
            _me.Move(finalDir);
        }

        Debug.Log("Direccion Final: " + finalDir);

        Vector3 localVel = _me.transform.InverseTransformDirection(_rb.velocity);
        _animator.SetFloat("Horizontal", Mathf.Clamp(localVel.x, -1f, 1f));
        _animator.SetFloat("Vertical", Mathf.Clamp(localVel.z, -1f, 1f));

        _me.Horizontal = Mathf.Clamp(localVel.x, -1f, 1f);
        _me.Vertical = Mathf.Clamp(localVel.z, -1f, 1f);

        if (_me.path.Count == 0)
        {
            _fsm.ChangeState("Patrol");
        }
    }

    public void GetPlayerPosition(Vector3 player)
    {
        _pj = player;
    }

    public void OnHeardPlayer()
    {
        //_fsm.ChangeState("Sound Heard");

        if (Pathfinding.InLineOfSight(_me.transform.position, _pj))
        {
            OnSpotPlayer();
        }
        else
        {
            _fsm.ChangeState("Sound Heard");
        }
    }
    public void OnSpotPlayer()
    {
        _fsm.ChangeState("Attack");
    }

}
