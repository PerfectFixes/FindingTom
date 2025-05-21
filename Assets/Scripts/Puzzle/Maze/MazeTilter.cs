using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MazeTilter : MonoBehaviour
{
    [Header("Tilt Settings")]
    [Tooltip("Maximum tilt angle in degrees")]
    [SerializeField] private float maxTiltAngle = 5f;
    
    [Tooltip("Time in seconds to reach maximum tilt angle")]
    [SerializeField] private float tiltTime = 3f;
    
    [Tooltip("Time in seconds to return to flat position")]
    [SerializeField] private float returnTime = 3f;
    
    [Tooltip("Reference to Input Actions asset")]
    [SerializeField] private InputActionReference moveActionReference;

    [Header("Delay Movement")]
    [SerializeField]
    private float delayTime = 0f;

    private bool isPuzzleSolved;
    // Input state
    private Vector2 inputVector;
    
    // Rotation state tracking
    private Quaternion initialRotation;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private bool isLerping = false;
    private float lerpStartTime;
    private float lerpDuration;

    private void Awake()
    {
        // Store the initial rotation (likely Quaternion.identity)
        initialRotation = transform.rotation;
        targetRotation = initialRotation;
        startRotation = initialRotation;
    }

    private void OnEnable()
    {
        if (moveActionReference != null && moveActionReference.action != null)
        {
            StartCoroutine(DelayForStart());
        }
    }

    private void OnDisable()
    {
        if (moveActionReference != null && moveActionReference.action != null)
        {
            // Unsubscribe to prevent memory leaks
            moveActionReference.action.performed -= OnMovePerformed;
            moveActionReference.action.canceled -= OnMoveCanceled;
            moveActionReference.action.Disable();
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // Read the current input vector (joystick or WASD)
        Vector2 newInput = context.ReadValue<Vector2>();
        
        // Only process if input has changed significantly
        if (Vector2.Distance(newInput, inputVector) > 0.1f)
        {
            inputVector = newInput;
            UpdateTargetRotation();
        }
    }

    public void StopMoving()
    {
        OnDisable();
        isPuzzleSolved = true;
        //ResetRotationToZero();
    }

    // New method to reset rotation to 0,0,0
    public void ResetRotationToZero()
    {
        // Save current rotation as start
        startRotation = transform.rotation;
        
        // Set target to zero rotation
        targetRotation = Quaternion.Euler(Vector3.zero);
        
        // Start a new lerp with return duration
        StartNewLerp(returnTime);
    }
    
    IEnumerator DelayForStart()
    {
        yield return new WaitForSeconds(delayTime);
        moveActionReference.action.Enable();
            
        // Subscribe to performed and canceled to get input updates
        moveActionReference.action.performed += OnMovePerformed;
        moveActionReference.action.canceled += OnMoveCanceled;
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        // Clear input when released
        inputVector = Vector2.zero;
        UpdateTargetRotation();
    }

    private void FixedUpdate()
    {
        // Update the platform rotation with proper timing
        if (isLerping && !isPuzzleSolved)
        {
            UpdateRotation();
        }
    }
    
    private void UpdateTargetRotation()
    {
      
        // Check if we have meaningful input
        bool hasInput = inputVector.magnitude > 0.1f;
        
        if (hasInput)
        {
            // Clamp input magnitude to 1 for consistent max tilt
            Vector2 normalizedInput = inputVector.magnitude > 1f 
                ? inputVector.normalized 
                : inputVector;
            
            // Create euler angles for tilt
            // X rotation is controlled by Y input (forward/back)
            // Z rotation is controlled by X input (left/right)
            Vector3 targetEuler = new Vector3(
                -normalizedInput.y * maxTiltAngle, // Negative for intuitive control
                0f, // No rotation on Y axis
                normalizedInput.x * maxTiltAngle
            );
            
            // Convert to quaternion
            Quaternion newTargetRotation = Quaternion.Euler(targetEuler);
            
            // Check if target has significantly changed
            if (Quaternion.Angle(newTargetRotation, targetRotation) > 1f)
            {
                // Save current rotation as start
                startRotation = transform.rotation;
                targetRotation = newTargetRotation;
                
                // Start a new lerp with tilt duration
                StartNewLerp(tiltTime);
            }
        }
        else if (!Quaternion.Equals(targetRotation, initialRotation))
        {
            // No input, so target is initial flat rotation
            startRotation = transform.rotation;
            targetRotation = initialRotation;
            
            // Start a new lerp with return duration
            StartNewLerp(returnTime);
        }
    }
    
    private void StartNewLerp(float duration)
    {
       
        // Start a new lerp
        isLerping = true;
        lerpStartTime = Time.time;
        lerpDuration = duration;
    }
    
    private void UpdateRotation()
    {
        // Calculate how much time has passed since lerp started
        float timeSinceLerpStarted = Time.time - lerpStartTime;
        
        // Check if we're done lerping
        if (timeSinceLerpStarted >= lerpDuration)
        {
            // We've reached the target duration, set the final rotation
            transform.rotation = targetRotation;
            isLerping = false;
            return;
        }
        
        // Calculate normalized time (0-1)
        float t = timeSinceLerpStarted / lerpDuration;
        
        // Apply smooth rotation from start position toward target
        transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
    }
}