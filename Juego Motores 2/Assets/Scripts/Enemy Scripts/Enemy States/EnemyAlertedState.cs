using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAlertedState : IState
{
    EnemyBasic _me;
    FSM _fsm;
    EnemyStats _stats;
    Rigidbody _rb;

    float _rotationSpeed;

    Vector3 _point;
    Animator _animator;
    public EnemyAlertedState(EnemyBasic me, FSM fsm, EnemyStats stats)
    {
        _me = me;
        _fsm = fsm;
        _stats = stats;

        _animator = _stats.animator;
        _rotationSpeed = _stats.rotationSpeed;

        _rb = _me.gameObject.GetComponent<Rigidbody>();
    }

    
    public void OnEnter()
    {
        EventManager.player.OnLastPositionHeard += SetPoint;
        _me.SetPath(_me.CalculateThetaStar(_me.GetMinNode(_me.transform.position), _me.GetMinNode(_point)));
    }

    public void OnExit()
    {
        EventManager.player.OnLastPositionHeard -= SetPoint;
    }

    public void OnUpdate()
    {
        Vector3 posNode = new Vector3(_me.path[0].transform.position.x, _me.transform.position.y, _me.path[0].transform.position.z);
        var dir = posNode - _me.transform.position;

        _me.Move(_me.transform.forward);
        if (_me.path.Count > 0)
        {

            

            if (_me.InLineOfSight(_me.POV.transform.position, _me.path[0].transform.position) == false)
            {
                //_me.SetPath(_me.CalculateThetaStar(_me.GetMinNode(_me.transform.position), _me.GetMinNode(_point)));
            }

            if (dir.magnitude <= 1f)
            {
                Debug.Log("Choque con el nodo");
                _me.path.RemoveAt(0);
            }
        }

        if (dir.sqrMagnitude > 1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _me.transform.rotation = Quaternion.Slerp(
                _me.transform.rotation,
                targetRot,
                _rotationSpeed * Time.deltaTime
            );
        }

        Vector3 localVel = _me.transform.InverseTransformDirection(_rb.velocity);

        _animator.SetFloat("Horizontal", Mathf.Clamp(localVel.x, -1, 1));
        _animator.SetFloat("Vertical", Mathf.Clamp(localVel.z, -1, 1));

        _me.Horizontal = Mathf.Clamp(localVel.x, -1, 1);
        _me.Vertical = Mathf.Clamp(localVel.z, -1, 1);
    }

    public void SetPoint(Vector3 position)
    {
        _point = position;
    }
}
