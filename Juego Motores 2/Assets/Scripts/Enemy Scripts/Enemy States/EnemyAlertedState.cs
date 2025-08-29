using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAlertedState : IState
{
    EnemyBasic _me;
    FSM _fsm;
    EnemyStats _stats;

    Vector3 _velocity;

    public EnemyAlertedState(EnemyBasic me, FSM fsm, EnemyStats stats)
    {
        _me = me;
        _fsm = fsm;
        _stats = stats;
    }

    public void OnEnter()
    {
        
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {



        if (_me.path.Count > 0)
        {
            Vector3 posNode = new Vector3(_me.path[0].transform.position.x, _me.transform.position.y, _me.path[0].transform.position.z);
            var dir = posNode - _me.transform.position;

            Vector3 leftRayDirection = Quaternion.AngleAxis(-45, Vector3.up) * _me.POV.transform.forward;
            Vector3 rightRayDirection = Quaternion.AngleAxis(45, Vector3.up) * _me.POV.transform.forward;

            if (Physics.Raycast(_me.POV.transform.position, rightRayDirection, _stats.detectionDistance, _stats.wallLayer) || Physics.Raycast(_me.POV.transform.position, leftRayDirection, _stats.detectionDistance, _stats.wallLayer))
            {
                AddForce(CalculateAvoidanceDirection(_stats.detectionDistance, _stats.avoidanceStrength));
                Debug.Log("Detecto pared");
            }
            else
            {
                AddForce(Seek(posNode));
            }

            //AddForce(CalculateAvoidanceDirection(0.1f, 1));

            if (_me.InLineOfSight(_me.POV.transform.position, _me.path[0].transform.position) == false)
            {
                //_me.SetPath(_me.CalculateThetaStar(GameManager.instance.arena.GetMinNode(_me.transform.position), GameManager.instance.arena.GetMinNode(GameManager.instance.pj.transform.position)));
            }

            if (dir.magnitude <= 1f)
            {
                Debug.Log("Choque con el nodo");
                _me.path.RemoveAt(0);
            }
        }

        _me.transform.position += _velocity * Time.deltaTime;
        _me.transform.forward = _velocity;
    }

    private Vector3 CalculateAvoidanceDirection(float detectionDistance, float avoidanceStrength)
    {
        Vector3 leftRayDirection = Quaternion.AngleAxis(-45, Vector3.up) * _me.POV.transform.forward;
        Vector3 rightRayDirection = Quaternion.AngleAxis(45, Vector3.up) * _me.POV.transform.forward;

        Debug.DrawRay(_me.POV.transform.position, leftRayDirection * detectionDistance, Color.green);
        Debug.DrawRay(_me.POV.transform.position, rightRayDirection * detectionDistance, Color.green);

        if (!Physics.Raycast(_me.POV.transform.position, rightRayDirection, detectionDistance, _stats.wallLayer))
        {
            return rightRayDirection * avoidanceStrength;
        }
        else if (!Physics.Raycast(_me.POV.transform.position, leftRayDirection, detectionDistance, _stats.wallLayer))
        {
            return leftRayDirection * avoidanceStrength;
        }
        return -_me.transform.forward;
    }
    void AddForce(Vector3 dir)
    {
        _velocity += dir;

        _velocity = Vector3.ClampMagnitude(_velocity, _stats.maxVelocity);
    }

    Vector3 Seek(Vector3 dir)
    {
        var desired = dir - _me.transform.position;
        desired.Normalize();
        desired *= _stats.maxVelocity;

        var steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _stats.maxForce);

        return steering;
    }
}
