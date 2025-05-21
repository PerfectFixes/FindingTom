using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class WindParticleSystem : MonoBehaviour
{
    private ParticleSystem windParticles;
    
    [Header("Wind Settings")]
    [SerializeField] private float windStrength = 5f;
    [SerializeField] private float windDistance = 3f;
    [SerializeField] private int particleCount = 50;
    
    [Header("Box Shape Settings")]
    [SerializeField] private Vector3 boxDimensions = new Vector3(2f, 2f, 0.1f);
    [SerializeField] private bool emitFromVolume = false;
    
    [Header("Activation Settings")]
    [SerializeField] private float activationTime = 3f;
    [SerializeField] private float deactivationTime = 2f;
    
    [Header("Appearance")]
    [SerializeField] private Color windColor = new Color(0.8f, 0.8f, 1f, 0.1f);
    [SerializeField] private float startSize = 0.3f;
    [SerializeField] private float endSize = 0.8f;
    
    // Runtime variables
    private float currentEmissionRate = 0f;
    private float targetEmissionRate = 0f;
    private float transitionStartTime;
    private bool isTransitioning = false;
    private float transitionDuration;
    
    void Start()
    {
        InitializeParticleSystem();
        
        // Start with wind off
        windParticles.Stop();
        currentEmissionRate = 0f;
        var emission = windParticles.emission;
        emission.rateOverTime = 0f;
    }
    
    void Update()
    {
        if (isTransitioning)
        {
            UpdateTransition();
        }
    }

    void UpdateTransition()
    {
        float elapsed = Time.time - transitionStartTime;
        float progress = Mathf.Clamp01(elapsed / transitionDuration);
        
        // Smooth interpolation
        float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
        
        // Update emission rate
        float previousRate = currentEmissionRate;
        currentEmissionRate = Mathf.Lerp(previousRate, targetEmissionRate, easedProgress);
        
        // Apply to particle system
        var emission = windParticles.emission;
        emission.rateOverTime = currentEmissionRate;
        
        // Stop transitioning when complete
        if (progress >= 1f)
        {
            isTransitioning = false;
            currentEmissionRate = targetEmissionRate;
            
            // Stop particle system completely if deactivating
            if (targetEmissionRate == 0f)
            {
                windParticles.Stop();
            }
        }
    }

    void InitializeParticleSystem()
    {
        windParticles = GetComponent<ParticleSystem>();
        
        // Configure main module
        var main = windParticles.main;
        main.startSpeed = windStrength;
        main.startLifetime = windDistance / windStrength;
        main.startSize = startSize;
        main.startColor = windColor;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        
        // Configure shape module for box emission
        var shape = windParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = boxDimensions;
        shape.position = Vector3.zero;
        shape.rotation = Vector3.zero;
        
        // Emit from surface or volume
        if (emitFromVolume)
        {
            shape.boxThickness = Vector3.one; // Fill the entire volume
            shape.radiusThickness = 1f;
        }
        else
        {
            shape.boxThickness = new Vector3(0f, 0f, 1f); // Emit only from the z-plane
            shape.radiusThickness = 0f;
        }
        
        // Configure emission module
        var emission = windParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = particleCount / (windDistance / windStrength);
        
        // Configure size over lifetime
        var sizeOverLifetime = windParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.separateAxes = false;
        AnimationCurve sizeGraph = new AnimationCurve();
        sizeGraph.AddKey(0f, startSize);
        sizeGraph.AddKey(0.7f, endSize);
        sizeGraph.AddKey(1f, endSize);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeGraph);
        
        // Configure color over lifetime for fade effect
        var colorOverLifetime = windParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(windColor, 0f), 
                new GradientColorKey(windColor, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0f, 0f), 
                new GradientAlphaKey(0.3f, 0.1f), 
                new GradientAlphaKey(0.1f, 0.8f), 
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        // Configure texture sheet animation for wind streaks
        var textureSheet = windParticles.textureSheetAnimation;
        textureSheet.enabled = false; // Enable if you have a texture sheet
        
        // Configure renderer
        var renderer = windParticles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale = 0.1f;
        renderer.lengthScale = 0.5f;
        renderer.material = CreateWindMaterial();
        renderer.sortingOrder = 1;
        
        // Optional: Add force field effect if needed
        var velocityOverLifetime = windParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = false; // Enable for additional wind patterns
        
        // Configure noise for more natural movement
        var noise = windParticles.noise;
        noise.enabled = true;
        noise.separateAxes = true;
        noise.strength = new ParticleSystem.MinMaxCurve(0.1f);
        noise.frequency = 0.5f;
        noise.scrollSpeed = new ParticleSystem.MinMaxCurve(0.3f);
        noise.octaveCount = 1;
        noise.octaveMultiplier = 0.5f;
        noise.octaveScale = 2f;
        noise.quality = ParticleSystemNoiseQuality.Medium;
    }

    private Material CreateWindMaterial()
    {
        // Create a simple material for the particles
        // You can replace this with your custom material
        Material windMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
        windMaterial.SetColor("_ColorBias", Color.white);
        windMaterial.SetFloat("_ColorAddition", 0f);
        windMaterial.SetFloat("_ColorSubtraction", 0f);
        windMaterial.SetFloat("_AlphaMultiplier", 1f);
        
        // Enable softness
        windMaterial.SetFloat("_SoftParticlesEnabled", 1f);
        windMaterial.SetFloat("_CameraFadingEnabled", 0f);
        
        // Set to additive blending for better visibility
        windMaterial.SetFloat("_Mode", 4f); // Additive
        windMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        windMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        windMaterial.SetFloat("_InvFade", 3f);
        
        return windMaterial;
    }

    // Function to activate wind with gradual increase
    public void ActivateWind()
    {
        if (windParticles == null) return;
        
        // Start particle system if not already running
        if (!windParticles.isPlaying)
        {
            windParticles.Play();
        }
        
        // Set up transition
        isTransitioning = true;
        transitionStartTime = Time.time;
        transitionDuration = activationTime;
        targetEmissionRate = particleCount / (windDistance / windStrength);
    }

    // Function to deactivate wind with gradual decrease
    public void DeactivateWind()
    {
        if (windParticles == null) return;
        
        // Set up transition
        isTransitioning = true;
        transitionStartTime = Time.time;
        transitionDuration = deactivationTime;
        targetEmissionRate = 0f;
    }

    // Alternative activation with custom time
    public void ActivateWind(float customActivationTime)
    {
        activationTime = customActivationTime;
        ActivateWind();
    }

    // Alternative deactivation with custom time
    public void DeactivateWind(float customDeactivationTime)
    {
        deactivationTime = customDeactivationTime;
        DeactivateWind();
    }

    // Check if wind is currently active
    public bool IsWindActive()
    {
        return windParticles != null && windParticles.isPlaying && currentEmissionRate > 0f;
    }

    // Get current wind intensity (0-1)
    public float GetCurrentWindIntensity()
    {
        if (windParticles == null) return 0f;
        float maxEmission = particleCount / (windDistance / windStrength);
        return currentEmissionRate / maxEmission;
    }

    // Optional: Method to control wind dynamically
    public void SetWindStrength(float strength)
    {
        // Optional: Enable dynamic control with existing wind strength method
        windStrength = strength;
        if (windParticles != null)
        {
            var main = windParticles.main;
            main.startSpeed = windStrength;
            main.startLifetime = windDistance / windStrength;
            
            // Update emission rate if wind is active
            if (IsWindActive() && !isTransitioning)
            {
                var emission = windParticles.emission;
                currentEmissionRate = particleCount / (windDistance / windStrength);
                emission.rateOverTime = currentEmissionRate;
            }
        }
    }
    
    // Adjust the box size dynamically for different corridor dimensions
    public void SetBoxDimensions(Vector3 dimensions)
    {
        boxDimensions = dimensions;
        if (windParticles != null)
        {
            var shape = windParticles.shape;
            shape.scale = boxDimensions;
        }
    }

    // Optional: Adjust wind direction
    public void SetWindDirection(Vector3 direction)
    {
        if (windParticles != null)
        {
            transform.forward = direction.normalized;
        }
    }

