using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolState : IState
{
    EnemyBasic _me;
    FSM _fsm;
    EnemyStats _stats;

    Vector3 _velocity;
    float _maxVelocity, _maxForce;
    Node[] _patrol;
    int _currWaypoint = 0;

    public EnemyPatrolState(EnemyBasic me, FSM fsm, EnemyStats stats)
    {
        _me = me;
        _fsm = fsm;
        _stats = stats;

        _maxVelocity = _stats.maxVelocity;
        _maxForce = _stats.maxForce;

    }

    public void OnEnter()
    {
        
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
        AddForce(Seek(_patrol[_currWaypoint].transform.position));

        if (Vector3.Distance(_patrol[_currWaypoint].transform.position, _me.transform.position) <= 0.5f)
        {
            _currWaypoint++;

            if (_currWaypoint >= _patrol.Length)
                _currWaypoint = 0;
        }

        _me.transform.position += _velocity * Time.deltaTime;
        _me.transform.forward = _velocity;
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
