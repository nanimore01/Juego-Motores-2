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
    VoiceLines _voiceLines;

    float _rotationSpeed;

    CountdownTimer _inpectTimer;


    UnityAction update;

    Vector3 avoidance = Vector3.zero;
    float _avoidanceStrength = 1f;
    float _avoidanceDistance = 2f;
    float _spreadAngle = 30f;

    public EnemyInspectState(FSM fsm, EnemyBasic me, EnemyStats stats)
    {
        _fsm = fsm;
        _me = me;
        _stats = stats;

        _inpectTimer = new CountdownTimer(_stats.inpectTime);
        _animator = _stats.animator;
        _rotationSpeed = _stats.rotationSpeed;

        _rb = _me.gameObject.GetComponent<Rigidbody>();
        EventManager.player.PlayerPosition += GetPlayerPosition;
    }



    public void OnEnter()
    {
        Debug.Log("Inspect Mode");
        update = OnInspect;
        _inpectTimer.Start();
        _inpectTimer.OnTimerStop += OnStopInspect;

        EventManager.player.PlayerPosition += GetPlayerPosition;
        _me.OnHeardPlayer += OnHeardPlayer;
        _me.OnSpotedPlayer += OnSpotPlayer;
    }

    public void OnExit()
    {
        _me.OnHeardPlayer -= OnHeardPlayer;
        _me.OnSpotedPlayer -= OnSpotPlayer;
        EventManager.player.PlayerPosition -= GetPlayerPosition;
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
        _me.OnStopInspect.Invoke();
        _me.SetPath(_me.CalculateThetaStar(_me.GetMinNode(_me.transform.position), _me.GetMinNode(_stats.nodePatrol[0].transform.position)));
    }

    public void OnReturnPatrol()
    {
        if (_me.path == null || _me.path.Count == 0)
        {
            _fsm.ChangeState("Patrol");
            return;
        }

        Vector3 posNode = new Vector3(_me.path[0].transform.position.x, _me.transform.position.y, _me.path[0].transform.position.z);
        var dir = posNode - _me.transform.position;

        if (_me.path.Count > 0)
        {
            if (dir.magnitude <= 1f)
            {
                Debug.Log("Choque con el nodo");
                _me.path.RemoveAt(0);
            }
        }

        if (dir.sqrMagnitude > .01f)
        {
            Vector3 finalDir = GetAvoidanceDirection(dir.normalized);

            Quaternion targetRot = Quaternion.LookRotation(finalDir);
            _me.transform.rotation = Quaternion.Slerp(
                _me.transform.rotation,
                targetRot,
                _rotationSpeed * Time.deltaTime
            );

            _me.Move(finalDir);
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
        //_fsm.ChangeState("Sound Heard");

        if (_me.InLineOfSight(_me.transform.position, _pj))
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

    Vector3 GetAvoidanceDirection(Vector3 targetDir)
    {
        if (targetDir.sqrMagnitude < 0.0001f)
            return _me.transform.forward;

        //Vector3 avoidance = Vector3.zero;
        //float avoidanceStrength = 1f;
        //float rayDistance = 2f;
        //float spreadAngle = 30f;

        Vector3 origin = _me.transform.position + Vector3.up * 0.5f;
        RaycastHit hit;


        if (Physics.Raycast(origin, targetDir, out hit, _avoidanceDistance))
        {
            avoidance += Vector3.Reflect(targetDir, hit.normal) * _avoidanceStrength;
        }


        Vector3 rightDir = Quaternion.Euler(0, _spreadAngle, 0) * targetDir;
        if (Physics.Raycast(origin, rightDir, out hit, _avoidanceDistance))
        {
            avoidance += Vector3.Reflect(targetDir, hit.normal) * _avoidanceStrength * 0.9f;
        }


        Vector3 leftDir = Quaternion.Euler(0, -_spreadAngle, 0) * targetDir;
        if (Physics.Raycast(origin, leftDir, out hit, _avoidanceDistance))
        {
            avoidance += Vector3.Reflect(targetDir, hit.normal) * _avoidanceStrength * 0.9f;
        }

        // Debug
        Debug.DrawRay(origin, targetDir * _avoidanceDistance, Color.red);
        Debug.DrawRay(origin, rightDir * _avoidanceDistance, Color.yellow);
        Debug.DrawRay(origin, leftDir * _avoidanceDistance, Color.cyan);

        Vector3 combined = (targetDir + avoidance);
        return combined.sqrMagnitude > 0.0001f ? combined.normalized : targetDir;
    }
}
