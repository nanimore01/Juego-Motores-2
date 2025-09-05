using UnityEngine;
using UnityEngine.Events;
using System;
public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [SerializeField]Animator _animator;

    [SerializeField] FirstPersonPlayer _pj;
    [SerializeField] protected LayerMask _shotableMask;

    public bool isReloading, _canShot;

    [SerializeField] protected float _shotTime;
    [SerializeField] protected int _ammoPerMag;
    [SerializeField] protected int _actualMag;
    [SerializeField] protected int _totalAmmoStash;

    public CountdownTimer _shotTimer;

    public event Action OnReloading;
    public event Action OnShot;
    public event Action<int, int> OnAmmoUsed;

    [SerializeField] protected UnityEvent OnClickUp;
    [SerializeField] protected UnityEvent OnClickDown;
    [SerializeField] protected UnityEvent OnClick;
    [SerializeField] protected UnityEvent OnClickRightUp;
    [SerializeField] protected UnityEvent OnClickRightDown;
    [SerializeField] protected UnityEvent OnClickRight;

    public void Awake()
    {
        //_pj = gameObject.GetComponentInParent<FirstPersonPlayer>();
        _shotTimer = new CountdownTimer(_shotTime);
    }
    public void Start()
    {
        _shotTimer.OnTimerStart += CooldownOn;
        _shotTimer.OnTimerStop += CooldownOff;

        _canShot = true;
    }

    public void Update()
    {
        _shotTimer.Tick(Time.deltaTime);

    }
    public void OnClickUpBehavior()
    {
        if(!isReloading && _canShot)
        {
            OnClickUp?.Invoke();
        }
    }
    public void OnClickDownBehavior()
    {
        if (!isReloading && _canShot)
        {
            OnClickDown?.Invoke();
        }
        print("Funciono");
    }
    public void OnClickBehavior()
    {
        if (!isReloading && _canShot)
        {
            OnClick?.Invoke();
        }
        
    }
    public void OnRightClickBehavior()
    {
        if (!isReloading && _canShot)
        {
            OnClickRight?.Invoke();
        }
    }
    public void OnRightClickUpBehavior()
    {
        if (!isReloading && _canShot)
        {
            OnClickRightDown?.Invoke();
        }
    }

    public void OnRightClickDownBeheavior()
    {
        if (!isReloading && _canShot)
        {
            OnClickRightUp?.Invoke();
        }
    }
    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    public void ShotCooldown()
    {
        _canShot = !_canShot;
        _shotTimer.Reset();
        OnShot?.Invoke();
        
    }

    public void CooldownOn()
    {
        _canShot = false;
        _shotTimer.Start();
    }

    public void CooldownOff()
    {
        _canShot = true;
        _shotTimer.Reset();
    }

    public void WeaponReloadBehavior()
    {
        if (_actualMag <= 0)
        {
            ReloadAnimation();
        }
        else
        {
            _actualMag--;
            OnAmmoUsed?.Invoke(_actualMag, _totalAmmoStash);
            _shotTimer.Start();
        }
    }

    public bool PointingOnLayerMask(LayerMask layer)
    {
        return layer == _shotableMask;
    }

    public void ReloadSound()
    {

    }

    public virtual void ReloadAnimation()
    {
        if (_totalAmmoStash > 0 && _actualMag < _ammoPerMag)
        {
            _animator.SetTrigger("Reload");
            isReloading = true;
        }
        else
        {
            if (_totalAmmoStash <= 0)
            {
                print($"No ammo left!");
                _canShot = false;
            }
        }
            
    }

    public virtual void Reload()
    {
        //if (_totalAmmoStash > 0 && _actualMag < _ammoPerMag)
        //{
        //    int neededAmmo = _ammoPerMag - _actualMag;
        //    if (neededAmmo <= _totalAmmoStash)
        //    {
        //        _totalAmmoStash -= neededAmmo;
        //        _actualMag = _ammoPerMag;
        //    }
        //    else
        //    {
        //        _actualMag += _totalAmmoStash;
        //        _totalAmmoStash = 0;
        //    }
        //}
        //else
        //{
        //    if (_totalAmmoStash <= 0)
        //    {
        //        print($"No ammo left!");
        //        _canShot = false;
        //    }
        //}
        int neededAmmo = _ammoPerMag - _actualMag;
        if (neededAmmo <= _totalAmmoStash)
        {
            _totalAmmoStash -= neededAmmo;
            _actualMag = _ammoPerMag;
        }
        else
        {
            _actualMag += _totalAmmoStash;
            _totalAmmoStash = 0;
        }
    }

}




