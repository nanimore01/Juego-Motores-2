
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
    Vector3 avoidance = Vector3.zero;
    float _avoidanceStrength = 1f;
    float _avoidanceDistance = 2f;
    float _spreadAngle = 30f;
    public EnemyAlertedState(EnemyBasic me, FSM fsm, EnemyStats stats, VoiceLines voiceLines)
    {
        _me = me;
        _fsm = fsm;
        _stats = stats;
        _voiceLines = voiceLines;

        _animator = _stats.animator;
        _rotationSpeed = _stats.rotationSpeed;
        _avoidanceDistance = _stats.avoidanceDistance;
        _avoidanceStrength = _stats.avoidanceStrength;
        _spreadAngle = _stats.spreadAngle;

        _rb = _me.gameObject.GetComponent<Rigidbody>();
        _audioSource = _me.gameObject.GetComponent<AudioSource>();

        EventManager.player.PlayerPosition += GetPlayerPosition;
        EventManager.player.OnLastPositionHeard += SetPoint;
    }

    public void OnEnter()
    {
        Debug.Log("Alerted Mode");
        
        
        EventManager.player.OnLastPositionHeard += SetPoint;
        EventManager.player.PlayerPosition += GetPlayerPosition;
        Update = OnPath;
        _me.SetPath(_me.CalculateThetaStar(_me.GetMinNode(_me.transform.position), _me.GetMinNode(_point)));
        Debug.Log("Punto de sonido: " + _point);
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
        if (_me.path == null || _me.path.Count == 0 || _me.InLineOfSight(_me.transform.position, _point))
        {
            Update = OnFinished;
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
        _animator.SetFloat("Horizontal", Mathf.Clamp(localVel.x, -1, 1));
        _animator.SetFloat("Vertical", Mathf.Clamp(localVel.z, -1, 1));
        _me.Horizontal = Mathf.Clamp(localVel.x, -1, 1);
        _me.Vertical = Mathf.Clamp(localVel.z, -1, 1);

        if (_me.path.Count == 0)
        {
            Update = OnFinished;
        }
    }

    public void OnFinished()
    {
        Vector3 position = new Vector3(_point.x, _me.transform.position.y, _point.z);
        var dir = position - _me.transform.position;

        Vector3 finalDir = GetAvoidanceDirection(dir.normalized);

        if (finalDir.sqrMagnitude > .01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(finalDir);
            _me.transform.rotation = Quaternion.Slerp(
                _me.transform.rotation,
                targetRot,
                _rotationSpeed * Time.deltaTime
            );
        }

        if (dir.magnitude <= 1f)
        {
            Debug.Log("Llegue al ruido");
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
        if (_me.InLineOfSight(_me.transform.position, _pj))
        {
            OnSpotPlayer();
        }
        else
        {
            _audioSource.PlayOneShot(_voiceLines.onHeardASound);
            _fsm.ChangeState("Sound Heard");
        }
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

    public void SetPoint(Vector3 position)
    {
        _point = position;
    }

    public void OnSpotPlayer()
    {
        Debug.Log("Te detecte");
    }
}

