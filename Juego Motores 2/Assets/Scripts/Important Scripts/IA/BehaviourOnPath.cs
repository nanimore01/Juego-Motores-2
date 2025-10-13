using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
public class BehaviourOnPath
{
    Transform _me;
    List<Node> _path = new List<Node>();
    
    Vector3 _axis;
    public Vector3 dir;


    public UnityAction OnFinishedPath;
    public Func<Vector3, Vector3, Vector3> AxisStrategy;
    public UnityAction OnNullPath;

    public BehaviourOnPath(Transform me)
    {
        _me = me;
        AxisStrategy = (mePos, target) => new Vector3(target.x, mePos.y, target.z);
    }

    public void PathBehaviour()
    {
        if(_path == null)
        {
            OnNullPath?.Invoke();
            return;
        }


        if ( _path.Count == 0)
        {
            OnFinishedPath?.Invoke();
            return;
        }

        Vector3 target = _path[0].transform.position;
        _axis = AxisStrategy(_me.position, target);

        dir = _axis - _me.position;

        if (dir.magnitude <= 1f)
        {
            DebugPrint.ConsecutiveLog("Choque con el nodo");
            _path.RemoveAt(0);
        }

        if (_path.Count == 0)
        {
            OnFinishedPath?.Invoke();
        }
    }
    public void SetPath(List<Node> newPath)
    {
        if (_path == null)
        {
            OnNullPath?.Invoke();
            return;
        }

        _path.Clear();
        _path.AddRange(newPath);
        
    }
}
