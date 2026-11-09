using UnityEngine;
using UnityEngine.InputSystem;

// Simple first-person movement: WASD to walk, mouse to look, Shift to walk faster.
// Put this on a "Player" object that has a CharacterController,
// with the camera as a child object at head height.
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Walking speed in metres per second.")]
    public float moveSpeed = 2.5f;

    [Tooltip("Speed while holding Shift.")]
    public float sprintSpeed = 4.5f;

    [Tooltip("Downward acceleration (m/s^2). Keeps the player on the ground and on slopes.")]
    public float gravity = -9.81f;

    [Header("Mouse look")]
    [Tooltip("Degrees turned per pixel of mouse movement. Keep this low: fast turning is hard to watch.")]
    [Range(0.01f, 0.5f)]
    public float mouseSensitivity = 0.08f;

    [Tooltip("How far up/down the player can look, in degrees.")]
    [Range(30f, 89f)]
    public float maxLookAngle = 80f;

    [Tooltip("The camera that sits at head height (a child of this object).")]
    public Transform cameraTransform;

    // When false, the player can't move or look (useful later for notes, menus, the ending screen).
    [HideInInspector] public bool canControl = true;

    private CharacterController controller;
    private float pitch;            // current up/down camera angle
    private float verticalVelocity; // falling speed, built up by gravity

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // If the camera wasn't dragged into the Inspector slot, use the camera inside the Player.
        if (cameraTransform == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();
            if (childCamera != null) cameraTransform = childCamera.transform;
            else Debug.LogError("FirstPersonController: no camera found. Add a Camera as a child of the Player.");
        }

        LockCursor(true);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        // Escape frees the mouse cursor; clicking in the game window captures it again.
        if (keyboard.escapeKey.wasPressedThisFrame) LockCursor(false);
        if (mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked) LockCursor(true);

        bool active = canControl && Cursor.lockState == CursorLockMode.Locked;
        if (active) Look(mouse);
        Move(keyboard, active);
    }

    void Look(Mouse mouse)
    {
        // Mouse delta = how many pixels the mouse moved this frame.
        // It's already "per frame", so we do NOT multiply it by Time.deltaTime.
        Vector2 delta = mouse.delta.ReadValue() * mouseSensitivity;

        // Left/right turns the whole body; up/down tilts only the camera.
        transform.Rotate(0f, delta.x, 0f);
        pitch = Mathf.Clamp(pitch - delta.y, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void Move(Keyboard keyboard, bool active)
    {
        // Read WASD as a direction: x = left/right, y = forward/back.
        Vector2 input = Vector2.zero;
        if (active)
        {
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
            input = Vector2.ClampMagnitude(input, 1f); // diagonal isn't faster
        }

        float speed = keyboard.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;

        // Turn the input into a world direction relative to where the player faces.
        Vector3 move = (transform.right * input.x + transform.forward * input.y) * speed;

        // Gravity: small constant push down while grounded, accelerate while falling.
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        // CharacterController.Move slides along walls instead of passing through them.
        controller.Move(move * Time.deltaTime);
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
