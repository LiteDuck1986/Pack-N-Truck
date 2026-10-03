using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class VehicleController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Visual")]
    [SerializeField] private Transform visual;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Wobble")]
    [SerializeField] private float idleAngle = 0.4f;
    [SerializeField] private float movingAngle = 1.5f;
    [SerializeField] private float idleFrequency = 1f;
    [SerializeField] private float movingFrequency = 3f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Quaternion startingRotation;
    private float wobblePhase;
    private VehicleTerrainSpeed terrainSpeed;

    // Initializes the Rigidbody2D and stores the starting rotation of the visual!
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startingRotation = visual.localRotation;
        terrainSpeed = GetComponent<VehicleTerrainSpeed>();
    }

    private void Update()
    {
        ReadMovement();

        if (moveInput.x != 0f)
        {
            spriteRenderer.flipX = moveInput.x < 0f;
        }
    }

    // Reads the movement input from the keyboard
    private void ReadMovement()
    {
        moveInput = Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            moveInput.x -= 1f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            moveInput.x += 1f;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            moveInput.y -= 1f;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            moveInput.y += 1f;

        // Prevent diagonal movement from being faster.
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    // Rigidbody update
    private void FixedUpdate()
    {
        float multiplier = terrainSpeed != null
            ? terrainSpeed.GetSpeedMultiplier()
            : 1f;

        rb.linearVelocity = moveInput * moveSpeed * multiplier;
    }

    // Applies a wobble effect to the vehicle's visual, based on whether it is moving or idle
    private void LateUpdate()
    {
        bool isMoving = rb.linearVelocity.sqrMagnitude > 0.01f;

        float angle = isMoving ? movingAngle : idleAngle;
        float frequency = isMoving ? movingFrequency : idleFrequency;

        wobblePhase += Time.deltaTime * frequency * Mathf.PI * 2f;
        float wobble = Mathf.Sin(wobblePhase) * angle;

        visual.localRotation =
            startingRotation * Quaternion.Euler(0f, 0f, wobble);
    }
}