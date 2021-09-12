using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables

    // Walk speed
    [SerializeField] private float _walkSpeed;

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
        // Getting input
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // Making input normalized
        Vector2 inputDir = input.normalized;

        // Check rotation
        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            transform.eulerAngles = Vector3.up 
                * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation,ref _turnSmoothVelocity, _turnSmoothTime);
        }

        // Getting target speed
        float targetSpeed = _walkSpeed * inputDir.magnitude;
        // Getting current speed
        _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedSmoothVelocity, _speedSmoothTime);

        // Move character
        transform.Translate(transform.forward * _currentSpeed * Time.deltaTime, Space.World);
    }

    #endregion
}
