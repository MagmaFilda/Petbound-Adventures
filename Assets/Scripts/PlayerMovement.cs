using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jump = 2f;
    public float gravity = -8f;
    public Transform cameraTransform;

    private PlayerInput playerInput;
    private InputAction movementAction;
    private InputAction jumpAction;
    private InputAction moveCameraAction;
    private InputAction rotateCameraAction;
    private CharacterController controller;

    private bool isGrounded;
    private float doJump;
    private float doRotateCamera;
    private float cameraPitch = 0f;
    private Vector3 playerVelocity;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        movementAction = playerInput.actions.FindAction("WASD");
        jumpAction = playerInput.actions.FindAction("Jump");
        moveCameraAction = playerInput.actions.FindAction("CameraMove");
        rotateCameraAction = playerInput.actions.FindAction("DoRotate");
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        MovePlayer();
        CameraMovement();
    }

    private void MovePlayer()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.1f))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) < 45f)
            {
                isGrounded = true;
                playerVelocity.y = 0f;
            }
            else {isGrounded = false;}
        }
        else { isGrounded = false;}

        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        Vector2 movementInput = movementAction.ReadValue<Vector2>();
        Vector3 move = transform.right * movementInput.x + transform.forward * movementInput.y;
        controller.Move(move * speed * Time.deltaTime);

        doJump = jumpAction.ReadValue<float>();
        if (doJump > 0 && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jump * -2f * gravity);
            controller.Move(playerVelocity * Time.deltaTime);
        }
    }

    private void CameraMovement()
    {
        doRotateCamera = rotateCameraAction.ReadValue<float>();
        if (doRotateCamera > 0)
        {
            Vector2 cameraMove = moveCameraAction.ReadValue<Vector2>();
            transform.Rotate(Vector3.up * cameraMove.x * 0.1f);

            cameraPitch -= cameraMove.y * 0.1f;
            cameraPitch = Mathf.Clamp(cameraPitch, -10f, 10f);

            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0); // to je nahoru a dolu kdybych chtìl
        }
    }
}
