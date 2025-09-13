using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : IState
{
    PlayerMovement _me;


    public PlayerRunState(PlayerMovement me)
    {
        _me = me;
    }

    public void OnEnter()
    {
        Debug.Log("Start Running");
        EventManager.player.OnStartRunning?.Invoke();
        
    }

    public void OnExit()
    {
        EventManager.player.OnStopRunning?.Invoke();
        EventManager.player.OnLastPositionHeard?.Invoke(_me.transform.position);
    }

    public void OnUpdate()
    {
        EventManager.player.OnRun?.Invoke();
        
    }
}
