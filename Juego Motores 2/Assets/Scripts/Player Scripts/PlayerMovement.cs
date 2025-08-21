using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class PlayerMovement : MonoBehaviour
{

    float _horizontalInput;
    float _verticalInput;

    FirstPersonPlayer _pj;
    Rigidbody _rb;
    [SerializeField] float _movementSpeed;
    [SerializeField] float _normalSpeed;
    [SerializeField] float dampingFactor = 10f;


    float prueba;

    public event Action<float, float> OnPlayerMove;
    public event Action OnPlayerStoped;

    public void Awake()
    {
        _pj = gameObject.GetComponent<FirstPersonPlayer>();
        _rb = gameObject.GetComponent<Rigidbody>();


        OnPlayerMove += OnPlayerMoved;
        OnPlayerStoped += OnPlayerStop;
    }

    public void OnEnable()
    {
        _pj.MovementFixedUpdate.AddListener(FixedUpdateAction);
        _pj.NormalUpdate.AddListener(UpdateAction);
    }

    public void OnDisable()
    {
        _pj.MovementFixedUpdate.RemoveListener(FixedUpdateAction);
        _pj.NormalUpdate.RemoveListener(UpdateAction);
    }

    public void UpdateAction()
    {
        SpeedControl();
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
    }

    public void FixedUpdateAction()
    {
        Movement(_horizontalInput, _verticalInput);
    }


    public void Movement(float moveHorizontal, float moveVertical)
    {
        Vector3 movement = (transform.forward * moveVertical + transform.right * moveHorizontal).normalized;
        _movementSpeed = _normalSpeed;
        _rb.AddForce(movement.normalized * _movementSpeed * 10f, ForceMode.Force);

        if (moveHorizontal != 0 || moveVertical != 0)
        {
            OnPlayerMove?.Invoke(moveHorizontal, moveVertical);
        }
        else
            OnPlayerStoped?.Invoke();
    }

    public void OnPlayerMoved(float horizontalAxis, float verticalAxis)
    {

    }

    public void OnPlayerStop()
    {
        //
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        if (flatVel.magnitude > _movementSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * _movementSpeed;
            _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
        }
        else
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, new Vector3(0, _rb.velocity.y, 0), Time.deltaTime * dampingFactor);
        }
    }
}
