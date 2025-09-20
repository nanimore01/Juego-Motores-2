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

    Vector3 originalPos;
    Vector3 Newpos;
    Quaternion originalRot;
    Quaternion newRot;

    public float speedRecover;

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

    [SerializeField] protected Action UpdateBehavior;
    public void Awake()
    {
        //_pj = gameObject.GetComponentInParent<FirstPersonPlayer>();
        _shotTimer = new CountdownTimer(_shotTime);
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
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

        UpdateBehavior.Invoke();
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

    public void WeaponModelRecoil(float recoil)
    {
        Newpos = new Vector3(0, 0, -recoil);
        if (transform.localPosition.z > originalPos.z - 0.2f)
        {
            transform.localPosition += Newpos;
        }


        if(!IsSubscribed(UpdateBehavior, ReturnWeaponPosition))
        {
            UpdateBehavior += ReturnWeaponPosition;
        }
    }

    public void WeaponRotation(float recoil)
    {
        newRot = Quaternion.Euler(originalRot.eulerAngles.x - recoil, originalRot.eulerAngles.y, originalRot.eulerAngles.z);
        transform.localRotation = newRot;

        if (!IsSubscribed(UpdateBehavior, ReturnWeaponPosition))
        {
            UpdateBehavior += ReturnWeaponPosition;
        }
    }

    public void ReturnWeaponPosition()
    {
        if (transform.localPosition.z < originalPos.z)
        {
            transform.localPosition -= Newpos * Time.deltaTime * speedRecover;
        }

        if (transform.localRotation.x <= originalRot.x)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRot, Time.deltaTime * speedRecover * 10);
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
    public bool IsSubscribed(Action evento, Action metodo)
    {
        if (evento == null) return false;

        foreach (var d in evento.GetInvocationList())
        {
            if (d.Method == metodo.Method && d.Target == metodo.Target)
                return true;
        }

        return false;
    }
}




