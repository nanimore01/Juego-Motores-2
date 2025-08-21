using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class FirstPersonPlayer : Entity
{
    [SerializeField] Rigidbody _rb;

    
    private float _xAxis, _zAxis, _inputMouseX, _inputMouseY, _mouseX;
    [Range(1f, 500f)] [SerializeField] private float _mouseSensitivity = 100f;

    public UnityEvent NormalUpdate;
    public UnityEvent MovementFixedUpdate;

    public event Action<float, float> OnAxisMouseRequied;
    void Start()
    {
        _currHp = _maxHP;
        OnDied.AddListener(OnDead);
    }

    // Update is called once per frame
    void Update()
    {
        NormalUpdate?.Invoke();
        _inputMouseX = Input.GetAxisRaw("Mouse X");
        _inputMouseY = Input.GetAxisRaw("Mouse Y");

        if (_inputMouseX != 0 || _inputMouseY != 0)
        {
            Rotation(_inputMouseX, _inputMouseY);
        }
    }

    public void OnDead()
    {
        
    }

    private void Rotation(float xAxis, float yAxis)
    {
        _mouseX += xAxis * _mouseSensitivity * Time.deltaTime;

        if (_mouseX >= 360 || _mouseX <= -360)
        {
            _mouseX -= 360 * Mathf.Sign(_mouseX);
        }

        yAxis *= _mouseSensitivity * Time.deltaTime;

        transform.rotation = Quaternion.Euler(0, _mouseX, 0);
        OnAxisMouseRequied?.Invoke(_mouseX, yAxis);
    }
}
