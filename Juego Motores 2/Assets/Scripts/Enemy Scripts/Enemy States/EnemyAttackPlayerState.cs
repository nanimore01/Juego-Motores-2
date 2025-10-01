using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAttackPlayerState : IState
{
    FSM _fsm;
    EnemyStats _stats => _getStats.Get();
    IGet<EnemyStats> _getStats;
    EnemyBasic _me;
    VoiceLines _voiceLines => _getVoiceLines.Get();
    IGet<VoiceLines> _getVoiceLines;
    float _rotationSpeed;
    Animator _animator => _stats.animator;
    CountdownTimer _movementTimer;
    Rigidbody _rb => _me._rb;
    Vector3 _pj;
    float _xmovement, _zmovement;


    UnityAction behaviour;

    BehaviorAvoidance _behaviorAvoidance;
    public EnemyAttackPlayerState(EnemyBasic me)
    {
        _fsm = me._fsm;
        _me = me;
        _getStats = me.GetComponent<IGet<EnemyStats>>();
        _getVoiceLines = me.GetComponent<IGet<VoiceLines>>();


        _behaviorAvoidance = new BehaviorAvoidance(me.transform, me);
        var time = Random.Range(0.5f, 3);
        _movementTimer = new CountdownTimer(time);
        _movementTimer.OnTimerStop += ChangeMovement;
        EventManager.player.PlayerPosition += GetPlayerPosition;
    }

    public void OnEnter()
    {
        EventManager.player.PlayerPosition += GetPlayerPosition;
    }

    public void OnExit()
    {
        EventManager.player.PlayerPosition -= GetPlayerPosition;
    }

    public void OnUpdate()
    {
        behaviour.Invoke();
    }

    public void ChangeMovement()
    {
        _xmovement = _pj.x + Random.Range(-1, 2);
        _zmovement = _pj.z + Random.Range(-1, 2);
        var time = Random.Range(0.5f, 3);
        _movementTimer.Reset(time);
        _movementTimer.Start();
    }

    public void OnViewPlayer()
    {
        _movementTimer.Tick(Time.deltaTime);

        Quaternion targetRot = Quaternion.LookRotation(_pj);
        _me.transform.rotation = Quaternion.Slerp(
            _me.transform.rotation,
            targetRot,
            _rotationSpeed * Time.deltaTime);

        
        Vector3 movement = new Vector3(_xmovement,_me.transform.position.y,_zmovement);
        Vector3 finalDir = _behaviorAvoidance.GetAvoidanceDirection(movement.normalized);

        _me.Move(finalDir);

        if (!_me.InFOV(_pj))
        {
            _me.SetPath(Pathfinding.CalculateThetaStar(Pathfinding.GetMinNode(_me.transform.position), Pathfinding.GetMinNode(_pj)));
            behaviour = OnLostView;
        }
    }

    public void OnLostView()
    {
        Vector3 posNode = new Vector3(_me.path[0].transform.position.x, _me.transform.position.y, _me.path[0].transform.position.z);
        var dir = posNode - _me.transform.position;

        if (_me.path.Count > 0)
        {
            if (dir.magnitude <= 1f)
            {
                DebugPrint.ConsecutiveLog("Choque con el nodo");
                _me.path.RemoveAt(0);
            }
        }

        if (dir.sqrMagnitude > .01f)
        {
            Vector3 finalDir = _behaviorAvoidance.GetAvoidanceDirection(dir.normalized);
            Debug.DrawRay(_me.transform.position, finalDir);
            DebugPrint.ConsecutiveLog("Direccion Final: " + finalDir);
            Quaternion targetRot = Quaternion.LookRotation(finalDir);
            _me.transform.rotation = Quaternion.Slerp(
                _me.transform.rotation,
                targetRot,
                _rotationSpeed * Time.deltaTime
            );

            _me.Move(finalDir);
            //
        }

        Vector3 localVel = _me.transform.InverseTransformDirection(_rb.velocity);
        _animator.SetFloat("Horizontal", Mathf.Clamp(localVel.x, -1, 1));
        _animator.SetFloat("Vertical", Mathf.Clamp(localVel.z, -1, 1));
        _me.Horizontal = Mathf.Clamp(localVel.x, -1, 1);
        _me.Vertical = Mathf.Clamp(localVel.z, -1, 1);
    }

    public void GetPlayerPosition(Vector3 player)
    {
        _pj = player;
    }
}


