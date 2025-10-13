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
    float _rotationSpeed => _stats.rotationSpeed;
    float _minDistanceToPlayer => _stats.minDistanceToPlayer;
    Animator _animator => _stats.animator;
    CountdownTimer _movementTimer;
    Rigidbody _rb => _me._rb;
    Vector3 _pj;
    float _xmovement, _zmovement;
    Vector3 _point;

    UnityAction behaviour;

    BehaviourOnPath _behaviourOnPath;
    BehaviorAvoidance _behaviorAvoidance;
    public EnemyAttackPlayerState(EnemyBasic me)
    {
        _fsm = me._fsm;
        _me = me;
        _getStats = me.GetComponent<IGet<EnemyStats>>();
        _getVoiceLines = me.GetComponent<IGet<VoiceLines>>();


        _behaviorAvoidance = new BehaviorAvoidance(me.transform, me);
        _behaviourOnPath = new BehaviourOnPath(me.transform);

        _behaviourOnPath.OnNullPath += OnNullPath;
        _behaviourOnPath.OnFinishedPath += OnNullPath;

        var time = Random.Range(0.5f, 3);
        _movementTimer = new CountdownTimer(time);
        _movementTimer.OnTimerStop += ChangeMovement;
        EventManager.player.PlayerPosition += GetPlayerPosition;
        EventManager.player.OnLastPositionHeard += SetPoint;
    }

    public void OnEnter()
    {
        EventManager.player.PlayerPosition += GetPlayerPosition;
        EventManager.player.OnLastPositionHeard += SetPoint;
        behaviour = OnViewPlayer;
        Debug.Log("Te voy a atacar");
        _movementTimer.Start();
    }

    public void OnExit()
    {
        EventManager.player.PlayerPosition -= GetPlayerPosition;
        EventManager.player.OnLastPositionHeard -= SetPoint;
    }

    public void OnUpdate()
    {
        behaviour.Invoke();

        Vector3 localVel = _me.transform.InverseTransformDirection(_rb.velocity);
        _animator.SetFloat("Horizontal", Mathf.Clamp(localVel.x, -1, 1));
        _animator.SetFloat("Vertical", Mathf.Clamp(localVel.z, -1, 1));
        _me.Horizontal = Mathf.Clamp(localVel.x, -1, 1);
        _me.Vertical = Mathf.Clamp(localVel.z, -1, 1);
    }

    public void ChangeMovement()
    {
        
        _xmovement = Random.Range(-1, 2);
        _zmovement = Random.Range(-1, 2);
        DebugPrint.Log("Cambio de movimiento ");

        var time = Random.Range(0.5f, 3);
        _movementTimer.Reset(time);
        _movementTimer.Start();
    }

    public void OnViewPlayer()
    {
        _movementTimer.Tick(Time.deltaTime);

        var PlayerPosition = new Vector3(_pj.x, _me.transform.position.y, _pj.z);

        Quaternion targetRot = Quaternion.LookRotation(PlayerPosition - _me.transform.position);
        _me.transform.rotation = Quaternion.Slerp(
            _me.transform.rotation,
            targetRot,
            _rotationSpeed * Time.deltaTime);

        Vector3 offset = new Vector3(_xmovement, 0, _zmovement);
        Vector3 movement = (PlayerPosition - _me.transform.position).normalized + offset;
        Vector3 finalDir = _behaviorAvoidance.GetAvoidanceDirection(movement.normalized);

        if ((PlayerPosition - _me.transform.position).magnitude > _minDistanceToPlayer * _minDistanceToPlayer)
        {
            _me.Move(finalDir);
        }
        else
        {
            _me.Move(Vector3.zero);

            //Vector3 backDir = -PlayerPosition.normalized;
            //_me.Move(backDir * 0.5f);
        }


        //Vector3 offset = new Vector3(_xmovement, 0, _zmovement);
        //Vector3 movement = (PlayerPosition - _me.transform.position).normalized + offset;
        //Vector3 finalDir = _behaviorAvoidance.GetAvoidanceDirection(movement.normalized);

        //_me.Move(finalDir);

        if (!_me.InFOV(_pj))
        {
            _behaviourOnPath.SetPath(Pathfinding.CalculateThetaStar(Pathfinding.GetMinNode(_me.transform.position), Pathfinding.GetMinNode(_pj)));
            behaviour = OnLostView;
        }
    }

    public void OnLostView()
    {
        _behaviourOnPath.PathBehaviour();

        if (_behaviourOnPath.dir.sqrMagnitude > .01f)
        {
            Vector3 finalDir = _behaviorAvoidance.GetAvoidanceDirection(_behaviourOnPath.dir.normalized);
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

        //Vector3 localVel = _me.transform.InverseTransformDirection(_rb.velocity);
        //_animator.SetFloat("Horizontal", Mathf.Clamp(localVel.x, -1, 1));
        //_animator.SetFloat("Vertical", Mathf.Clamp(localVel.z, -1, 1));
        //_me.Horizontal = Mathf.Clamp(localVel.x, -1, 1);
        //_me.Vertical = Mathf.Clamp(localVel.z, -1, 1);

        if (_me.InFOV(_pj))
        {
            behaviour = OnViewPlayer;
        }
    }

    public void OnNullPath()
    {
        behaviour = SearchLastPlayerPosition;
    }

    public void SearchLastPlayerPosition()
    {
        Vector3 position = new Vector3(_point.x, _me.transform.position.y, _point.z);
        Vector3 dir = position - _me.transform.position;
        //_behaviorAvoidance.GetAvoidanceDirection(dir.normalized);
        Vector3 finalDir = _behaviorAvoidance.GetAvoidanceDirection(dir.normalized);
        _me.Move(finalDir);

        Quaternion targetRot = Quaternion.LookRotation(position - _me.transform.position);
        _me.transform.rotation = Quaternion.Slerp(
            _me.transform.rotation,
            targetRot,
            _rotationSpeed * Time.deltaTime);

        if (_me.InFOV(_pj))
        {
            behaviour = OnViewPlayer;
        }
    }


    public void GetPlayerPosition(Vector3 player)
    {
        _pj = player;

    }
    public void SetPoint(Vector3 position)
    {
        _point = position;
    }
}


