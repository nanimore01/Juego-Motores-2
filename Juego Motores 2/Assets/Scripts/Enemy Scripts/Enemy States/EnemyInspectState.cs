using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyInspectState : IState
{
    FSM _fsm;
    EnemyBasic _me;
    EnemyStats _stats;
    Animator _animator;
    Rigidbody _rb;
    Vector3 _pj;

    float _rotationSpeed;

    CountdownTimer _inpectTimer;


    UnityAction update;  
    

    public EnemyInspectState(FSM fsm, EnemyBasic me, EnemyStats stats)
    {
        _fsm = fsm;
        _me = me;
        _stats = stats;

        _inpectTimer = new CountdownTimer(_stats.inpectTime);
        _animator = _stats.animator;
        _rotationSpeed = _stats.rotationSpeed;

        _rb = _me.gameObject.GetComponent<Rigidbody>();

    }



    public void OnEnter()
    {
        Debug.Log("Inspect Mode");
        update = OnInspect;
        _inpectTimer.Start();
        _inpectTimer.OnTimerStop += OnStopInspect;

        _me.OnHeardPlayer += OnHeardPlayer;
        //_me.OnSpotedPlayer -= OnSpotPlayer;
    }

    public void OnExit()
    {
        _me.OnHeardPlayer -= OnHeardPlayer;
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
        _me.SetPath(_me.CalculateThetaStar(_me.GetMinNode(_me.transform.position), _me.GetMinNode(_stats.nodePatrol[0].transform.position)));
    }

    public void OnReturnPatrol()
    {
        if (_me.path == null || _me.path.Count == 0)
        {
            _fsm.ChangeState("Patrol");
            return;
        }


        Vector3 targetWorld = _me.path[0].transform.position;
        Vector3 posNode = new Vector3(targetWorld.x, _me.transform.position.y, targetWorld.z);

        Vector3 delta = posNode - _me.transform.position;
        float dist = delta.magnitude;
        Vector3 dir = dist > 0.001f ? (delta / dist) : Vector3.zero; 


        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _me.transform.rotation = Quaternion.Slerp(
                _me.transform.rotation,
                targetRot,
                _rotationSpeed * Time.deltaTime
            );
        }

        
        float angleToTarget = Vector3.Angle(_me.transform.forward, dir);
        const float moveAngleThreshold = 20f; 

        if (dist > 1f) 
        {
            if (angleToTarget <= moveAngleThreshold)
            {
                
                _me.Move(_me.transform.forward);
            }
          
        }
        else
        {
            _me.path.RemoveAt(0);
        }


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
        if (_me.InFOV(_pj))
        {
            //OnSpotPlayer();
        }
        else
        {
            _fsm.ChangeState("Sound Heard");
        }
    }
}
