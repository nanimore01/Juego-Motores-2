using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolState : IState
{
    EnemyBasic _me;
    FSM _fsm;
    EnemyStats _stats;

    Vector3 _velocity;
    float _maxVelocity, _maxForce, _rotationSpeed;
    Node[] _patrol;
    int _currWaypoint = 0;
    int _previousWaypoint;
    Animator _animator;
    Rigidbody _rb;
    Vector3 _pj;
    public EnemyPatrolState(EnemyBasic me, FSM fsm, EnemyStats stats)
    {
        _me = me;
        _fsm = fsm;
        _stats = stats;

        _maxVelocity = _stats.maxVelocity;
        _maxForce = _stats.maxForce;
        _patrol = _stats.nodePatrol;
        _animator = _stats.animator;
        _rotationSpeed = _stats.rotationSpeed;

        _rb = _me.gameObject.GetComponent<Rigidbody>();
    }

    public void OnEnter()
    {
        Debug.Log("Patrol Mode");
        //EventManager.player.PlayerPosition += GetPlayerPosition;
        _me.OnHeardPlayer += OnHeardPlayer;
        //_me.OnSpotedPlayer += OnSpotPlayer;
    }

    public void OnExit()
    {
        EventManager.player.PlayerPosition -= GetPlayerPosition;
        _me.OnHeardPlayer -= OnHeardPlayer;
        _me.OnSpotedPlayer -= OnSpotPlayer;
    }

    public void OnUpdate()
    {
        Vector3 nodo = new Vector3(_patrol[_currWaypoint].transform.position.x, _me.transform.position.y, _patrol[_currWaypoint].transform.position.z);
        var NextNode = _patrol[_currWaypoint];
        var PreviousNode = _patrol[_previousWaypoint];

        if (Vector3.Distance(nodo, _me.transform.position) <= 0.5f)
        {
            //PreviousNode.OnPatrolNode?.Invoke();

            _previousWaypoint = _currWaypoint;
            
            _currWaypoint++;

            NextNode.OnNextPatrolNode?.Invoke();

            if (_currWaypoint >= _patrol.Length)
                _currWaypoint = 0;
        }

       

        Vector3 dir = (nodo - _me.transform.position).normalized;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _me.transform.rotation = Quaternion.Slerp(
                _me.transform.rotation,
                targetRot,
                _rotationSpeed * Time.deltaTime
            );
        }

        if(NextNode.CanContinue)
        {
            _me.Move(_me.transform.forward);
        }
        

        Vector3 localVel = _me.transform.InverseTransformDirection(_rb.velocity);

        _animator.SetFloat("Horizontal", Mathf.Clamp(localVel.x, -1, 1));
        _animator.SetFloat("Vertical", Mathf.Clamp(localVel.z, -1, 1));

        _me.Horizontal = Mathf.Clamp(localVel.x, -1, 1);
        _me.Vertical = Mathf.Clamp(localVel.z, -1, 1);

        //if (_me.path.Count == 0)
        //{
        //    _fsm.ChangeState("Inspect");
        //}

        //if(_me.InFOV(_pj))
        //{
        //    Debug.Log("Veo al jugador");
        //    _me.StartReactionTime();
        //}
        //else
        //{
        //    _me.ResetReactionTime();
        //}
    }

    public void GetPlayerPosition(Vector3 player)
    {
        _pj = player;
    }


    public void OnHeardPlayer()
    {
        if(_me.InFOV(_pj))
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
