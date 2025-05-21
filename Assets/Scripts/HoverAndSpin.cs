using UnityEngine;

[AddComponentMenu("Custom/Hover And Spin")]
public class HoverAndSpin : MonoBehaviour
{
    [Header("Hover Settings")]
    [Tooltip("Minimum hover height")]
    public float hoverAmplitudeMin = 0.1f;
    [Tooltip("Maximum hover height")]
    public float hoverAmplitudeMax = 0.3f;
    [Tooltip("Minimum hover speed")]
    public float hoverFrequencyMin = 0.5f;
    [Tooltip("Maximum hover speed")]
    public float hoverFrequencyMax = 1.5f;
    
    [Header("Rotation Settings")]
    [Tooltip("Minimum rotation speed X axis")]
    public float rotationSpeedXMin = 0f;
    [Tooltip("Maximum rotation speed X axis")]
    public float rotationSpeedXMax = 30f;
    [Tooltip("Minimum rotation speed Y axis")]
    public float rotationSpeedYMin = 10f;
    [Tooltip("Maximum rotation speed Y axis")]
    public float rotationSpeedYMax = 50f;
    [Tooltip("Minimum rotation speed Z axis")]
    public float rotationSpeedZMin = 0f;
    [Tooltip("Maximum rotation speed Z axis")]
    public float rotationSpeedZMax = 20f;

    [Header("Phase Variation")]
    [Tooltip("Randomize the starting phase")]
    public bool randomizePhase = true;

    // Private randomized values for this instance
    private float hoverAmplitude;
    private float hoverFrequency;
    private Vector3 rotationSpeed;
    private float phaseOffset;
    
    // Original position
    private Vector3 startPosition;

    private void Start()
    {
        // Store the initial position
        startPosition = transform.position;
        
        // Randomize all parameters for this instance
        RandomizeParameters();
    }

    private void RandomizeParameters()
    {
        // Randomize hover parameters
        hoverAmplitude = Random.Range(hoverAmplitudeMin, hoverAmplitudeMax);
        hoverFrequency = Random.Range(hoverFrequencyMin, hoverFrequencyMax);
        
        // Randomize rotation speeds for each axis
        float rotX = Random.Range(rotationSpeedXMin, rotationSpeedXMax);
        float rotY = Random.Range(rotationSpeedYMin, rotationSpeedYMax);
        float rotZ = Random.Range(rotationSpeedZMin, rotationSpeedZMax);
        rotationSpeed = new Vector3(rotX, rotY, rotZ);
        
        // Randomize phase offset for hover if enabled
        phaseOffset = randomizePhase ? Random.Range(0f, 2f * Mathf.PI) : 0f;
    }

    private void Update()
    {
        // Apply hover effect
        float hoverOffset = hoverAmplitude * Mathf.Sin((Time.time * hoverFrequency) + phaseOffset);
        Vector3 newPosition = startPosition + new Vector3(0f, hoverOffset, 0f);
        transform.position = newPosition;
        
        // Apply rotation
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }

    // Optional: Method to reset and re-randomize parameters at runtime
    public void ResetAndRandomize()
    {
        RandomizeParameters();
    }
}