using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{

    // Camera Parameters
    [Header("Global Parameters")]
    [SerializeField, Tooltip("Camera Game Object")] private Camera playerCamera;
    [SerializeField, Tooltip("Player Game Object")] private Transform playerObject;

    // 1st Person Parameter Section
    [Header("1st Person Parameters")]
    [SerializeField, Tooltip("Camera Pivot")] private Transform firstPersonAnchor;
    [SerializeField, Tooltip("Camera X Sensitivity")] private float firstPersonMouseSensX;
    [SerializeField, Tooltip("Camera Y Sensitivity")] private float firstPersonMouseSensY;
    [SerializeField, Tooltip("Camera FOV")] private float firstPersonFOV;

    // 3rd Person Parameter Section
    [Header("3rd Person Parameters")]
    [SerializeField, Tooltip("Camera Pivot")] private Transform thirdPersonAnchor;
    [SerializeField, Tooltip("Camera Distance")] private float cameraDist;
    [SerializeField, Tooltip("Camera Smooth Toggle")] private bool camSmoothToggle;
    [SerializeField, Tooltip("Camera Smooth Strenght")] private float camSmoothStrength;
    [SerializeField, Tooltip("Camera X Sensitivity")] private float thirdPersonMouseSensX;
    [SerializeField, Tooltip("Camera Y Sensitivity")] private float thirdPersonMouseSensY;
    [SerializeField, Tooltip("Max Camera Height")] private float maxCameraHeight;
    [SerializeField, Tooltip("Min Camera Height")] private float minCameraHeight;
    
    // Vehicle Camera Parameter Section
    [Header("Vehicle Camera Parameters")]

    // Camera Values for Debugging
    [Header("Camera Debug Values")]
    [SerializeField, Tooltip("Camera Input")] private Vector2 mouseInput;
    [SerializeField] private float limitUp;
    [SerializeField] private float limitDown;

    // Enum for View Modes
    [SerializeField, Tooltip("Toggle View")] private enum ViewMode {
        FirstPerson,
        ThirdPerson,
    }

    // Declaring View Mode
    private ViewMode currentViewMode = ViewMode.FirstPerson;

    // Declare the Input Controller
    private PlayerControls playerInput;

    // Declare Camera Targets
    private Transform thirdPersonCameraTarget;
    private Transform firstPersonCameraTarget;
    private Transform vehthirdPersonCameraTarget;

    // Declare mouseX for Look Clamp
    private float playerX = 0f;

    // Create Delegate for Camera Swapping
    delegate void currentCameraControl();
    currentCameraControl playerCameraControl;

    private void Awake()
    {
        playerInput = new PlayerControls();
        thirdPersonCameraTarget = thirdPersonAnchor.GetChild(0);
        firstPersonCameraTarget = firstPersonAnchor;
        playerCameraControl = PlayerFirstCameraControl;
    }

    private void OnEnable()
    {
        playerInput.Player.Look.Enable();
        playerInput.Player.SwapCam.Enable();
        playerInput.Player.SwapCam.started += PlayerCameraMode;
        //playerInput.Player.Jump.performed += Jump;

    }
    private void OnDisable()
    {
        playerInput.Player.Look.Disable();
        playerInput.Player.SwapCam.Disable();
        playerInput.Player.SwapCam.started -= PlayerCameraMode;
    }
    
    // Handles Jumping
    /*
    private void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            Debug.Log("Jump!");
            playerBody.AddRelativeForce(new Vector3(0, 15 * jumpheight, 0), ForceMode.Impulse);
        }

    }
    */


    private void Start()
    {
        // Disable Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {
        // Follows the Player with the Selected Camera
        playerCameraControl();
    
    }
    

    
    // Swap Camera View on a Key press
    private void PlayerCameraMode(InputAction.CallbackContext context)
    {
        // Easily switch between First and Third person
        if (currentViewMode == ViewMode.ThirdPerson) currentViewMode = 0; else currentViewMode++;

        switch (currentViewMode)
        {
            case ViewMode.FirstPerson:
                {
                    playerCameraControl = PlayerFirstCameraControl;
                    return;
                }
            case ViewMode.ThirdPerson: 
                { 
                    playerCameraControl = PlayerThirdCameraControl;
                    return; 
                }

        }
        
    }

    // Allows the Player to Control the First Person Camera
    private void PlayerFirstCameraControl()
    {
        
        // Get Mouse Input
        mouseInput = GetPlayerMouseInput(firstPersonMouseSensX, firstPersonMouseSensY);
        // Move Camera to First Person Anchor
        playerCamera.transform.position = new(firstPersonAnchor.position.x, firstPersonAnchor.position.y, firstPersonAnchor.position.z);

        // Clamp rotation for Quaternion to avoid fucky wucky
        playerX += -mouseInput.y;
        playerX = Mathf.Clamp(playerX, limitDown, limitUp);

        // Rotate the Camera and Player as per Input
        playerCamera.transform.localRotation = Quaternion.Euler(playerX, playerObject.eulerAngles.y, 0f);
        playerObject.eulerAngles = new(playerObject.eulerAngles.x, playerObject.eulerAngles.y + mouseInput.x, playerObject.eulerAngles.z);
        
    }

    // Allows the Player to Control the Third Person Camera
    private void PlayerThirdCameraControl()
    {

        // Get Mouse Input
        mouseInput = GetPlayerMouseInput(thirdPersonMouseSensX, thirdPersonMouseSensY);

        // Follow the Player with Camera and then Track them
        thirdPersonAnchor.position = new(playerObject.transform.position.x, playerObject.transform.position.y + 1f, playerObject.transform.position.z);
        playerCamera.transform.LookAt(playerObject,Vector3.up);
                
        // Toggle the Camera Smooth
        if (camSmoothToggle)
        {
            Vector3 oldPosition = playerCamera.transform.position;
            Vector3 newPosition = thirdPersonCameraTarget.position;
            Vector3 LerpPosition = Vector3.Lerp(oldPosition, newPosition, camSmoothStrength * Time.deltaTime);
            playerCamera.transform.position = LerpPosition;
            
        }  else
        {
            playerCamera.transform.position = thirdPersonCameraTarget.position;
        }
        
        // Move Camera's Y position to Make it look like the camera is Pitching
        Vector3 moveCamera = new(thirdPersonCameraTarget.localPosition.x ,thirdPersonCameraTarget.localPosition.y + -mouseInput.y, thirdPersonCameraTarget.localPosition.z);
        float cameraHeight = Mathf.Clamp(moveCamera.y, minCameraHeight, maxCameraHeight);
        thirdPersonCameraTarget.localPosition = new(moveCamera.x, cameraHeight, -cameraDist);

        // Rotate Pivot to Control Camera and Player Rotation.
        playerObject.eulerAngles = new(playerObject.eulerAngles.x, playerObject.eulerAngles.y + mouseInput.x, playerObject.eulerAngles.z);

    }
    
    // Normalise the Mouse Input
    private Vector2 GetPlayerMouseInput(float mouseSensX, float mouseSensY)
    {
        mouseInput = playerInput.Player.Look.ReadValue<Vector2>();
        mouseInput = new Vector2(mouseInput.x * Time.deltaTime * mouseSensX, mouseInput.y * Time.deltaTime * mouseSensY);
        return mouseInput;
    }
}







