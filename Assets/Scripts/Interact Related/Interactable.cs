using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onInteract;
    
    [SerializeField]
    private UnityEvent OnInteractEnter;
    
    [SerializeField]
    private UnityEvent OnInteractExit;
    
    [Header("SFX")]
    public AudioClip interactAudioClip;
    public AudioSource AudioSource;
    [Range(0, 1)] public float interactAudioVolume = 0.5f;
    
    private bool isPlayerTouching = false;
    
    private bool isInteracting = true;

    private void Awake()
    {
        isInteracting = false;
    }

    public void PlayerTouch()
    {
        if (isPlayerTouching)
        {
            return;
        }
        isPlayerTouching = true;
        InteractEnter();
    }

    private void InteractEnter()
    {
        OnInteractEnter?.Invoke();
    }
    public void InteractExit()
    {
        isPlayerTouching = false;
        OnInteractExit?.Invoke();
    }
    public void Interact()
    {
        if (isInteracting)
        {
            return;
        }
        if (interactAudioClip != null)
        {
            AudioSource.PlayOneShot(interactAudioClip, interactAudioVolume);
        }
        else
        {
            Debug.LogWarning("Audio Clip Not Found");
        }
        //the ? means a null check for onInteract, (if onInteract != null)
        onInteract?.Invoke();
        StartCoroutine(AddInteractionCooldown());
    }

    IEnumerator AddInteractionCooldown()
    {
        isInteracting = true;
        yield return new WaitForSeconds(2f);
        isInteracting = false;
    }
}
