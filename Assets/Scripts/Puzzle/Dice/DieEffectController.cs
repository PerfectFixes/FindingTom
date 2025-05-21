using System.Collections;
using UnityEngine;

public class DieEffectController : MonoBehaviour
{
    [Header("Effect Settings")]
    [Tooltip("Duration of the effect in seconds")]
    public float effectDuration = 2f;
    
    [Tooltip("Particle system to play (if any)")]
    public ParticleSystem effectParticles;
    
    [Tooltip("Audio to play (if any)")]
    public AudioClip effectSound;
    
    [Tooltip("Scale animation settings")]
    public bool useScaleAnimation = true;
    public float maxScale = 1.5f;
    public float scaleSpeed = 5f;
    
    [Header("Rotation Animation")]
    public bool useRotationAnimation = true;
    public float rotationSpeed = 180f;
    
    [Header("Color Settings")]
    public bool useColorPulse = true;
    public Color pulseColor = Color.white;
    public float pulseSpeed = 3f;
    
    private AudioSource audioSource;
    private new Renderer renderer;
    private Color originalColor;
    private bool isPlaying = false;
    
    private void Awake()
    {
        // Get renderer if we want to use color pulse
        if (useColorPulse)
        {
            renderer = GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_Color"))
            {
                originalColor = renderer.material.color;
            }
        }
        
        // Add an audio source if we have a sound
        if (effectSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = effectSound;
            audioSource.playOnAwake = false;
        }
    }
    
    public void PlayEffect()
    {
        if (isPlaying)
            return;
            
        isPlaying = true;
        
        // Play particle system if we have one
        if (effectParticles != null)
        {
            effectParticles.Play();
        }
        
        // Play sound if we have one
        if (audioSource != null)
        {
            audioSource.Play();
        }
        
        // Start animations
        if (useScaleAnimation)
        {
            StartCoroutine(ScaleAnimation());
        }
        
        if (useRotationAnimation)
        {
            StartCoroutine(RotationAnimation());
        }
        
        if (useColorPulse && renderer != null)
        {
            StartCoroutine(ColorPulseAnimation());
        }
        
        // Destroy the effect after duration
        Destroy(gameObject, effectDuration);
    }
    
    private IEnumerator ScaleAnimation()
    {
        float timer = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 maxScaleVec = Vector3.one * maxScale;
        
        // Scale up
        while (timer < effectDuration / 2f)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / (effectDuration / 2f));
            transform.localScale = Vector3.Lerp(startScale, maxScaleVec, t);
            yield return null;
        }
        
        // Scale down
        timer = 0f;
        while (timer < effectDuration / 2f)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / (effectDuration / 2f));
            transform.localScale = Vector3.Lerp(maxScaleVec, Vector3.zero, t);
            yield return null;
        }
    }
    
    private IEnumerator RotationAnimation()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < effectDuration)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator ColorPulseAnimation()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < effectDuration && renderer != null)
        {
            // Ping-pong between original color and pulse color
            float t = Mathf.PingPong(elapsedTime * pulseSpeed, 1f);
            renderer.material.color = Color.Lerp(originalColor, pulseColor, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Reset to original color
        if (renderer != null)
        {
            renderer.material.color = originalColor;
        }
    }
}