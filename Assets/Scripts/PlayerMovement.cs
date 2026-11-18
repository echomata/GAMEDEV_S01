using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float sprintMultiplier = 1.5f;
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float gravity = -19.62f;
    [SerializeField] float turnSpeed = 720f;

    [Header("Third-person view")]
    public Transform movementCamera;

    CharacterController controller;
    Vector3 velocity;
    Vector3 spawnPosition;
    Vector2 moveInput;
    bool jumpRequested;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        spawnPosition = transform.position;
    }

    void Start()
    {
        if (movementCamera == null && Camera.main != null)
            movementCamera = Camera.main.transform;
        LockCursor();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            LockCursor();

        Vector2 input = moveInput;
        if (keyboard != null)
        {
            Vector2 keys = new Vector2(
                (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0)
                - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0),
                (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0)
                - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0));
            if (keys.sqrMagnitude > 0) input = keys;
            if (keyboard.spaceKey.wasPressedThisFrame) jumpRequested = true;
        }

        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        if (jumpRequested && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        jumpRequested = false;

        Vector3 forward = movementCamera != null ? movementCamera.forward : Vector3.forward;
        Vector3 right = movementCamera != null ? movementCamera.right : Vector3.right;
        forward.y = 0; right.y = 0;
        Vector3 move = Vector3.ClampMagnitude(forward.normalized * input.y + right.normalized * input.x, 1);
        if (move.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(move), turnSpeed * Time.deltaTime);

        float speed = moveSpeed;
        if (keyboard != null && keyboard.leftShiftKey.isPressed) speed *= sprintMultiplier;
        velocity.y += gravity * Time.deltaTime;
        controller.Move((move * speed + velocity) * Time.deltaTime);

        if (transform.position.y < -10f) Respawn();
    }

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnJump(InputValue value) { if (value.isPressed) jumpRequested = true; }

    public void Respawn()
    {
        controller.enabled = false;
        transform.position = spawnPosition;
        velocity = Vector3.zero;
        controller.enabled = true;
        GetComponent<PlayerAura>()?.ResetToNeutral();
        if (movementCamera != null) movementCamera.GetComponent<ThirdPersonCamera>()?.SnapToTarget();
    }

    void LockCursor() { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    void OnDisable() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
}