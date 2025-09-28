using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BehaviorAvoidance 
{
    float _avoidanceDistance => _stats.avoidanceDistance;
    float _avoidanceStrength => _stats.avoidanceStrength;
    LayerMask _layerMask => _stats.wallLayer; 

    Transform _me;
    IGet<EnemyStats> _getStats;
    EnemyStats _stats => _getStats.Get();
    //public Action OnGetStuck = delegate { };

    public BehaviorAvoidance(Transform me, IGet<EnemyStats> stats)
    {
        _me = me;
        _getStats = stats;
    }

    public Vector3 GetAvoidanceDirection(Vector3 desiredDir)
    {
        //Debug.Log("AvoidanceDirection Funciona");
        desiredDir.Normalize();
        Vector3 origin = _me.transform.position + Vector3.up * 0.6f;
        float rayDistance = 2f;

        // centro
        if (Physics.SphereCast(origin, _avoidanceDistance, desiredDir, out RaycastHit hit, rayDistance, _layerMask))
        {
            // elegimos un offset lateral (cross con up = vector perpendicular en plano XZ)
            Vector3 lateral = Vector3.Cross(Vector3.up, desiredDir).normalized;

            // decidir izquierda o derecha según cuál esté libre
            bool leftClear = !Physics.Raycast(origin, -lateral, 1f, _layerMask);
            bool rightClear = !Physics.Raycast(origin, lateral, 1f, _layerMask);

            if (rightClear && !leftClear)
            {
                Debug.Log("AvoidanceDirection Funciona voy a la izquierda");
                return (desiredDir + lateral * _avoidanceStrength).normalized;
            }

            if (leftClear && !rightClear) 
            {
                Debug.Log("AvoidanceDirection Funciona voy a la Derecha");
                return (desiredDir - lateral * _avoidanceStrength).normalized;
            }

            if (leftClear && rightClear) return (desiredDir + lateral * _avoidanceStrength).normalized;

            Debug.DrawRay(origin, desiredDir * rayDistance, Color.red);
            Debug.DrawRay(origin, lateral, Color.green);
            Debug.DrawRay(origin, -lateral, Color.blue);

            return Vector3.zero;

            
        }

        

        return desiredDir;
    }

    
}

public interface IGet<out T> 
{
    T Get();
}