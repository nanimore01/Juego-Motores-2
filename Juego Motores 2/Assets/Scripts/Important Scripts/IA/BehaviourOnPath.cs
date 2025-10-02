using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class BehaviourOnPath
{
    Transform _me;
    List<Node> _path = new List<Node>();

    Vector3 _axis;

    public Vector3 dir;

    public UnityAction OnFinishedPath;

    public BehaviourOnPath(Transform me)
    {
        _me = me;
        //_axis = new Vector3(_path[0].transform.position.x, _me.transform.position.y, _path[0].transform.position.z);
    }

    public void SetAxis(Vector3 axis)
    {
        _axis = axis;
    }

    public void PathBehaviour()
    {
        if (_path?.Count == 0)
        {
            OnFinishedPath?.Invoke();
        }

        dir = _axis - _me.transform.position;

        if (_path?.Count > 0)
        {
            if (dir.magnitude <= 1f)
            {
                DebugPrint.ConsecutiveLog("Choque con el nodo");
                _path.RemoveAt(0);
            }
        }

        
    }

    public void SetPath(List<Node> newPath)
    {
        _path.Clear();


        _path.AddRange(newPath);

        _axis = new Vector3(_path[0].transform.position.x, _me.transform.position.y, _path[0].transform.position.z);
    }
}
