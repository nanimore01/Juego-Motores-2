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
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
        update.Invoke(); 
    }

    public void OnInspect()
    {
        _inpectTimer.Tick(Time.deltaTime);
        
    }

    public void OnStopInspect()
    {
        update = OnReturnPatrol;
        _me.SetPath(_me.CalculateThetaStar(_me.GetMinNode(_me.transform.position), _me.GetMinNode(_stats.nodePatrol[0].transform.position)));
    }

    public void OnReturnPatrol()
    {
        Vector3 posNode = new Vector3(_me.path[0].transform.position.x, _me.transform.position.y, _me.path[0].transform.position.z);
        var dir = posNode - _me.transform.position;

        if (_me.path.Count > 0)
        {
            if (_me.InLineOfSight(_me.POV.transform.position, _me.path[0].transform.position) == false)
            {
                //_me.SetPath(_me.CalculateThetaStar(_me.GetMinNode(_me.transform.position), _me.GetMinNode(_point)));
            }

            if (dir.magnitude <= .5f)
            {
                Debug.Log("Choque con el nodo");
                _me.path.RemoveAt(0);
            }
        }

        if (dir.sqrMagnitude > .01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _me.transform.rotation = Quaternion.Slerp(
                _me.transform.rotation,
                targetRot,
                _rotationSpeed * Time.deltaTime
            );
        }

        _me.Move(_me.transform.forward);

        Vector3 localVel = _me.transform.InverseTransformDirection(_rb.velocity);

        _animator.SetFloat("Horizontal", Mathf.Clamp(localVel.x, -1, 1));
        _animator.SetFloat("Vertical", Mathf.Clamp(localVel.z, -1, 1));

        _me.Horizontal = Mathf.Clamp(localVel.x, -1, 1);
        _me.Vertical = Mathf.Clamp(localVel.z, -1, 1);

        if (_me.path.Count == 0)
        {
            _fsm.ChangeState("Patrol");
        }
    }
}
