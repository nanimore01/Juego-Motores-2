using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStopState : IState
{
    PlayerMovement _me;


    public PlayerStopState(PlayerMovement me)
    {
        _me = me;
    }

    public void OnEnter()
    {
        Debug.Log("Idle");
        
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
        
    }
}
