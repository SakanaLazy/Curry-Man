using UnityEngine;
using UnityEngine.InputSystem; // New Input System

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 720f;
    public float gravity = -9.81f;
    public float jumpForce = 5f;

    [Header("Refs")]
    public Animator anim;

    // 🔒 Combat can toggle this:
    [HideInInspector] public bool movementLocked = false;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (!anim) anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        var kb = Keyboard.current;

        // --- Grounding & gravity (always runs) ---
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        // --- Movement input (blocked if locked) ---
        Vector2 move = Vector2.zero;
        if (!movementLocked && kb != null)
        {
            if (kb.wKey.isPressed) move.y += 1;
            if (kb.sKey.isPressed) move.y -= 1;
            if (kb.dKey.isPressed) move.x += 1;
            if (kb.aKey.isPressed) move.x -= 1;
        }

        Vector3 inputDir = new Vector3(move.x, 0, move.y).normalized;

        bool isRunning = !movementLocked && kb != null && kb.leftShiftKey.isPressed;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (!movementLocked && inputDir.sqrMagnitude > 0.001f)
        {
            // Move & rotate
            Vector3 moveDir = inputDir * currentSpeed;
            controller.Move(moveDir * Time.deltaTime);

            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            if (anim) anim.SetFloat("Speed", currentSpeed);
        }
        else
        {
            if (anim) anim.SetFloat("Speed", 0f);
        }

        // --- Jump (blocked if locked) ---
        if (!movementLocked && kb != null && kb.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = jumpForce;
            if (anim) anim.SetTrigger("Jump");
        }

        // --- Apply gravity ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
