using UnityEngine;

public class WindRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float startDelay = 3f;

    private WindArea windAreaScript;
    
    [SerializeField]
    private WindParticleSystem windParticleSystem;
    
    private float currentRotationSpeed = 0f;
    private bool wasWindActive = false;
    private float startTimer = 0f;
    private bool startComplete = false;

    private void Start()
    {
        // Get the WindZone script from the parent
        windAreaScript = GetComponentInParent<WindArea>();
        
        // Always start with zero rotation speed regardless of the wind state
        currentRotationSpeed = 0f;
        
        if (windAreaScript == null)
        {
            Debug.LogError("WindZone script not found on parent object. Make sure this GameObject has a parent with the WindZone script.");
        }
        else
        {
            wasWindActive = windAreaScript.windActive;
        }

        if (windParticleSystem == null)
        {
            Debug.LogError("Make sure the WindParticleSystem script is attached to the game object.");
        }
    }

    private void Update()
    {
        if (windAreaScript == null)
            return;

        // Handle start delay
        if (!startComplete)
        {
            startTimer += Time.deltaTime;
            if (startTimer >= startDelay)
            {
                windParticleSystem.ActivateWind();
                startComplete = true;
            }
            else
            {
                // Don't start rotating until the delay is complete
                return;
            }
        }

        bool isWindActive = windAreaScript.windActive;
        
        // Check if wind state has changed
        if (isWindActive != wasWindActive)
        {
            wasWindActive = isWindActive;
        }

        // Smoothly adjust the rotation speed
        float targetSpeed = isWindActive ? rotationSpeed : 0f;
        currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, targetSpeed, Time.deltaTime * transitionSpeed);

        // Apply rotation
        if (Mathf.Abs(currentRotationSpeed) > 0.01f)
        {
            transform.Rotate(rotationAxis, currentRotationSpeed * Time.deltaTime);
        }
    }
}