#if UNITY_EDITOR
    // Helper method to visualize wind in editor
    void OnDrawGizmos()
    {
        // Draw box shape
        Gizmos.color = Color.cyan;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, boxDimensions);
        Gizmos.matrix = Matrix4x4.identity;
        
        // Draw wind direction
        Gizmos.color = Color.yellow;
        Vector3 endPoint = transform.position + transform.forward * windDistance;
        
        // Draw lines from corners of box to end point to show flow direction
        Vector3[] corners = new Vector3[4];
        corners[0] = transform.TransformPoint(new Vector3(-boxDimensions.x * 0.5f, -boxDimensions.y * 0.5f, 0));
        corners[1] = transform.TransformPoint(new Vector3(boxDimensions.x * 0.5f, -boxDimensions.y * 0.5f, 0));
        corners[2] = transform.TransformPoint(new Vector3(boxDimensions.x * 0.5f, boxDimensions.y * 0.5f, 0));
        corners[3] = transform.TransformPoint(new Vector3(-boxDimensions.x * 0.5f, boxDimensions.y * 0.5f, 0));
        
        foreach (Vector3 corner in corners)
        {
            Gizmos.DrawLine(corner, corner + transform.forward * windDistance);
        }
        
        // Draw box at end of wind flow
        Matrix4x4 endMatrix = Matrix4x4.TRS(endPoint, transform.rotation, Vector3.one);
        Gizmos.matrix = endMatrix;
        Gizmos.DrawWireCube(Vector3.zero, boxDimensions);
        Gizmos.matrix = Matrix4x4.identity;
        
        // Draw direction indicator
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPoint, 0.1f);
    }
#endif
}