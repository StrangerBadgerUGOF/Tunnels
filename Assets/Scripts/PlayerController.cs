using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterStance { Standing, Crouching }

public class PlayerController : MonoBehaviour
{
    #region Variables

    // Ray cast length
    private const float RAY_Z_SCALAR_INCREASE = 1000;

    // Camera transform
    [SerializeField] private Transform _cameraTransform;
    // Player rigidbody
    [SerializeField] private Rigidbody _playerRigidbody;
    // Player capsule collide
    [SerializeField] private CapsuleCollider _playerCollider;
    private Collider[] _obstructions = new Collider[8];
    // Player layer mask
    [SerializeField] private LayerMask _layerMask;

    // Speed in different states
    [Header("Speed (Normal, Sprinting)")]
    [SerializeField] private Vector2 _standingSpeed = new Vector2(0, 0);
    [SerializeField] private Vector2 _crouchingSpeed = new Vector2(0, 0);

    // Capsule values
    [Header("Capsule (Radius, Height, VOffset)")]
    [SerializeField] private Vector3 _standingCapsule = Vector3.zero;
    [SerializeField] private Vector3 _crouchingCapsule = Vector3.zero;

    [Header("Movement varaibles")]

    // Changing position speed
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    private CharacterStance _characterStance;

    // Jump force
    [SerializeField] private float _jumpForce;
    private bool _toJump;
    [SerializeField] private float _fallMultiplier;
    private bool _isOnFloor;

    // Strafe force
    [SerializeField] private float _strafeForce;
    private bool _toStrafe;

    // Turn smooth time
    [SerializeField] private float _turnSmoothTime = 0.2f;
    private float _turnSmoothVelocity;

    // Speed smooth
    [SerializeField] private float _speedSmoothTime = 0.1f;
    private float _speedSmoothVelocity;
    private float _currentSpeed;

    #endregion

    #region Unity

