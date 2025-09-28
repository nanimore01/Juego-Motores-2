using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class WeaponAmmoText : MonoBehaviour
{
    public IGet<IWeapon> _getWeapon;
    IWeapon _stats => _getWeapon.Get();

    [SerializeField] TMP_Text _text;

    [SerializeField] GameObject _weapon;

    public void Awake()
    {
        _getWeapon = _weapon.GetComponent<IGet<IWeapon>>();
    }

    public void SetNewWeapon(IGet<IWeapon> weapon)
    {
        _getWeapon = weapon;
    }

    public void OnEnable()
    {
        _stats.OnAmmoUsed += UpdateText;
    }

    public void OnDisable()
    {
        _stats.OnAmmoUsed -= UpdateText;
    }

    public void UpdateText(int actualAmmo, int totalAmmoStash)
    {
        _text.text = actualAmmo + "/" + totalAmmoStash;
    }
}
