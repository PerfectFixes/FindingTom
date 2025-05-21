using System;
using UnityEngine;

public class DoorInteraction : MonoBehaviour
{

    private Animator animator;
    
    private Canvas interactCanvas;
    
    private bool isDoorOpen;
    

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
        interactCanvas = GetComponentInChildren<Canvas>();
        interactCanvas.gameObject.SetActive(false); 
    }

    public void DisplayUI()
    {
        if (isDoorOpen)
        {
            return;
        }
        interactCanvas.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        if (isDoorOpen)
        {
            return;
        }
        interactCanvas.gameObject.SetActive(false);
    }
    public void OpenDoor()
    {
        animator.SetTrigger("OpenDoor");
        interactCanvas.gameObject.SetActive(false);
        isDoorOpen = true;

    }
}
