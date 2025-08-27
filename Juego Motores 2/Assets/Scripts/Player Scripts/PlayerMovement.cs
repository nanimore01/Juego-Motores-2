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
    [SerializeField] float _normalSpeed, _walkSpeed;
    [SerializeField] float dampingFactor = 10f;

    [SerializeField] AudioSource audioSource;

    [SerializeField] AudioClip _runSound;

    IState currentState;
    public void Awake()
    {
        _pj = gameObject.GetComponent<FirstPersonPlayer>();
        _rb = gameObject.GetComponent<Rigidbody>();

        EventManager.player.OnStartRunning += RunningSound;
        EventManager.player.OnStopRunning += OnStop;


        ChangeState(new PlayerStopState(this));
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

        bool isMoving = (_horizontalInput != 0 || _verticalInput != 0);
        //print(_horizontalInput + "," + _verticalInput);

        if (!isMoving)
        {
            if (!(currentState is PlayerStopState))
                ChangeState(new PlayerStopState(this));
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            _movementSpeed = _walkSpeed;
            if (!(currentState is PlayerWalkState))
                ChangeState(new PlayerWalkState(this));
        }
        else
        {
            _movementSpeed = _normalSpeed;
            if (!(currentState is PlayerRunState))
                ChangeState(new PlayerRunState(this));
        }

        currentState?.OnUpdate();
        EventManager.player.PlayerPosition(transform.position);
    }

    

    public void FixedUpdateAction()
    {
        Movement(_horizontalInput, _verticalInput);
    }


    public void Movement(float moveHorizontal, float moveVertical)
    {
        Vector3 movement = (transform.forward * moveVertical + transform.right * moveHorizontal).normalized;
        
        _rb.AddForce(movement.normalized * _movementSpeed * 10f, ForceMode.Force);
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

    public void RunningSound()
    {
        audioSource.loop = true;
        audioSource.clip = _runSound;
        audioSource.Play();
    }

    public void OnStop()
    {
        audioSource.loop = false;
        audioSource.Stop();
    }

    void ChangeState(IState newState)
    {
        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }
}
