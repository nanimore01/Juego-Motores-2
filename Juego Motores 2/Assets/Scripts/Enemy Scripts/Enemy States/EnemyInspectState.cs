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
    public EnemyInspectState(EnemyBasic me)
    {
        _fsm = me._fsm;
        _me = me;
        _getStats = me.GetComponent<IGet<EnemyStats>>();

        _inpectTimer = new CountdownTimer(_stats.inpectTime);

        _behaviorAvoidance = new BehaviorAvoidance(me.transform, me);
        _rb = _me.gameObject.GetComponent<Rigidbody>();
        EventManager.player.PlayerPosition += GetPlayerPosition;
    }



    public void OnEnter()
    {
        Debug.Log("Inspect Mode");
        update = OnInspect;
        _inpectTimer.Start();
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
    }

    public void OnStopInspect()
    {
        update = OnReturnPatrol;
        _me.OnStopInspect?.Invoke();
        _me.SetPath(_me.CalculateThetaStar(_me.GetMinNode(_me.transform.position), _me.GetMinNode(_stats.nodePatrol[0].transform.position)));
    }

    public void OnReturnPatrol()
    {
        if (_me.path == null || _me.path.Count == 0)
        {
            _fsm.ChangeState("Patrol");
            return;
        }

        Vector3 posNode = new Vector3(_me.path[0].transform.position.x, _me.transform.position.y, _me.path[0].transform.position.z);
        var dir = posNode - _me.transform.position;

        if (_me.path.Count > 0)
        {
            if (dir.magnitude <= 1f)
            {
                Debug.Log("Choque con el nodo");
                _me.path.RemoveAt(0);
            }
        }
        Vector3 finalDir = _behaviorAvoidance.GetAvoidanceDirection(dir.normalized);
        if (dir.sqrMagnitude > .01f)
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

        if (_me.InLineOfSight(_me.transform.position, _pj))
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
        Debug.Log("Te detecte");
    }

}
