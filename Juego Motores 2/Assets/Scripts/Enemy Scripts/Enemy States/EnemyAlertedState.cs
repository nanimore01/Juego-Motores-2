
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAlertedState : IState
{
    EnemyBasic _me;
    FSM _fsm;
    EnemyStats _stats;
    Rigidbody _rb;
    VoiceLines _voiceLines;

    float _rotationSpeed;
    Vector3 _pj;
    Vector3 _point;
    Animator _animator;

    UnityAction Update;
    AudioSource _audioSource;


    BehaviorAvoidance _behaviorAvoidance;
    BehaviourOnPath _behaviourOnPath;
    public EnemyAlertedState(EnemyBasic me, FSM fsm, EnemyStats stats, VoiceLines voiceLines)
    {
        _me = me;
        _fsm = fsm;
        _stats = stats;
        _voiceLines = voiceLines;

        _animator = _stats.animator;
        _rotationSpeed = _stats.rotationSpeed;

        _rb = _me._rb;
        _audioSource = _me.gameObject.GetComponent<AudioSource>();

        _behaviorAvoidance = new BehaviorAvoidance(me.transform,me);
        _behaviourOnPath = new BehaviourOnPath(me.transform);

        EventManager.player.PlayerPosition += GetPlayerPosition;
        EventManager.player.OnLastPositionHeard += SetPoint;
    }

    public void OnEnter()
    {
        Debug.Log("Alerted Mode");
        
        EventManager.player.OnLastPositionHeard += SetPoint;
        EventManager.player.PlayerPosition += GetPlayerPosition;

        _me.OnHeardPlayer += OnHeardPlayer;

        //_me.SetPath(Pathfinding.CalculateThetaStar(Pathfinding.GetMinNode(_me.transform.position), Pathfinding.GetMinNode(_point)));
        DebugPrint.ConsecutiveLog("Punto de sonido: " + _point);
        _behaviourOnPath.SetPath(Pathfinding.CalculateThetaStar(Pathfinding.GetMinNode(_me.transform.position), Pathfinding.GetMinNode(_point)));

        

        _behaviourOnPath.OnFinishedPath += StopedPath;
        Update = OnPath;
        //_behaviorAvoidance.OnGetStuck = WallDetected;
    }

    public void OnExit()
    {
        EventManager.player.OnLastPositionHeard -= SetPoint;
        EventManager.player.PlayerPosition -= GetPlayerPosition;
        _me.OnHeardPlayer -= OnHeardPlayer;
    }

    public void OnUpdate()
    {
        Update.Invoke();
        Vector3 localVel = _me.transform.InverseTransformDirection(_rb.velocity);
        _animator.SetFloat("Horizontal", Mathf.Clamp(localVel.x, -1, 1));
        _animator.SetFloat("Vertical", Mathf.Clamp(localVel.z, -1, 1));
    }

    public void OnPath()
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
        }

        
        //if (_me.path.Count == 0)
        //{
        //    Update = OnFinished;
        //}
    }

    public void StopedPath()
    {
        Update = OnFinished;
    }

    public void OnFinished()
    {
        Vector3 position = new Vector3(_point.x, _me.transform.position.y, _point.z);
        var dir = position - _me.transform.position;

        Vector3 finalDir = _behaviorAvoidance.GetAvoidanceDirection(dir.normalized);
        Debug.DrawRay(_me.transform.position, finalDir,Color.red, 1f);
        DebugPrint.ConsecutiveLog("Direccion Final: " + finalDir);
        if (finalDir.sqrMagnitude > .01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(finalDir);
            _me.transform.rotation = Quaternion.Slerp(
                _me.transform.rotation,
                targetRot,
                _rotationSpeed * Time.deltaTime
            );
        }

        //Debug.Log("Direccion Final: " + finalDir);

        if (dir.magnitude <= 1f)
        {
            DebugPrint.ConsecutiveLog("Llegue al ruido");
            _fsm.ChangeState("Inspect");
        }

        _me.Move(finalDir);
    }

    public void GetPlayerPosition(Vector3 player)
    {
        _pj = player;
    }

    public void OnHeardPlayer()
    {
        if (Pathfinding.InLineOfSight(_me.transform.position, _pj))
        {
            OnSpotPlayer();
        }
        else
        {
            _audioSource.PlayOneShot(_voiceLines.onHeardASound);
            _fsm.ChangeState("Sound Heard");
        }
    }

    public void SetPoint(Vector3 position)
    {
        _point = position;
    }

    public void OnSpotPlayer()
    {
        _fsm.ChangeState("Attack");
    }
}

