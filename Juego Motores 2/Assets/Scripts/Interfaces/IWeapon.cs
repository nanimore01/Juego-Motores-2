using UnityEngine;
using System;
public interface IWeapon 
{
    public event Action OnReload;
    public event Action OnShot;
    public event Action<int, int> OnAmmoUsed;

}
