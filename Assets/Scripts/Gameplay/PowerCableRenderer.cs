using UnityEngine;
using System.Collections.Generic;

// Creates a power cable effect using LineRenderer with adjustable properties
public class PowerCableRenderer : MonoBehaviour
{
    [Tooltip("Reference to the LineRenderer component")]
    [SerializeField] private LineRenderer lineRenderer;

    [Tooltip("List of anchor points for the cable")]
    [SerializeField] private List<Transform> anchorPoints = new List<Transform>();

    [Tooltip("Number of points to generate between each anchor point")]
    [SerializeField] private int pointsBetweenAnchors = 10;

    [Tooltip("Controls how tight the cable appears (0-1 range)")]
    [Range(0f, 1f)]
    [SerializeField] private float cableTightness = 0.5f;

    [Tooltip("Amount of bend/sag in the cable")]
    [Range(0f, 1f)]
    [SerializeField] private float bendAmount = 0.5f;

    [Tooltip("Influences the random variation of points")]
    [Range(0f, 0.5f)]
    [SerializeField] private float noiseAmount = 0.05f;

    [Tooltip("Controls cable thickness")]
    [SerializeField] private float cableWidth = 0.1f;

    [Tooltip("Apply small movement to simulate wind or vibration")]
    [SerializeField] private bool applyDynamicMovement = false;

    [Tooltip("Speed of dynamic movement")]
    [SerializeField] private float dynamicMovementSpeed = 1f;

    [Tooltip("Magnitude of dynamic movement")]
    [Range(0f, 0.2f)]
    [SerializeField] private float dynamicMovementMagnitude = 0.05f;

    // Seed for consistent random generation
    private int noiseSeed;
    private Vector3[] cablePoints;

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                SetupDefaultLineRenderer();
            }
        }

        noiseSeed = Random.Range(0, 10000);
    }

    private void SetupDefaultLineRenderer()
    {
        lineRenderer.startWidth = cableWidth;
        lineRenderer.endWidth = cableWidth;
        lineRenderer.positionCount = 0;
        
        // You might want to set up a default material here
        // lineRenderer.material = yourDefaultMaterial;
    }

    private void Update()
    {
        if (anchorPoints.Count >= 2)
        {
            GenerateCable();
        }
    }

    // Generates the cable points based on current settings
    private void GenerateCable()
    {
        // Calculate total points needed
        int totalSegments = anchorPoints.Count - 1;
        int totalPoints = (pointsBetweenAnchors * totalSegments) + anchorPoints.Count;
        
        // Initialize or resize the points array if needed
        if (cablePoints == null || cablePoints.Length != totalPoints)
        {
            cablePoints = new Vector3[totalPoints];
        }

        lineRenderer.positionCount = totalPoints;
        
        int currentPoint = 0;

        // Process each segment between anchor points
        for (int i = 0; i < anchorPoints.Count - 1; i++)
        {
            Transform startAnchor = anchorPoints[i];
            Transform endAnchor = anchorPoints[i + 1];
            
            if (startAnchor == null || endAnchor == null) continue;

            Vector3 startPos = startAnchor.position;
            Vector3 endPos = endAnchor.position;

            // Calculate direction and midpoint
            Vector3 direction = endPos - startPos;
            Vector3 midPoint = startPos + direction * 0.5f;
            
            // Calculate perpendicular vector (primarily downward for gravity effect)
            Vector3 perpendicular = Vector3.down;
            // Ensure it's actually perpendicular to the direction
            perpendicular = Vector3.Cross(Vector3.Cross(direction, perpendicular).normalized, direction).normalized;
            
            // Determine sag amount based on distance and bend setting
            float sagAmount = direction.magnitude * bendAmount * (1f - cableTightness);
            
            // Add the start point
            cablePoints[currentPoint] = startPos;
            currentPoint++;

            // Generate in-between points
            for (int p = 1; p <= pointsBetweenAnchors; p++)
            {
                float t = (float)p / (pointsBetweenAnchors + 1);
                
                // Basic linear interpolation
                Vector3 linearPos = Vector3.Lerp(startPos, endPos, t);
                
                // Apply curve using quadratic function for sagging effect
                // Parabola peaks at t=0 and t=1, and reaches its lowest at t=0.5
                float parabola = -4f * (t * t - t);
                Vector3 curveDisplacement = perpendicular * sagAmount * parabola;
                
                // Add some noise if desired
                Vector3 noise = Vector3.zero;
                if (noiseAmount > 0)
                {
                    float noiseT = t * 10f + i * 10f + noiseSeed;
                    noise = new Vector3(
                        Mathf.PerlinNoise(noiseT, 0) - 0.5f,
                        Mathf.PerlinNoise(0, noiseT) - 0.5f,
                        Mathf.PerlinNoise(noiseT, noiseT) - 0.5f
                    ) * noiseAmount * direction.magnitude;
                }
                
                // Apply dynamic movement if enabled
                Vector3 dynamicMovement = Vector3.zero;
                if (applyDynamicMovement)
                {
                    float dynamicT = Time.time * dynamicMovementSpeed + i * 3.7f + t * 6.3f;
                    dynamicMovement = new Vector3(
                        Mathf.Sin(dynamicT * 1.3f),
                        Mathf.Sin(dynamicT * 0.7f),
                        Mathf.Sin(dynamicT * 1.1f)
                    ) * dynamicMovementMagnitude * direction.magnitude * parabola;
                }
                
                // Combine all effects
                cablePoints[currentPoint] = linearPos + curveDisplacement + noise + dynamicMovement;
                currentPoint++;
            }

            // Don't add the endpoint except for the last segment
            if (i == anchorPoints.Count - 2)
            {
                cablePoints[currentPoint] = endPos;
                currentPoint++;
            }
        }

        // Apply all points to the line renderer
        lineRenderer.SetPositions(cablePoints);
    }

    // Add a new anchor point to the cable
    public void AddAnchorPoint(Transform newPoint)
    {
        if (newPoint != null && !anchorPoints.Contains(newPoint))
        {
            anchorPoints.Add(newPoint);
        }
    }

    // Remove an anchor point from the cable
    public void RemoveAnchorPoint(Transform point)
    {
        if (point != null && anchorPoints.Contains(point))
        {
            anchorPoints.Remove(point);
        }
    }

    // Remove the anchor point at the specified index
    public void RemoveAnchorPointAt(int index)
    {
        if (index >= 0 && index < anchorPoints.Count)
        {
            anchorPoints.RemoveAt(index);
        }
    }
    
    // Set cable tightness (0-1 range)
    public void SetCableTightness(float tightness)
    {
        cableTightness = Mathf.Clamp01(tightness);
    }
    
    // Set cable bend amount (0-1 range)
    public void SetBendAmount(float bend)
    {
        bendAmount = Mathf.Clamp01(bend);
    }
    
    // Set cable thickness
    public void SetCableWidth(float width)
    {
        cableWidth = Mathf.Max(0.01f, width);
        lineRenderer.startWidth = cableWidth;
        lineRenderer.endWidth = cableWidth;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw spheres at anchor points for easier visualization in editor
        Gizmos.color = Color.yellow;
        foreach (var point in anchorPoints)
        {
            if (point != null)
            {
                Gizmos.DrawSphere(point.position, 0.1f);
            }
        }
    }
#endif
}