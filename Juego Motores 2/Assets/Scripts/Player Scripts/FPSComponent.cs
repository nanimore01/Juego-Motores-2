using UnityEngine;

public class FPSComponent : MonoBehaviour
{
    [SerializeField] WeaponBase[] Allweapons, weapons;
    int _currentIndexWeapon;
    int _previousIndexWeapon;
    float _mouseScrollWheel;
    [SerializeField] WeaponBase currentWeapon => weapons[_currentIndexWeapon];

    IFPPlayer _Ipj;
    FirstPersonPlayer _pj;

    public void Awake()
    {
        _pj = gameObject.GetComponent<FirstPersonPlayer>();
        _Ipj = _pj.GetComponent<IFPPlayer>();
    }

    public void OnEnable()
    {
        _pj.NormalUpdate.AddListener(Inputs);
    }

    public void OnDisable()
    {
        _pj.NormalUpdate.RemoveListener(Inputs);
    }

    public void Inputs()
    {
        if (Input.GetMouseButton(0))
            currentWeapon?.OnClickBehavior();

        if (Input.GetMouseButtonDown(0))
            currentWeapon?.OnClickDownBehavior();

        if (Input.GetMouseButtonUp(0))
            currentWeapon?.OnClickUpBehavior();

        if (Input.GetMouseButton(1))
            currentWeapon?.OnRightClickBehavior();

        if (Input.GetMouseButtonDown(1))
            currentWeapon?.OnRightClickDownBeheavior();

        if (Input.GetMouseButtonDown(1))
            currentWeapon?.OnRightClickUpBehavior();

        if(currentWeapon != null)
        {
            if (Input.GetKeyDown(KeyCode.R))
                currentWeapon.Reload();
        }

        
    }


    public void ChangeWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length || currentWeapon.isReloading) return;

        if (_currentIndexWeapon >= 0 && _currentIndexWeapon < weapons.Length)
        {
            weapons[_currentIndexWeapon].gameObject.SetActive(false);
        }

        _previousIndexWeapon = _currentIndexWeapon;

        _currentIndexWeapon = index;

        weapons[_currentIndexWeapon].gameObject.SetActive(true);
    }
}
