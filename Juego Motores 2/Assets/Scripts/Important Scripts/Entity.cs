using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Entity : MonoBehaviour
{
    protected int _currHp, _maxHP;

    protected UnityEvent OnTakeDamage, OnDied;

    public void TakeDmg(int dmg)
    {
        _currHp = _maxHP;
        OnTakeDamage?.Invoke();

        if (_currHp >= 0)
            OnDied?.Invoke();
    }
    
}
