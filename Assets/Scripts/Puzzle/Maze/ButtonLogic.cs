using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ButtonLogic : MonoBehaviour
{
    [SerializeField] UnityEvent onButtonPressed;
    [SerializeField] UnityEvent onButtonUnpressed;
    [SerializeField] private bool canBePressedOnce = true;
    private bool hasButtonPressed = false;
    private Animator animator;
    
    [Header("SFX")]
    public AudioClip pressAudioClip;
    public AudioClip unpressAudioClip;
    public AudioSource audioSource;
    [Range(0, 1)] public float endAudioVolume = 0.5f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GameObject.FindGameObjectWithTag("Audio Source Maze").GetComponent<AudioSource>();
    }
    public void BallOnButton()
    {
        SquishButton();
        if (!audioSource.isPlaying)
        {
              
            if (pressAudioClip != null)
            {
                print("Player SFX");
                audioSource.PlayOneShot(pressAudioClip, endAudioVolume);
            }
            else
            {
                Debug.LogWarning("No audio clip assigned to button press" + gameObject.name);
            }
        }
        
        if (hasButtonPressed && canBePressedOnce)
        {
            return;
        }
        hasButtonPressed = true;
        onButtonPressed?.Invoke();
    }

    private void SquishButton()
    {
        animator.SetBool("ButtonPress", true);
    }

    private void UnsquishButton()
    {
        animator.SetBool("ButtonPress", false);
        animator.SetBool("Release",false);
    }
    public void OnButtonUnpressed()
    {
        UnsquishButton();
        if (!audioSource.isPlaying)
        {
              
            if (unpressAudioClip != null)
            {
                print("Player SFX");
                audioSource.PlayOneShot(unpressAudioClip, endAudioVolume);
            }
            else
            {
                Debug.LogWarning("No audio clip assigned to unpress" + gameObject.name);
            }
        }
        animator.SetBool("Release",true);
        //Player animation of button unsmushed
        onButtonUnpressed?.Invoke();
    }
}
