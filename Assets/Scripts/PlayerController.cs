using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables

    // Player rigidbody
    [SerializeField] private Rigidbody _playerRigidbody;

  
    [Header("Movement varaibles")]

    // Walk speed
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;

    // Jump force
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _fallMultiplier;

    // Turn smooth time
    [SerializeField] private float _turnSmoothTime = 0.2f;
    private float _turnSmoothVelocity;

    // Speed smooth
    [SerializeField] private float _speedSmoothTime = 0.1f;
    private float _speedSmoothVelocity;
    private float _currentSpeed;

    // Camera transform
    [SerializeField]private Transform _cameraTransform;

    #endregion

    #region Unity

    // Update is called once per frame
    void Update()
    {
        MovementInput();
    }

    // Fixed update is called for physics
    private void FixedUpdate()
    {
        PhysicsMovementCheck();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Movement input
    /// </summary>
    private void MovementInput()
    {
        // Getting input
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // Making input normalized
        Vector2 inputDir = input.normalized;

        // Check rotation
        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            transform.eulerAngles = Vector3.up
                * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref _turnSmoothVelocity, _turnSmoothTime);
        }

        // Running check
        bool isRunning = false;
        if (Input.GetKey(KeyCode.LeftShift) == true)
        {
            isRunning = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) == true)
        {
            _playerRigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }

        // Checking what type of movement we have 
        float currentSpeed = isRunning == true ? _runSpeed : _walkSpeed;
        // Getting target speed
        float targetSpeed = currentSpeed * inputDir.magnitude;
        // Getting current speed
        _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedSmoothVelocity, _speedSmoothTime);
    }

    /// <summary>
    /// Checks physics movement
    /// </summary>
    private void PhysicsMovementCheck()
    {
        // Move character
        _playerRigidbody.AddForce(transform.forward * _currentSpeed, ForceMode.Impulse);
        // Fall check 
        if (_playerRigidbody.velocity.y < 0)
        {
            _playerRigidbody.velocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.deltaTime;
        }
    }

    #endregion
}
