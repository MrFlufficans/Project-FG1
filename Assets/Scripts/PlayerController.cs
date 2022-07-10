using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Player Variables
    [Header("Player Parameters")]
    [SerializeField, Tooltip("Player Speed")] private float speedMult;
    [SerializeField, Tooltip("Jump Multiplier")] private float jumpheight;
    [SerializeField, Tooltip("Max Speed")] private float speedMax;
    [SerializeField, Tooltip("Ground Clearance")] private float groundCheckDistance;

    // Camera Pivot
    [SerializeField, Tooltip("Camera Controller")] private CameraController cameraController;
    [SerializeField, Tooltip("Camera Pivot")] private Transform cameraPivot;

    // Read only Values for Debugging
    [Header("Read Only Values")]
    [SerializeField, Tooltip("Current Speed")] private Vector3 playerVel;
    [SerializeField, Tooltip("Is Grounded")] private bool isGrounded;
    [SerializeField, Tooltip("Move Input")] private Vector2 movementInput;

    // Declare the Input Controller
    private PlayerControls playerInput;
    // Declare Rigidbody
    private Rigidbody playerBody;

    // Declared for Movement Input
    [SerializeField] private float currentMoveSpeed;

    private void Awake()
    {
        playerBody = GetComponent<Rigidbody>();
        playerInput = new PlayerControls();
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
        playerInput.Player.Jump.performed += Jump;
    }
    private void OnDisable()
    {
        playerInput.Player.Disable();
        playerInput.Player.Jump.performed -= Jump;
    }

    // Handles Jumping
    private void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            Debug.Log("Jump!");
            playerBody.AddRelativeForce(new Vector3(0, 15 * jumpheight, 0), ForceMode.Impulse);
        }

    }
   
    // Handles normal Movement
    private void HandleMovement()
    {
        transform.Translate(movementInput.x * Time.deltaTime * speedMult, 0,movementInput.y * Time.deltaTime * speedMult);
        //currentMoveSpeed = transform.position.z - transform.position.z * Time.deltaTime;
        //transform.localPosition = new(transform.localPosition.x + movementInput.x * Time.deltaTime * speedMult, transform.localPosition.y, transform.forward + movementInput.y * Time.deltaTime * speedMult);
    }

    // Check if player is Grounded
    private void IsOnGround()
    {
        //int groundLayer = LayerMask.NameToLayer("Ground");
        isGrounded = Physics.Raycast(playerBody.transform.position, Vector3.down, groundCheckDistance);
    }

    private void Update()
    {
        movementInput = playerInput.Player.Movement.ReadValue<Vector2>();
        IsOnGround();
        playerVel = playerBody.velocity;
        if (playerVel.y < 0 && !isGrounded)
        {
            if (playerBody.mass >= 20f) playerBody.mass = 20f;
            playerBody.mass = playerBody.mass * 1.1f;
        } 
        
        if (playerVel.y < 0 && isGrounded) playerBody.mass = 2f;

        // Match the Players turn with the Camera Pivot
        //transform.eulerAngles = new Vector3 (transform.eulerAngles.x , cameraPivot.eulerAngles.y , transform.eulerAngles.z);

        // Handles the Movement
        HandleMovement();
        

    }

}
