using UnityEngine;
using System;
public interface IWeapon 
{
    public event Action OnReloading;
    public event Action OnShot;
    public event Action<int, int> OnAmmoUsed;

}
