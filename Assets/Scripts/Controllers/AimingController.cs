using UnityEngine;
using UnityEngine.InputSystem;

// Controller for aiming and shooting balls using mouse input
public class AimingController : MonoBehaviour
{
    [Header("Aiming Settings")]
    public LineRenderer aimLine;
    public float aimLineLength = 5f;
    public int aimLineSegments = 20;
    public Transform aimOrigin;
    
    [Header("Input")]
    public InputActionReference readyBallAction;
    public InputActionReference shootAction;
    public InputActionReference mousePositionAction;

    [Header("Constraints")]
    public float minYDirection = 0.3f;  // Minimum upward direction (prevents too flat)
    public float maxYDirection = 1f;    // Maximum upward direction (prevents too steep)

    private Camera mainCamera;
    private bool isAiming = false;
    private Vector3 aimDirection;
    private BallSpawner ballSpawner;

    private void Awake()
    {
        mainCamera = Camera.main;
        ballSpawner = GetComponent<BallSpawner>();
        
        if (aimLine != null)
        {
            aimLine.positionCount = aimLineSegments;
            aimLine.enabled = false;
        }
    }

    private void OnEnable()
    {
        // Subscribe to input actions
        if (readyBallAction != null)
        {
            readyBallAction.action.Enable();
            readyBallAction.action.performed += OnReadyBall;
        }

        if (shootAction != null)
        {
            shootAction.action.Enable();
            shootAction.action.performed += OnShoot;
        }

        if (mousePositionAction != null)
        {
            mousePositionAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from input actions
        if (readyBallAction != null)
        {
            readyBallAction.action.performed -= OnReadyBall;
            readyBallAction.action.Disable();
        }

        if (shootAction != null)
        {
            shootAction.action.performed -= OnShoot;
            shootAction.action.Disable();
        }

        if (mousePositionAction != null)
        {
            mousePositionAction.action.Disable();
        }
    }

    private void Update()
    {
        if (isAiming)
        {
            UpdateAimDirection();
            DrawAimLine();
        }
    }

    private void OnReadyBall(InputAction.CallbackContext context)
    {
        // Start aiming when ready action is triggered and no ball is active
        if (!isAiming && ballSpawner != null && !ballSpawner.HasActiveBall())
        {
            StartAiming();
        }
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        // Shoot the ball when shoot action is triggered
        if (isAiming)
        {
            ShootBall();
        }
    }

    private void StartAiming()
    {
        isAiming = true;
        if (aimLine != null)
        {
            aimLine.enabled = true;
        }
        Debug.Log("[AimingController] Ready to aim!");
    }

    private void UpdateAimDirection()
    {
        if (mousePositionAction == null || mainCamera == null || aimOrigin == null)
            return;

        // Get mouse position in screen space
        Vector2 mousePos = mousePositionAction.action.ReadValue<Vector2>();
        
        // Convert to world position (10f is distance from camera)
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
        
        // Calculate simple direction from origin to mouse
        Vector3 direction = mouseWorldPos - aimOrigin.position;
        direction.z = 0f;  // Keep it 2D
        direction.Normalize();  // Make it unit length

        // Simple clamping: limit Y component to prevent too flat or too steep shots
        direction.y = Mathf.Clamp(direction.y, minYDirection, maxYDirection);
        
        // Re-normalize after clamping
        aimDirection = direction.normalized;
    }

    private void DrawAimLine()
    {
        if (aimLine == null || aimOrigin == null)
            return;

        Vector3 startPos = aimOrigin.position;

        // Draw line by placing points along the aim direction
        for (int i = 0; i < aimLineSegments; i++)
        {
            // Calculate how far along the line (0 to 1)
            float percent = (float)i / (aimLineSegments - 1);
            
            // Calculate position = start + direction * distance
            Vector3 pos = startPos + aimDirection * aimLineLength * percent;
            
            aimLine.SetPosition(i, pos);
        }
    }

    private void ShootBall()
    {
        if (ballSpawner != null)
        {
            ballSpawner.ShootBallWithDirection(aimDirection);
        }

        StopAiming();
    }

    private void StopAiming()
    {
        isAiming = false;
        if (aimLine != null)
        {
            aimLine.enabled = false;
        }
        Debug.Log("[AimingController] Ball launched!");
    }

    public bool IsAiming()
    {
        return isAiming;
    }
}
