using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Top-down movement driven by the arrow keys (WASD also works).
/// Walls stop the player through normal 2D physics collisions.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;

    Rigidbody2D body;
    Vector2 moveInput;
    bool controlsEnabled = true;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
    }

    void Update()
    {
        moveInput = controlsEnabled ? ReadKeyboard() : Vector2.zero;
    }

    void FixedUpdate()
    {
        body.linearVelocity = moveInput * moveSpeed;
    }

    static Vector2 ReadKeyboard()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return Vector2.zero;

        Vector2 direction = Vector2.zero;
        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) direction.x -= 1f;
        if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) direction.x += 1f;
        if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) direction.y += 1f;
        if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) direction.y -= 1f;

        // Normalizing keeps diagonal movement the same speed as straight movement.
        return direction.normalized;
    }

    public void SetControlsEnabled(bool value)
    {
        controlsEnabled = value;
        if (value) return;

        moveInput = Vector2.zero;
        body.linearVelocity = Vector2.zero;
    }
}
