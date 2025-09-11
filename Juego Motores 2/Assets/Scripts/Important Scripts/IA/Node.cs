using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
public class Node : MonoBehaviour
{
    public List<Node> neighbour = new List<Node>();
    public int heuristic = 1;
    public int cost = 1;
    public bool isPath;

    public bool CanContinue;

    CountdownTimer _StopTime;

    public UnityEvent OnNextPatrolNode;
    private void Awake()
    {
        _StopTime = new CountdownTimer(1);

        //neighbour = Physics.OverlapSphere(transform.position, 15f).Select(x => x.GetComponent<Node>()).Where(x => x != null).
        //Where(x => x.gameObject != gameObject).ToList();
    }

    public void Update()
    {
        _StopTime.Tick(Time.deltaTime);
    }

    public void StopTime(float time)
    {
        print("Me activo");
        _StopTime.Reset(time);

        _StopTime.OnTimerStart += DesactivatePath;
        _StopTime.Start();
        
        _StopTime.OnTimerStop += ActivatePath;
    }

    public void DesactivatePath()
    {
        CanContinue = false;
    }

    public void ActivatePath()
    {
        CanContinue = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isPath ? Color.green : Color.red;

        Gizmos.DrawSphere(transform.position, 0.1f);

        Gizmos.color = Color.blue;

        foreach (var node in neighbour)
        {
            Gizmos.DrawLine(transform.position, node.transform.position);
        }
    }
}
