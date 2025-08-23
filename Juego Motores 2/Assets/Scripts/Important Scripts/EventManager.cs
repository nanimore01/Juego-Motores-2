using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
public static class EventManager
{
    public static readonly PlayerEvents player = new PlayerEvents();
    public static readonly EnemyEvents enemy = new EnemyEvents();

    public class EnemyEvents
    {
        public UnityAction OnDead;
    }

    public class PlayerEvents
    {
        public UnityAction OnStartWalking;

        public UnityAction OnRun;
        public UnityAction OnWalking;

        public UnityAction OnStopRunning;
    }

    

}
