using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringBehaviors
{
    Transform _me;

    SteeringStats _stats;

    Vector3 _velocity;
    float _maxVelocity => _stats.maxVelocity;
    float _maxForce => _stats.maxForce;
    float _separationRadius => _stats.separationRadius;

    public SteeringBehaviors(SteeringStats stats, Transform transform)
    {
        _me = transform;
        _stats = stats;
    }

    /// <summary>
    /// <para> [ES]Aplica una fuerza (dirección) al movimiento actual. </para>
    /// <para> [EN]Apply a force (direction) to the current movement. </para>
    /// </summary>
    public void AddForce(Vector3 dir)
    {
        _velocity += dir;

        _velocity = Vector3.ClampMagnitude(_velocity, _maxVelocity);
    }

    public void MoveForward()
    {
        _me.position += _velocity * Time.deltaTime;
        _me.forward = _velocity;
    }

    public void Move()
    {
        _me.position += _velocity * Time.deltaTime;
    }
    /// <summary>
    /// <para>[EN] Moves the agent toward a target position.</para>
    /// <para>[ES] Mueve al agente hacia una posición objetivo.</para>
    /// </summary>
    public Vector3 Seek(Vector3 dir)
    {
        var desired = dir - _me.position;
        desired.Normalize();
        desired *= _maxVelocity;

        var steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce);

        return steering;
    }

    public Vector3 Arrive(Vector3 dir)
    {
        float dist = Vector3.Distance(_me.position, dir);

        if (dist > _separationRadius)
            return Seek(dir);

        var desired = dir - _me.position;
        desired.Normalize();
        desired *= (_maxVelocity * (dist / _separationRadius));

        var steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce);

        return steering;
    }

    public Vector3 Separation<T>(List<T> objects, float radius, System.Func<T, Vector3> getPosition)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (var obj in objects)
        {
            Vector3 pos = getPosition(obj);
            Vector3 dir = pos - _me.position;
            float dist = dir.magnitude;

            if (dist < radius && dist > 0f)
            {
                desired -= dir / dist;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        desired /= count;
        desired.Normalize();
        desired *= _maxVelocity;

        return CalculateSteering(desired);
    }

    public Vector3 CalculateSteering(Vector3 desired)
    {
        var steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce);

        return steering;
    }
    public Vector3 Pursue(Transform target, Vector3 targetVelocity)
    {
        Vector3 toTarget = target.position - _me.position;

        float distance = toTarget.magnitude;

        float predictionTime = distance / (_maxVelocity + 0.0001f);

        Vector3 futurePosition = target.position + targetVelocity * predictionTime;

        return Seek(futurePosition);
    }

    Vector3 Flee(Vector3 dir)
    {
        return -Seek(dir);
    }
}
[System.Serializable]
public struct SteeringStats
{
    public float maxVelocity;
    public float maxForce;

    public float separationRadius;

    [Range(0f, 1f)] public float seekWeight;
    [Range(0f, 1f)] public float separationWeight;
    [Range(0f, 1f)] public float pursueWeight;

}
