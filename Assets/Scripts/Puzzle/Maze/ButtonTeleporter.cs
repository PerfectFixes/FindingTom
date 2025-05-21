using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ButtonTeleporter : MonoBehaviour
{
    [Header("Teleport Points")]
    [SerializeField] private Transform buttonATeleportPoint;
    [SerializeField] private Transform buttonBTeleportPoint;
    
    [Header("Button References")]
    [SerializeField] private GameObject buttonA;
    [SerializeField] private GameObject buttonB;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem buttonAEffect;
    [SerializeField] private ParticleSystem buttonBEffect;
    [SerializeField] private float effectDuration = 1.0f;
    
    [Header("Events")]
    public UnityEvent OnTeleportFromAToB;
    public UnityEvent OnTeleportFromBToA;
    
    // State tracking
    private bool canTeleport = true;
    private GameObject lastPressedButton;
    
    // Called by your existing button press event on Button A
    public void OnButtonAPressed(GameObject ball)
    {
        if (canTeleport && lastPressedButton != buttonA)
        {
            // Teleport ball to Button B's position
            TeleportBall(ball, buttonBTeleportPoint.position);
            
            // Set the last pressed button
            lastPressedButton = buttonA;
            
            // Disable teleportation until ball gets off the destination button
            canTeleport = false;
            
            // Play teleport effects
            PlayTeleportEffects(buttonAEffect, buttonBEffect);
            
            // Trigger teleport event
            OnTeleportFromAToB?.Invoke();
        }
    }
    
    // Called by your existing button press event on Button B
    public void OnButtonBPressed(GameObject ball)
    {
        if (canTeleport && lastPressedButton != buttonB)
        {
            // Teleport ball to Button A's position
            TeleportBall(ball, buttonATeleportPoint.position);
            
            // Set the last pressed button
            lastPressedButton = buttonB;
            
            // Disable teleportation until ball gets off the destination button
            canTeleport = false;
            
            // Play teleport effects
            PlayTeleportEffects(buttonBEffect, buttonAEffect);
            
            // Trigger teleport event
            OnTeleportFromBToA?.Invoke();
        }
    }
    
    // Called by your existing button unpress event on Button A
    public void OnButtonAUnpressed()
    {
        if (lastPressedButton == buttonB)
        {
            // If this was the destination button, enable teleportation again
            canTeleport = true;
        }
    }
    
    // Called by your existing button unpress event on Button B
    public void OnButtonBUnpressed()
    {
        if (lastPressedButton == buttonA)
        {
            // If this was the destination button, enable teleportation again
            canTeleport = true;
        }
    }
    
    // Helper method to handle the actual teleportation
    private void TeleportBall(GameObject ball, Vector3 targetPosition)
    {
        // Option 1: Simple position change
        ball.transform.position = targetPosition;
        
        // Option 2: If using Rigidbody, you might want to reset velocity
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    
    // Helper method to play teleport effects at both source and destination
    private void PlayTeleportEffects(ParticleSystem sourceEffect, ParticleSystem destinationEffect)
    {
        if (sourceEffect != null)
        {
            sourceEffect.Play();
        }
        
        if (destinationEffect != null)
        {
            destinationEffect.Play();
        }
        
        // Optional: Add audio effect
        StartCoroutine(StopEffectsAfterDelay(sourceEffect, destinationEffect));
    }
    
    // Coroutine to stop effects after the specified duration
    private IEnumerator StopEffectsAfterDelay(ParticleSystem sourceEffect, ParticleSystem destinationEffect)
    {
        yield return new WaitForSeconds(effectDuration);
        
        if (sourceEffect != null && sourceEffect.isPlaying)
        {
            sourceEffect.Stop();
        }
        
        if (destinationEffect != null && destinationEffect.isPlaying)
        {
            destinationEffect.Stop();
        }
    }
}