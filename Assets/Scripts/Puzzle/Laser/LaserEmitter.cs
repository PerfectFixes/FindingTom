using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
    [Header("Laser Settings")]
    [Tooltip("Maximum distance the laser can travel")]
    public float maxDistance = 100f;
    
    [Tooltip("The layer mask to determine what the laser can hit")]
    public LayerMask layerMask;
    
    [Tooltip("The width of the laser beam")]
    public float laserWidth = 0.1f;
    
    [Tooltip("Maximum number of deflections allowed")]
    public int maxDeflections = 10;
    
    [Header("Emission Control")]
    [Tooltip("Master switch to enable/disable the laser")]
    public bool isEnabled = true;
    
    [Tooltip("Controls the emission mode: On=Continuous laser, Off=Pulsing laser")]
    public bool isContinuous = false;
    
    [Header("Timing Settings")]
    [Tooltip("Time between laser activations in seconds (only used in pulsing mode)")]
    public float cycleTime = 3.5f;
    
    [Tooltip("Duration the laser stays active in seconds (only used in pulsing mode)")]
    public float activeTime = 0.5f;
    
    [Tooltip("Time before the laser starts shrinking (in seconds)")]
    public float shrinkDelay = 0.1f;
    
    [Header("Visualization")]
    [Tooltip("Prefab to use for laser segments (cube recommended)")]
    public GameObject laserSegmentPrefab;
    
    [Tooltip("Default material for the laser")]
    public Material defaultLaserMaterial;

    [SerializeField] 
    private Animator sunAnimator;
    
    // Runtime variables
    private List<Vector3> hitPoints = new List<Vector3>();
    private List<GameObject> laserSegments = new List<GameObject>();
    private Transform laserParent; // Parent object for all laser segments
    private float timer = 0f;
    private float activeTimer = 0f; // Tracks how long the laser has been active
    private bool isLaserActive = false;
    
    // Puzzle tracking variables
    private List<LaserDeflector> hitDeflectors = new List<LaserDeflector>();
    private int totalDeflectorsInScene = 0;
    
    // Each segment needs its own material
    private List<Material> segmentMaterials = new List<Material>();
    
    [Header("SFX")]
    public bool isPlayingSFX;
    public AudioClip laserSpawnAudioClip;
    [Range(0, 1)] public float laserAudioVolume = 0.5f;
    
    [Obsolete("Obsolete")]
    private void Awake()
    {
        // Create a parent object for all laser segments
        GameObject parentObject = new GameObject("Laser_" + gameObject.name);
        laserParent = parentObject.transform;
        // Make it a child of this emitter for organization
        laserParent.parent = transform;
        
        // Count the total number of deflectors in the scene
        totalDeflectorsInScene = FindObjectsOfType<LaserDeflector>().Length;
        
        // Check if we have a default material
        if (defaultLaserMaterial == null)
        {
            Debug.LogWarning("No default laser material assigned. Creating a basic emissive material.");
            defaultLaserMaterial = new Material(Shader.Find("Standard"));
            defaultLaserMaterial.EnableKeyword("_EMISSION");
            defaultLaserMaterial.SetColor("_Color", Color.red);
            defaultLaserMaterial.SetColor("_EmissionColor", Color.red * 2f);
        }
    }
    
    private void Start()
    {
        //Set the animation to start as charging
        sunAnimator.SetBool("Charge", true);
        // Initialize the laser state based on settings
        if (isEnabled && isContinuous)
        {
            isLaserActive = true;
            UpdateLaserPath();
        }
        else
        {
            isLaserActive = false;
            DeactivateLaser();
        }
    }
    
    private void Update()
    {
        // Master switch: if disabled, ensure laser is off
        if (!isEnabled)
        {
            if (isLaserActive)
            {
                DeactivateLaser();
                isLaserActive = false;
            }
            return;
        }
        
        // Continuous mode: keep the laser always active
        if (isContinuous)
        {
            if (!isLaserActive)
            {
                isLaserActive = true;
                UpdateLaserPath();
            }
            else
            {
                laserWidth = 0.03f;
                // Keep updating the path (for moving objects)
                UpdateLaserPath();
            }
            return;
        }
        
        // Pulsing mode: handle laser timing cycle
        timer += Time.deltaTime;
    
        // Check if we need to toggle the laser state
        if (isLaserActive && timer >= activeTime)
        {
            isPlayingSFX = false;
            // Turn off the laser
            DeactivateLaser();
            isLaserActive = false;
            activeTimer = 0f; // Reset the active timer
            sunAnimator.SetBool("Charge", true);
        }
        else if (!isLaserActive && timer >= cycleTime)
        {
            if (!isPlayingSFX)
            {
                StartCoroutine(PlaySFXAndAnimationDelay());
            }
          
        }
        
        // If laser is active, update its state
        if (isLaserActive)
        {
            // Track how long the laser has been active
            activeTimer += Time.deltaTime;
            
            // Update the laser path to show the shrinking effect
            UpdateLaserPath();
        }
    }
   
    /// <summary>
    /// Deactivates the laser by removing all segments
    /// </summary>
    private void DeactivateLaser()
    {
        // Destroy all laser segments
        foreach (GameObject segment in laserSegments)
        {
            Destroy(segment);
        }
        laserSegments.Clear();
        hitPoints.Clear();
        hitDeflectors.Clear();
        segmentMaterials.Clear();
        
    }

    IEnumerator PlaySFXAndAnimationDelay()
    {
        if (laserSpawnAudioClip != null)
        {
          
            AudioSource.PlayClipAtPoint(laserSpawnAudioClip, transform.position, laserAudioVolume);
            sunAnimator.SetTrigger("Fire");
            isPlayingSFX = true;
   
            // Reset the timer and turn on the laser
            timer = 0f;
            activeTimer = 0f; // Reset the active timer when laser activates
            isLaserActive = true;
            UpdateLaserPath();
            yield return new WaitForSeconds(0.5f);
            sunAnimator.SetBool("Charge", false);
            sunAnimator.ResetTrigger("Fire");
        }
        else
        {
            Debug.LogWarning("No audio clip assigned to Laser_" + gameObject.name);
        }
    }
    /// <summary>
    /// Manually activate the laser (useful for triggering from other scripts)
    /// </summary>
    public void ActivateLaser()
    {
        if (!isEnabled) return;
        
        isLaserActive = true;
        timer = 0f;
        activeTimer = 0f; // Reset the active timer
        UpdateLaserPath();
    }
    
    /// <summary>
    /// Manually deactivate the laser (useful for triggering from other scripts)
    /// </summary>
    public void ForceDeactivateLaser()
    {
        //isLaserActive = false;
        DeactivateLaser();
    }
    
    /// <summary>
    /// Calculate and visualize the laser path
    /// </summary>
    private void UpdateLaserPath()
    {
        // Clear everything first
        DeactivateLaser();
        
        // If the laser is not active, don't create new segments
        if (!isLaserActive) return;

        // Start position is at the emitter's position
        Vector3 currentPosition = transform.position;
        Vector3 currentDirection = transform.forward;
        
        // Add initial position
        hitPoints.Add(currentPosition);
        
        // Current material starts with default
        Material currentMaterial = defaultLaserMaterial;
        
        // Track deflections
        int deflectionCount = 0;
        
        // Cast the laser and handle deflections - this loop builds the path points
        while (deflectionCount < maxDeflections)
        {
            // Cast a ray from current position in current direction
            if (Physics.Raycast(currentPosition, currentDirection, out RaycastHit hit, maxDistance, layerMask))
            {
                // Add hit position to the list
                hitPoints.Add(hit.point);
                
                // Check if we hit a deflector
                LaserDeflector deflector = hit.collider.GetComponent<LaserDeflector>();
                if (deflector != null)
                {
                    // Track this deflector
                    if (!hitDeflectors.Contains(deflector))
                    {
                        hitDeflectors.Add(deflector);
                    }
                    
                    // Get the exit direction from the deflector
                    Vector3 exitDirection = deflector.GetExitDirection();
                    
                    // Get center point of the deflector
                    Vector3 towerCenter = deflector.transform.position;
                    
                    // Add the center point
                    hitPoints.Add(towerCenter);
                    
                    // See if this deflector changes the material
                    Material deflectorMaterial = deflector.GetLaserMaterial();
                    deflector.FadeColorOut();
                    //Prtiitititititititiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii
                    if (deflectorMaterial != null)
                    {
                        // Update current material for future segments
                        currentMaterial = deflectorMaterial;
                        //Debug.Log($"Deflector {deflector.name} is changing material to {deflectorMaterial.name}");
                    }
                    
                    // Calculate where laser exits the deflector
                    Ray exitRay = new Ray(towerCenter, exitDirection);
                    Vector3 exitPoint;
                    
                    if (hit.collider.Raycast(exitRay, out RaycastHit exitHit, 100f))
                    {
                        exitPoint = exitHit.point;
                    }
                    else
                    {
                        exitPoint = towerCenter + exitDirection * (hit.collider.bounds.extents.magnitude);
                    }
                    
                    // Add exit point
                    hitPoints.Add(exitPoint);
                    
                    // Update for next segment
                    currentPosition = exitPoint + exitDirection * 0.01f;
                    currentDirection = exitDirection;
                    
                    deflectionCount++;
                }
                else
                {
                    // Check if we hit a receiver
                    LaserReceiver receiver = hit.collider.GetComponent<LaserReceiver>();
                    if (receiver != null)
                    {
                        receiver.ReceiveLaser(hitDeflectors, totalDeflectorsInScene);
                    }
                    
                    // End the path here
                    break;
                }
            }
            else
            {
                // Nothing hit - add the end position of the ray
                hitPoints.Add(currentPosition + currentDirection * maxDistance);
                break;
            }
        }
        
        // Now that we have all points, determine what materials to use between each point
        // We start with the default material
        currentMaterial = defaultLaserMaterial;
        
        // Now create segments between all points - each segment could have a different material
        for (int i = 0; i < hitPoints.Count - 1; i++)
        {
            Vector3 startPoint = hitPoints[i];
            Vector3 endPoint = hitPoints[i+1];
            
            // Check if we're entering or exiting a deflector
            bool isExitingDeflector = false;
            
            // Check if this segment is exiting from a deflector center (i.e., if the start point matches a deflector position)
            foreach (LaserDeflector deflector in hitDeflectors)
            {
                if (Vector3.Distance(startPoint, deflector.transform.position) < 0.01f)
                {
                    // This segment is exiting a deflector
                    isExitingDeflector = true;
                    
                    // Check if this deflector has a material
                    Material deflectorMaterial = deflector.GetLaserMaterial();
                    if (deflectorMaterial != null)
                    {
                        // Use the deflector's material for this segment
                        currentMaterial = deflectorMaterial;
                            //Debug.Log($"Segment {i} is exiting deflector {deflector.name}, using material {deflectorMaterial.name}");
                    }
                    break;
                }
            }
            
            // If not exiting a deflector, we keep using the current material
            if (!isExitingDeflector)
            {
                //Debug.Log($"Segment {i} is using current material {currentMaterial?.name ?? "null"}");
            }
            
            // Create the segment with the appropriate material
            GameObject segment = CreateLaserSegment(startPoint, endPoint, currentMaterial);
            laserSegments.Add(segment);
            
            // Store the material we used
            segmentMaterials.Add(currentMaterial);
        }

    }
    
    /// <summary>
    /// Create a single laser segment between two points with specified material
    /// </summary>
    private GameObject CreateLaserSegment(Vector3 start, Vector3 end, Material material)
    {
        GameObject segment;
        
        if (laserSegmentPrefab != null)
        {
            // Instantiate the prefab
            segment = Instantiate(laserSegmentPrefab);
            
            // Apply material if provided
            Renderer renderer = segment.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                // Use the provided material
                renderer.material = material;
                
            }
            else
            {
                //Debug.LogWarning($"Could not apply material: renderer={renderer != null}, material={material != null}");
            }
        }
        else
        {
            // Create a default cube if no prefab is provided
            segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            // Apply the material if provided
            Renderer renderer = segment.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.material = material;
            }
        }
        
        // Give the segment a descriptive name
        segment.name = "LaserSegment_" + laserSegments.Count;
        
        // Make the segment a child of our laser parent for organization
        segment.transform.parent = laserParent;
        
        // Position and scale the segment to form a laser beam
        Vector3 midPoint = (start + end) / 2f;
        segment.transform.position = midPoint;
        
        // Calculate direction and look at the end point
        Vector3 direction = (end - start).normalized;
        segment.transform.forward = direction;
        
        // Calculate the current width based on the shrinking effect
        float currentWidth = CalculateCurrentLaserWidth();
        
        // Scale the segment with the calculated width
        float length = Vector3.Distance(start, end);
        segment.transform.localScale = new Vector3(currentWidth, currentWidth, length);
        
        // Disable collision on the segment
        Collider collider = segment.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        return segment;
    }
    
    /// <summary>
    /// Calculate the current width of the laser based on how long it has been active
    /// </summary>
    /// <returns>The current width to use for the laser</returns>
    private float CalculateCurrentLaserWidth()
    {
        // If we haven't reached the shrink delay, return full width
        if (activeTimer <= shrinkDelay)
        {
            return laserWidth;
        }
        
        // Calculate how far through the shrinking process we are
        float shrinkDuration = activeTime - shrinkDelay;
        float shrinkProgress = (activeTimer - shrinkDelay) / shrinkDuration;
        
        // Clamp to ensure we don't go below 0
        shrinkProgress = Mathf.Clamp01(shrinkProgress);
        
        // Linear interpolation from full width to 0
        return Mathf.Lerp(laserWidth, 0f, shrinkProgress);
    }
}