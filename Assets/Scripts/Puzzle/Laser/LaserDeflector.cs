using System;
using UnityEngine;
using System.Collections;

public class LaserDeflector : MonoBehaviour
{
    [Header("Deflection Settings")]

    [Tooltip("The angle to deflect the laser in degrees (from the tower's forward direction)")]
    [Range(-180, 180)]
    public float deflectionAngle = -45f;

    [Header("Visual Settings")] [Tooltip("Material to apply to the laser after deflection")]
    public Material laserMaterial;

    // Store original colors for gizmos
    private Color gizmoIncomingColor = Color.yellow;
    private Color gizmoOutgoingColor = Color.green;
    [SerializeField]
    private Renderer objectRenderer;
    private Material instancedMaterial;

    [Tooltip("Time to wait before starting the fade")]
    public float pauseTime = 0.2f;

    [Tooltip("Duration of the fade effect")]
    public float fadeTime = 1.2f;

    [Tooltip("The material to fade back to")]
    public Material defaultMaterial;

    private bool isFading = false;

    private LaserEmitter laserEmitter;

    private void Awake()
    {
        laserEmitter = GameObject.FindWithTag("Laser Emitter").GetComponent<LaserEmitter>();
        //objectRenderer = GetComponent<Renderer>();

        // Create an instanced material from the default material
        // This way we can modify it without affecting the original asset
        if (defaultMaterial != null)
        {
            instancedMaterial = new Material(defaultMaterial);
            objectRenderer.material = instancedMaterial;
        }
    }

    private void OnDestroy()
    {
        // Clean up instanced materials when the object is destroyed
        if (instancedMaterial != null)
        {
            Destroy(instancedMaterial);
        }
    }

    // Called by Unity when this component is selected in editor
    private void OnDrawGizmosSelected()
    {
        // Draw a visual representation of the entry and exit rays
        Gizmos.color = gizmoIncomingColor;
        // Incoming ray (assuming from left side)
        Vector3 incoming = Vector3.left;
        Gizmos.DrawRay(transform.position, incoming.normalized * 2);

        // Outgoing ray based on deflection angle
        Gizmos.color = laserMaterial != null ? gizmoOutgoingColor : Color.white;
        Vector3 outgoing = GetExitDirection();
        Gizmos.DrawRay(transform.position, outgoing.normalized * 2);
    }

    public Renderer GetDeflectorRenderer()
    {
        return objectRenderer;
    }

    // Returns the exit direction of the laser based on the deflection angle
    public Vector3 GetExitDirection()
    {
        // Create a rotation around the up vector based on the deflection angle
        // This gives us a direction relative to the tower's orientation
        Quaternion rotation = Quaternion.Euler(0, deflectionAngle, 0);

        // Apply this rotation to the forward direction of the tower
        // This means at 0 degrees, the laser continues straight forward
        Vector3 exitDirection = rotation * transform.forward;

        return exitDirection.normalized;
    }

    // Public method to set the deflection angle, useful for inspectors/editors
    public void SetDeflectionAngle(float newAngle)
    {
        deflectionAngle = Mathf.Clamp(newAngle, -180f, 180f);
    }
    
    // Returns the material to be applied to the laser after it hits this deflector
    public Material GetLaserMaterial()
    {
        return laserMaterial;
    }

    public void FadeColorOut()
    {
        // Only start a new fade if we're not already fading
        if (!isFading)
        {
            StopAllCoroutines();
            StartCoroutine(FadeToDefaultColor());
        }
    }

    private IEnumerator FadeToDefaultColor()
    {
        if (!isFading)
        {
            isFading = true;

            // Extract colors from materials
            Color laserColor = GetColorFromMaterial(laserMaterial);
            Color defaultColor = GetColorFromMaterial(defaultMaterial);

            // Ensure we have a material instance to modify
            if (instancedMaterial == null && objectRenderer != null)
            {
                instancedMaterial = new Material(objectRenderer.material);
                objectRenderer.material = instancedMaterial;
            }

            // Set the initial color to the laser color
            if (instancedMaterial.HasProperty("_Color"))
            {
                instancedMaterial.color = laserColor;
            }


            // Wait for the pause time before starting the fade
            yield return new WaitForSeconds(pauseTime);
            if (!laserEmitter.isContinuous)
            {
                // Start the fade timer
                float elapsedTime = 0;

                // Perform the gradual fade
                while (elapsedTime < fadeTime)
                {
                    // Calculate the interpolation factor (0 to 1)
                    float t = elapsedTime / fadeTime;

                    // Use a smooth step function for more natural easing
                    float smoothT = Mathf.SmoothStep(0, 1, t);

                    // Interpolate the color
                    if (instancedMaterial.HasProperty("_Color"))
                    {
                        Color lerpedColor = Color.Lerp(laserColor, defaultColor, smoothT);
                        instancedMaterial.color = lerpedColor;
                    }

                    // Wait for the next frame
                    yield return null;

                    // Update the elapsed time
                    elapsedTime += Time.deltaTime;
                }

                // Ensure we end with the exact target color
                if (instancedMaterial.HasProperty("_Color"))
                {
                    instancedMaterial.color = defaultColor;
                }

                isFading = false;
            }
        }
    }
    
    // Helper method to safely extract a color from a material
    private Color GetColorFromMaterial(Material material)
    {
        if (material != null && material.HasProperty("_Color"))
        {
            return material.color;
        }

        return Color.white;
    }
}