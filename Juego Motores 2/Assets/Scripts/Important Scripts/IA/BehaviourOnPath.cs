using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class BehaviourOnPath
{
    Transform _me;
    public List<Node> path;

    Vector3 _axis;

    public Vector3 dir;

    public UnityAction OnFinishedPath;

    public BehaviourOnPath(Transform me, List<Node> path)
    {
        _me = me;
        this.path = path;
    }

    public void SetAxis(Vector3 axis)
    {
        _axis = axis;
    }

    public void PathBehaviour()
    {
        dir = _axis - _me.transform.position;

        if (path.Count > 0)
        {
            if (dir.magnitude <= 1f)
            {
                DebugPrint.ConsecutiveLog("Choque con el nodo");
                path.RemoveAt(0);
            }
        }

        if (path.Count == 0)
        {
            OnFinishedPath?.Invoke();
        }
    }

    public void ChangePath(List<Node> path)
    {
        this.path = path;
    }

}
