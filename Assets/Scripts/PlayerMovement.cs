using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float gravity = -8f;
    public Transform cameraTransform;
    public Transform character;

    private PlayerInput playerInput;
    private InputAction movementAction;
    private InputAction jumpAction;
    private InputAction moveCameraAction;
    private InputAction rotateCameraAction;
    private InputAction zoomCameraAction;
    private InputAction changeFullscreenAction;
    private CharacterController controller;
    private Animator animator;

    private bool isGrounded;
    private float doJump;
    private float doRotateCamera;
    private float cameraPitch = 0f;
    private float cameraYaw = 0f;
    private Vector3 playerVelocity;
    private Vector2 cameraMove;

    private PlayerStats playerStats;
    private CinemachineThirdPersonFollow zoomCamera;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        movementAction = playerInput.actions.FindAction("WASD");
        jumpAction = playerInput.actions.FindAction("Jump");
        moveCameraAction = playerInput.actions.FindAction("CameraMove");
        rotateCameraAction = playerInput.actions.FindAction("DoRotate");
        zoomCameraAction = playerInput.actions.FindAction("Zoom");
        changeFullscreenAction = playerInput.actions.FindAction("Fullscreen");
        controller = GetComponent<CharacterController>();
        animator = character.GetComponent<Animator>();

        playerStats = PlayerStats.Instance;
        zoomCamera = cameraTransform.Find("Third Person Aim Camera").GetComponent<CinemachineThirdPersonFollow>();
    }

    private void Update()
    {
        if (playerStats.canMove)
        {
            MovePlayer();
        }     
        if (playerStats.canRotateCamera)
        {
            CameraMovement();
        }

        if (changeFullscreenAction.triggered)
        {
            if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.SetResolution(1920,1080,false);
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            }
        }
    }

    private void MovePlayer()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.1f))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) < 45f)
            {
                isGrounded = true;
                playerVelocity.y = 0f;

                if (animator.GetBool("IsJumping"))
                {
                    animator.SetBool("IsJumping", false);
                }
            }
            else {isGrounded = false;}
        }
        else { isGrounded = false;}

        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        Vector2 movementInput = movementAction.ReadValue<Vector2>();
        if (movementInput != new Vector2(0, 0))
        {
            Vector3 move = transform.right * movementInput.x + transform.forward * movementInput.y;
            controller.Move(move * playerStats.playerSpeed * Time.deltaTime);
            if (!animator.GetBool("IsWalking"))
            {
                animator.SetBool("IsWalking", true);
            }            

            if (cameraYaw != 0f)
            {
                transform.Rotate(Vector3.up * cameraYaw);

                cameraYaw = 0f;
                cameraTransform.localRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
            }            
        }
        else
        {
            if (animator.GetBool("IsWalking"))
            {
                animator.SetBool("IsWalking", false);
            }
        }

        doJump = jumpAction.ReadValue<float>();
        if (doJump > 0 && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(playerStats.playerJumpPower * -2f * gravity);
            controller.Move(playerVelocity * Time.deltaTime);

            if (!animator.GetBool("IsJumping"))
            {
                animator.SetBool("IsJumping", true);
            }
        }
    }

    private void CameraMovement()
    {
        doRotateCamera = rotateCameraAction.ReadValue<float>();
        if (doRotateCamera > 0)
        {
            cameraMove = moveCameraAction.ReadValue<Vector2>();

            cameraPitch -= cameraMove.y * 0.1f;
            cameraPitch = Mathf.Clamp(cameraPitch, -15f, 40f);

            cameraYaw += cameraMove.x * 0.3f;

            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        }

        float zoomPower = zoomCameraAction.ReadValue<Vector2>().y;
        if (zoomPower != 0)
        {
            zoomCamera.CameraDistance += (-zoomPower/2);
            if (zoomCamera.CameraDistance < 2) { zoomCamera.CameraDistance = 2; }
            else if (zoomCamera.CameraDistance > 8) { zoomCamera.CameraDistance = 8;}
        }
    }
}
