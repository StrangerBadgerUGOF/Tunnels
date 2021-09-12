using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    #region Variables

    // Mouse sensitivity
    [SerializeField] private float _mouseSensitivity = 10;
    // Target to follow
    [SerializeField] private Transform _target;
    // Distance to keep from target
    [SerializeField] private float _distanceFromTarget = 2;
    // Pitch minimum and maximum
    [SerializeField] private Vector2 _pitchMinMax = new Vector2(-40, 85);

    // Smooth for rotation
    [SerializeField] private float _rotationSmoothTime = 0.12f;
    private Vector3 _rotationSmoothVelocity;
    private Vector3 currentRotation;

    // Left or right
    private float _yaw;
    // Up or down
    private float _pitch;

    #endregion

    #region Unity

    // Update is called once per frame
    void LateUpdate()
    {
        // Getting player input
        _yaw += Input.GetAxis("Mouse X") * _mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, _pitchMinMax.x, _pitchMinMax.y);

        // Smoothing rotation
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(_pitch, _yaw), ref _rotationSmoothVelocity, _rotationSmoothTime);
        // Setting rotation vector
        transform.eulerAngles = currentRotation;

        // Set camera position
        transform.position = _target.position - transform.forward * _distanceFromTarget;
    }

    #endregion
}
