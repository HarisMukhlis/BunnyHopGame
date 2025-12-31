using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSensitivity = 1f;
    [SerializeField] private float movementMax = 2f;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 5f;
    [Space]
    [SerializeField] private float airTimeDuration = 1f;
    [Space]
    [SerializeField] private float downtimeGravityScale = 1.2f;
    [SerializeField] private float downtimeGravityClamp = 2f;

    [Header("Ground Check")]
    [SerializeField] private float groundRaycastDistance = .2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Obstacle")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float obstacleRayDistance;

    [Header("Visual")]
    [SerializeField] private float maxTiltingAmount = 12f;
    [SerializeField] private float tiltingLerp = 1f;

    [Header("[DEBUG]")]
    [SerializeField] private bool isInvincible = false;

    private Rigidbody _rb;
    private float _sideMovementInput = 0f; //much like Input.GetAxis, returns from -1f to 1f horizontally
    private bool _isGrounded = false;
    private bool _isFalling = false;
    private bool _isJumping = false;

    private bool _isMoveable = true; //turn off when game over

    public float sideMovement
    {
        get
        {
            return Mathf.LerpUnclamped(0f, movementMax, Mathf.Clamp(_sideMovementInput * movementSensitivity, -1f, 1f));
        }
    }

    void Awake()
    {
        try
        {
            _rb = this.gameObject.GetComponent<Rigidbody>();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error at object {this.gameObject.name}, message {e}");
        }
    }

    void FixedUpdate()
    {
        GroundCheck();

        if (_isFalling)
            ApplyDownForce();

        if (!isInvincible)
            ObstacleCheck();
    }

    void Update()
    {
        // DebugInputs();
        TiltCharacter();
    }

    private void GroundCheck()
    {
        // RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = -transform.up;

        if (Physics.Raycast(rayOrigin, rayDirection, groundRaycastDistance, groundLayer))
        {
            _isGrounded = true;
            _isFalling = false;
        }
        else _isGrounded = false;
    }

    private void ObstacleCheck()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, obstacleRayDistance, obstacleLayer))
        {
            GameManager.Instance.GameOver();
        }
    }

    public void HandleSideInputs(float axisInput)
    {
        if (_isMoveable)
            _sideMovementInput = axisInput;
    }

    private void DebugInputs()
    {
        //debug inputs

        if (Input.GetKeyDown(KeyCode.Space) && _isMoveable)
        {
            Jump();
        }
        // _sideMovementInput = Input.GetAxis("Horizontal");
    }

    //handle jumping
    [ContextMenu("Do Jump")]
    public void Jump()
    {
        StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        if (_isGrounded && !_isJumping && _isMoveable)
        {
            _isJumping = true;

            _isFalling = false;
            _rb.AddForce(Vector3.up * jumpHeight);

            yield return new WaitForSeconds(airTimeDuration);

            _isJumping = false;

            if (!_isGrounded)
                _isFalling = true;
        }
    }

    private void ApplyDownForce()
    {
        _rb.AddForce(Vector3.down * downtimeGravityScale);

        if (_rb.linearVelocity.y < -downtimeGravityClamp)
        {
            Vector3 currentVelocity = _rb.linearVelocity;
            _rb.linearVelocity = new Vector3(currentVelocity.x, -downtimeGravityClamp, currentVelocity.z);
        }
    }

    public void DisableInputs()
    {
        _isMoveable = false;

        _sideMovementInput = 0f;
    }

    private void TiltCharacter()
    {
        bool tiltingRight = _sideMovementInput >= 0f; //if greater than 0, tilts right. otherwise tilt left

        float tiltingTarget = Mathf.LerpAngle(0f, maxTiltingAmount, Mathf.Abs(_sideMovementInput)) * (tiltingRight ? 1f : -1f);
        float tiltingAmount = Mathf.LerpAngle(transform.rotation.y, tiltingTarget, tiltingLerp);

        // Debug.Log($"Tilting amount {tiltingAmount} | Tilting target {tiltingTarget} | Side Movement {_sideMovementInput}");
        transform.localRotation = Quaternion.Euler(0f, tiltingAmount, 0f);
    }
}
