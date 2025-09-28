using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMuzzleFlash : MonoBehaviour
{
    [SerializeField] GameObject _weapon;
    [SerializeField] ParticleSystem _particleSystem;



    public IGet<IWeapon> _getWeapon;
    IWeapon _stats => _getWeapon.Get();
    public void Awake()
    {
        _getWeapon = _weapon.GetComponent<IGet<IWeapon>>();
    }

    public void OnEnable()
    {
        _stats.OnShot += PlayParticule;
    }

    public void OnDisable()
    {
        _stats.OnShot -= PlayParticule;
    }

    public void PlayParticule()
    {
        print("Saco particulas");
        _particleSystem.Play();
    }
    
}
