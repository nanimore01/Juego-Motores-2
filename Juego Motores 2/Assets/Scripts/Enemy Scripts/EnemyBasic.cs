using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBasic : Entity
{
    private FSM _fsm;
    [SerializeField] EnemyStats stats;
    [SerializeField] VoiceLines voiceLines;
    public GameObject POV;
    [SerializeField] protected LayerMask _wallLayer;

    [SerializeField] float _viewRadius;
    [SerializeField] float _viewAngle, _hearRadius;
    public UnityAction<Vector3> OnHeardPlayer;

    public void Awake()
    {
        _currHp = _maxHP;

        _fsm = new FSM();

        EventManager.player.OnLastPositionHeard += Audition;
    }

    

    public void Audition(Vector3 playerPosition)  
    {
        var dir = playerPosition - transform.position;

        if(dir.magnitude < _hearRadius)
        {
            OnHeardPlayer?.Invoke(playerPosition);
        }
    }

    public void DisableAudition()
    {

    }


    public bool InLineOfSight(Vector3 start, Vector3 end)
    {
        var dir = end - start;

        return !Physics.Raycast(start, dir, dir.magnitude, _wallLayer);
    }

    public bool InFOV(Vector3 obj)
    {
        var dir = obj - transform.position;

        if (dir.magnitude < _viewRadius)
        {
            if (Vector3.Angle(transform.forward, dir) <= _viewAngle * 0.5f)
            {
                return InLineOfSight(POV.transform.position, obj);
            }
        }

        return false;
    }
}

[System.Serializable]
public struct EnemyStats
{
    public float maxForce;
    public float maxVelocity;
    public float hearRadius;
    public Node[] nodePatrol;
}

[System.Serializable]
public struct VoiceLines
{

}
