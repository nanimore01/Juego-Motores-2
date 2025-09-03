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
        //EventManager.player.PlayerPosition += GetPlayerPosition;
        //_me.OnHeardPlayer += OnHeardPlayer;
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

        
        //AddForce(Seek(_patrol[_currWaypoint].transform.position));

        //_animator.SetFloat("Horizontal", _patrol[_currWaypoint].transform.position.x);
        //_animator.SetFloat("Vertical", _patrol[_currWaypoint].transform.position.y);

        if (Vector3.Distance(_patrol[_currWaypoint].transform.position, _me.transform.position) <= 0.5f)
        {
            _currWaypoint++;

            if (_currWaypoint >= _patrol.Length)
                _currWaypoint = 0;
        }

        //_me.transform.position += _velocity * Time.deltaTime;
        //_me.transform.forward = _velocity;


        //_me.transform.position += _velocity * Time.deltaTime;

        //// Rotación suavizada
        //if (_velocity.sqrMagnitude > 0.01f)
        //{
        //    Quaternion targetRot = Quaternion.LookRotation(_velocity.normalized);
        //    _me.transform.rotation = Quaternion.Slerp(
        //        _me.transform.rotation,
        //        targetRot,
        //        _rotationSpeed * Time.deltaTime
        //    );
        //}

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

        _me.Move(_me.transform.forward);

        Vector3 localVel = _me.transform.InverseTransformDirection(_rb.velocity);

        _animator.SetFloat("Horizontal", Mathf.Clamp(localVel.x, -1, 1));
        _animator.SetFloat("Vertical", Mathf.Clamp(localVel.z, -1, 1));

        _me.Horizontal = Mathf.Clamp(localVel.x, -1, 1);
        _me.Vertical = Mathf.Clamp(localVel.z, -1, 1);

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
            
        }
    }

    public void OnSpotPlayer()
    {
        Debug.Log("Te detecte");
    }

    Vector3 Seek(Vector3 dir)
    {
        var desired = dir - _me.transform.position;
        desired.Normalize();
        desired *= _maxVelocity;

        var steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce);

        return steering;
    }
    void AddForce(Vector3 dir)
    {
        _velocity += dir;

        _velocity = Vector3.ClampMagnitude(_velocity, _maxVelocity);
    }
}
