
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
    }

    public void OnUpdate()
    {
        Update.Invoke();
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

        Vector3 localVel = _me.transform.InverseTransformDirection(_rb.velocity);
        _animator.SetFloat("Horizontal", Mathf.Clamp(localVel.x, -1, 1));
        _animator.SetFloat("Vertical", Mathf.Clamp(localVel.z, -1, 1));
        _me.Horizontal = Mathf.Clamp(localVel.x, -1, 1);
        _me.Vertical = Mathf.Clamp(localVel.z, -1, 1);

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

        Vector3 localVel = _me.transform.InverseTransformDirection(_rb.velocity);
        _animator.SetFloat("Horizontal", Mathf.Clamp(localVel.x, -1, 1));
        _animator.SetFloat("Vertical", Mathf.Clamp(localVel.z, -1, 1));
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

    //Vector3 GetAvoidanceDirection(Vector3 desiredDir)
    //{
    //    if (desiredDir.sqrMagnitude < 0.0001f)
    //        return Vector3.zero;

    //    desiredDir.Normalize();

    //    Vector3 origin = _me.transform.position + Vector3.up * 0.6f;
    //    float rayDistance = _avoidanceDistance;
    //    float spread = _spreadAngle;

    //    // definimos las 3 direcciones
    //    Vector3 center = desiredDir;
    //    Vector3 left = Quaternion.Euler(0, -spread, 0) * desiredDir;
    //    Vector3 right = Quaternion.Euler(0, spread, 0) * desiredDir;

    //    // --- Raycast central ---
    //    if (Physics.Raycast(origin, center, rayDistance))
    //    {
    //        bool leftClear = !Physics.Raycast(origin, left, rayDistance);
    //        bool rightClear = !Physics.Raycast(origin, right, rayDistance);

    //        if (leftClear && !rightClear) return (desiredDir + left * 0.7f).normalized;
    //        if (rightClear && !leftClear) return (desiredDir + right * 0.7f).normalized;
    //        if (leftClear && rightClear)
    //        {
    //            // elige el m�s alineado con la direcci�n deseada
    //            float dotL = Vector3.Dot(desiredDir, left);
    //            float dotR = Vector3.Dot(desiredDir, right);
    //            return (desiredDir + (dotL > dotR ? left : right) * 0.7f).normalized;
    //        }

    //        OnDetectWall?.Invoke();

    //        return Vector3.zero;
    //    }

    //    // --- si no hay nada enfrente, segu� normal ---
    //    return desiredDir;

    //    Debug.DrawRay(origin, center * rayDistance, Color.red);
    //    Debug.DrawRay(origin, left * rayDistance, Color.yellow);
    //    Debug.DrawRay(origin, right * rayDistance, Color.yellow);

    //}

    public void WallDetected()
    {
        DebugPrint.ConsecutiveLog("WallDetected Funciona");
        _me.SetPath(Pathfinding.CalculateThetaStar(Pathfinding.GetMinNode(_me.transform.position), Pathfinding.GetMinNode(_point)));
        Update = OnPath;
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

