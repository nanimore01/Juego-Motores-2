using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : IState
{
    public PlayerMovement _me;

    public PlayerWalkState(PlayerMovement me)
    {
        _me = me;
    }

    public void OnEnter()
    {
        Debug.Log("Start Walking");
        EventManager.player.OnStartWalking?.Invoke();
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
        
    }
}