    // Start function
    private void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        // Initialize variables
        _walkSpeed = _standingSpeed.x;
        _runSpeed = _standingSpeed.y;
        _characterStance = CharacterStance.Standing;
        SetCapsuleDimensions(_standingCapsule);
        _isOnFloor = true;
        _toJump = _toStrafe = false;
        // Form mask
        int mask = 0;
        for (int i = 0; i < 32; i++)
        {
            if (!Physics.GetIgnoreLayerCollision(gameObject.layer, i))
            {
                mask |= 1 << i;
            }
        }
        _layerMask = mask;
    }

    // Update is called once per frame
    private void Update()
    {
        MovementInput();
        InteractionCheck();

       // For debugging
        Vector3 posToRayFrom, posToRayTo;
        posToRayFrom = Camera.main.transform.position;
        posToRayTo = Camera.main.transform.position + Camera.main.transform.forward * RAY_Z_SCALAR_INCREASE;
        Debug.DrawRay(posToRayFrom, posToRayTo, Color.red);
    }

    // Fixed update is called for physics
    private void FixedUpdate()
    {
        PhysicsMovementCheck();
    }

    // Late update
    private void LateUpdate()
    {
        StanceChangeInput();
    }

    // Check collision
    private void OnCollisionEnter(Collision collision)
    {
        _isOnFloor = true;
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

        // Jump check
        if (Input.GetKeyDown(KeyCode.Space) == true && 
            RequestStanceChange(CharacterStance.Standing) == true &&
            _isOnFloor == true)
        {
            _toJump = true;
            _isOnFloor = false;
        }

        // Strafe check
        if (Input.GetKeyDown(KeyCode.LeftControl) == true)
        {
            _toStrafe = true;
        }

        // Checking what type of movement we have 
        float currentSpeed = isRunning == true ? _runSpeed : _walkSpeed;
        // Getting target speed
        float targetSpeed = currentSpeed * inputDir.magnitude;
        // Getting current speed
        _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedSmoothVelocity, _speedSmoothTime);
    }

    /// <summary>
    /// Check interactable objects
    /// </summary>
    private void InteractionCheck()
    {
        // Checking if player clicked on field
        if (Input.GetMouseButtonDown(0) == true)
        {
            // Ray cast from camera
            RaycastHit checkRay = RayCastFromCamera();
            // Check raycast 
            if (checkRay.collider == null) return;
            // Check, if it is interactable object
            Interactable interactObject = checkRay.collider.gameObject.GetComponent<Interactable>();
            if (interactObject != null)
            {
                // Try to interact with object
                interactObject.TryInteract(transform);
            }
        }
    }

    /// <summary>
    /// Ray cast check
    /// </summary>
    /// <returns></returns>
    private RaycastHit RayCastFromCamera()
    {
        // Raycast from camera to point where it looks
        Vector3 posToRayFrom, posToRayTo;
        posToRayFrom = Camera.main.transform.position;
        posToRayTo = Camera.main.transform.position + Camera.main.transform.forward * RAY_Z_SCALAR_INCREASE;
        // Get all objects, which were on the path of raycast
        RaycastHit[] raycastHits = Physics.RaycastAll(posToRayFrom, posToRayTo);
        // Go through all objects, until you get non-player one
        for (int i = 0; i < raycastHits.Length; i++)
        {
            if (raycastHits[i].collider.gameObject != gameObject)
            {
                return raycastHits[i];
            }
        }
        return new RaycastHit();
    }

    /// <summary>
    /// Stance change input
    /// </summary>
    private void StanceChangeInput()
    {
        // Checking input
        if (Input.GetKey(KeyCode.C) == true)
        {
            // Crouching
            RequestStanceChange(CharacterStance.Crouching);
        }
        else 
        {
            // Standing
            RequestStanceChange(CharacterStance.Standing);
        }
    }

    /// <summary>
    /// Sends request, if it is possible to change player stance
    /// </summary>
    /// <param name="newStance">Requested stance</param>
    private bool RequestStanceChange(CharacterStance newStance)
    {
        // If the stance is the same as current one - return
        if (_characterStance == newStance) { return true; }
        // Check our recent stance
        switch (_characterStance)
        {
            case CharacterStance.Standing:
                if (newStance == CharacterStance.Crouching)
                {
                    if (!CharacterOverlap(_crouchingCapsule))
                    {
                        _walkSpeed = _crouchingSpeed.x;
                        _runSpeed = _crouchingSpeed.y;
                        _characterStance = newStance;
                        SetCapsuleDimensions(_crouchingCapsule);
                        return true;
                    }
                }
                break;
            case CharacterStance.Crouching:
                if (newStance == CharacterStance.Standing)
                {
                    if (!CharacterOverlap(_standingCapsule))
                    {
                        _walkSpeed = _standingSpeed.x;
                        _runSpeed = _standingSpeed.y;
                        _characterStance = newStance;
                        SetCapsuleDimensions(_standingCapsule);
                        return true;
                    }
                }
                break;
        }
        // Changing of stance is impossible - return false 
        return false;
    }

    /// <summary>
    /// Check, if character overlaps
    /// </summary>
    /// <param name="dimensions">Dimension of state/param>
    /// <returns></returns>
    private bool CharacterOverlap(Vector3 dimensions)
    {
        // Getting center of our collider
        float radius = dimensions.x / (_characterStance == CharacterStance.Standing ? 2 : 1);
        float height = dimensions.y;
        Vector3 center = new Vector3(_playerCollider.center.x, dimensions.z + 0.5F, _playerCollider.center.z);
        // Checking upper point and lower point
        Vector3 point0, point1;
        if (height < radius * 2)
        {
            // Both points are at the same place
            point0 = point1 = transform.position + center;
        }
        else
        {
            // Upper point
            point0 = transform.position + center + (transform.up * (float)(height * 0.5 - radius));
            // Lower point
            point1 = transform.position + center - (transform.up * (float)(height * 0.5 - radius));
        }
        // Get all overlaps
        int numOverlaps = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, _obstructions, _layerMask);
        // Check if player is not the only who overlaps
        for (int i = 0; i < numOverlaps; i++)
        {
            if (_obstructions[i] == _playerCollider)
            {
                numOverlaps--;
            }
        }
        return numOverlaps > 0;
    }

    /// <summary>
    /// Changes capsule dimension
    /// </summary>
    /// <param name="dimensions">New capsule dimension</param>
    private void SetCapsuleDimensions(Vector3 dimensions)
    {
        _playerCollider.center = new Vector3(_playerCollider.center.x, dimensions.z, _playerCollider.center.z);
        _playerCollider.radius = dimensions.x;
        _playerCollider.height = dimensions.y;
    }

    /// <summary>
    /// Checks physics movement
    /// </summary>
    private void PhysicsMovementCheck()
    {
        // Move character
        _playerRigidbody.AddForce(transform.forward * _currentSpeed, ForceMode.Impulse);
        // Jump
        if (_toJump == true)
        {
            _playerRigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _toJump = false;
        }
        // Strafe
        if (_toStrafe == true)
        {
            _playerRigidbody.AddForce(transform.forward * _strafeForce, ForceMode.Impulse);
            _toStrafe = false;
        }
        // Fall check 
        if (_playerRigidbody.velocity.y < 0)
        {
            _playerRigidbody.velocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.deltaTime;
        }
    }

    #endregion
}