using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Playermovement : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float dashFOV = 50f;
    [SerializeField] private float fovLerpSpeed = 12f;
    private float pitchRotation;
    private Camera cameraComponent;
    private float initialFOV;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;
    private CharacterController characterController;
    private float verticalVelocity;

    [Header("Dash")]
    [SerializeField] private float dashMultiplier = 3f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 3f;
    private bool isDashing;
    private float dashTimeRemaining;
    private float cooldownRemaining;
    private Vector3 dashDirection;

    [Header("Crosshair")]
    [SerializeField] private bool showCrosshair = true;
    [SerializeField] private int crosshairSize = 8;
    [SerializeField] private int crosshairThickness = 2;
    [SerializeField] private Color crosshairColor = Color.white;
    private static Texture2D crosshairTexture;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        if (cameraTransform != null)
        {
            cameraComponent = cameraTransform.GetComponent<Camera>();
        }
        if (cameraComponent == null && Camera.main != null)
        {
            cameraComponent = Camera.main;
        }
        if (crosshairTexture == null)
        {
            crosshairTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            crosshairTexture.SetPixel(0, 0, Color.white);
            crosshairTexture.Apply();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (cameraComponent != null)
        {
            initialFOV = cameraComponent.fieldOfView;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Mouse look (FPS)
        if (cameraTransform != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Yaw on the player body
            transform.Rotate(Vector3.up * mouseX);

            // Pitch on the camera
            pitchRotation -= mouseY;
            pitchRotation = Mathf.Clamp(pitchRotation, -90f, 90f);
            Vector3 cameraEuler = cameraTransform.localEulerAngles;
            cameraEuler.x = pitchRotation;
            cameraEuler.y = 0f;
            cameraEuler.z = 0f;
            cameraTransform.localEulerAngles = cameraEuler;
        }

        // Movement input (WASD)
        Vector3 inputDirection = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );
        inputDirection = inputDirection.sqrMagnitude > 1f ? inputDirection.normalized : inputDirection;

        float currentSpeed = moveSpeed;

        // Dash start (E key) - allow dashing while stationary
        if (Input.GetKeyDown(KeyCode.E) && cooldownRemaining <= 0f && !isDashing)
        {
            isDashing = true;
            dashTimeRemaining = dashDuration;
            cooldownRemaining = dashCooldown;
            // Capture dash direction: use input if moving, otherwise forward
            Vector3 desired = inputDirection.sqrMagnitude > 0f ? inputDirection.normalized : Vector3.forward;
            dashDirection = transform.TransformDirection(desired);
            dashDirection.y = 0f;
            if (dashDirection.sqrMagnitude > 0f)
            {
                dashDirection.Normalize();
            }
            else
            {
                dashDirection = transform.forward;
            }
        }

        // Update dash state
        if (isDashing)
        {
            currentSpeed *= dashMultiplier;
            dashTimeRemaining -= Time.deltaTime;
            if (dashTimeRemaining <= 0f)
            {
                isDashing = false;
            }
        }

        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
        }

        // Gravity & Jump
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // small downward force to keep grounded
        }
        if (characterController.isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 worldMove = transform.TransformDirection(inputDirection) * currentSpeed;
        if (isDashing)
        {
            worldMove = dashDirection * (moveSpeed * dashMultiplier);
        }
        worldMove.y = verticalVelocity;
        characterController.Move(worldMove * Time.deltaTime);

        // Camera FOV zoom during dash
        if (cameraComponent != null)
        {
            float targetFOV = isDashing ? dashFOV : (initialFOV > 0f ? initialFOV : 60f);
            cameraComponent.fieldOfView = Mathf.Lerp(
                cameraComponent.fieldOfView,
                targetFOV,
                Time.deltaTime * fovLerpSpeed
            );
        }
    }

    void OnGUI()
    {
        if (!showCrosshair || crosshairTexture == null)
        {
            return;
        }

        Color prevColor = GUI.color;
        GUI.color = crosshairColor;

        int centerX = Screen.width / 2;
        int centerY = Screen.height / 2;

        // Horizontal line
        Rect horiz = new Rect(
            centerX - crosshairSize,
            centerY - (crosshairThickness / 2),
            crosshairSize * 2,
            crosshairThickness
        );
        // Vertical line
        Rect vert = new Rect(
            centerX - (crosshairThickness / 2),
            centerY - crosshairSize,
            crosshairThickness,
            crosshairSize * 2
        );

        GUI.DrawTexture(horiz, crosshairTexture);
        GUI.DrawTexture(vert, crosshairTexture);

        GUI.color = prevColor;
    }
}
