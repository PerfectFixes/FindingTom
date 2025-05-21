using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using System.Collections.Generic;

public class DoorInteractionOld : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 2.0f;
    [SerializeField] private LayerMask doorLayer;
    [SerializeField] private Transform playerCamera;
    
    // Reference to the Input System
    [SerializeField] private InputActionReference interactAction;
    
    // Cache for doors that have been opened
    private HashSet<int> openedDoors = new HashSet<int>();
    
    // Current interaction state
    private Animator currentDoorAnimator;
    private Canvas currentDoorCanvas;
    private GameObject currentDoorObject;
    private bool canInteract = false;
    
    // Cache for found canvases to avoid GetComponent calls
    private Dictionary<int, Canvas> doorCanvasCache = new Dictionary<int, Canvas>();

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();
        }
        
        HideCurrentDoorCanvas();
    }

    private void Update()
    {
        CheckForDoorInteraction();
    }

    private void CheckForDoorInteraction()
    {
        // Hide current canvas first to prevent overlapping UI
        if (currentDoorObject != null)
        {
            HideCurrentDoorCanvas();
            currentDoorAnimator = null;
            currentDoorObject = null;
            canInteract = false;
        }

        // Cast a ray to detect doors
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, interactionDistance, doorLayer))
        {
            GameObject doorObject = hit.collider.gameObject;
            int doorID = doorObject.GetInstanceID();
            
            // Skip already opened doors
            if (openedDoors.Contains(doorID))
            {
                return;
            }
            
            // Get door animator
            Animator doorAnimator = doorObject.GetComponent<Animator>();
            if (doorAnimator == null)
            {
                return;
            }
            
            // Store references
            currentDoorAnimator = doorAnimator;
            currentDoorObject = doorObject;
            canInteract = true;
            
            // Get canvas - check cache first to avoid GetComponent calls
            if (!doorCanvasCache.TryGetValue(doorID, out currentDoorCanvas))
            {
                // Check door object first
                currentDoorCanvas = doorObject.GetComponent<Canvas>();
                
                // If not found, check children but only if needed
                if (currentDoorCanvas == null)
                {
                    // Use GetComponentInChildren only when necessary as it's expensive
                    Transform canvasTransform = doorObject.transform.Find("Canvas");
                    if (canvasTransform != null)
                    {
                        currentDoorCanvas = canvasTransform.GetComponent<Canvas>();
                    }
                    
                    // If still not found, do the more expensive full search
                    if (currentDoorCanvas == null)
                    {
                        currentDoorCanvas = doorObject.GetComponentInChildren<Canvas>(true);
                    }
                }
                
                // Cache the result whether found or not
                doorCanvasCache[doorID] = currentDoorCanvas;
            }
            
            // Show the canvas if found
            if (currentDoorCanvas != null)
            {
                currentDoorCanvas.gameObject.SetActive(true);
            }
        }
    }

    private void HideCurrentDoorCanvas()
    {
        if (currentDoorCanvas != null && currentDoorCanvas.gameObject.activeSelf)
        {
            currentDoorCanvas.gameObject.SetActive(false);
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!canInteract || currentDoorAnimator == null || currentDoorObject == null)
        {
            return;
        }
        
        // Trigger the door animation
        currentDoorAnimator.SetTrigger("OpenDoor");
        
        // Add to opened doors set
        int doorID = currentDoorObject.GetInstanceID();
        openedDoors.Add(doorID);
        
        // Hide the canvas immediately
        HideCurrentDoorCanvas();
        
        // Reset current door references
        currentDoorAnimator = null;
        currentDoorObject = null;
        canInteract = false;
    }

    // Animation Event Handler - can be called from the door's animation
    public void OnDoorFullyOpened(AnimationEvent evt)
    {
        // If this method is called from animation events, you can
        // add additional cleanup or state changes here
    }
    
    // Optional: Reset a specific door to be interactable again (for cases where doors can be closed)
    public void ResetDoorInteractability(GameObject doorObject)
    {
        if (doorObject != null)
        {
            openedDoors.Remove(doorObject.GetInstanceID());
        }
    }

}