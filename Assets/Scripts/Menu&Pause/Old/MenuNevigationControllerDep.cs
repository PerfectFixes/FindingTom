/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class  MenuNevigationControllerDep : MonoBehaviour
{
    [Header("Input References")]
    [SerializeField] private InputActionReference navigationAction;
    [SerializeField] private InputActionReference submitAction;
    [SerializeField] private InputActionReference cancelAction;

    [Header("UI References")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private GameObject optionsMenuCanvas;
    
    [Header("Settings")]
    [SerializeField] private float navigationDelay = 0.2f;
    
    // Track the currently active canvas
    private GameObject currentCanvas;
    private GameObject previousCanvas;
    private float lastNavigationTime;
    
    private void Awake()
    {
        // Enable the input actions
        navigationAction.action.Enable();
        submitAction.action.Enable();
        cancelAction.action.Enable();
    }
    
    private void OnEnable()
    {
        // Subscribe to input events
        submitAction.action.performed += OnSubmit;
        cancelAction.action.performed += OnCancel;
        
        // Set main menu as the initial canvas
        SetActiveCanvas(mainMenuCanvas);
    }
    
    private void OnDisable()
    {
        // Unsubscribe from input events
        submitAction.action.performed -= OnSubmit;
        cancelAction.action.performed -= OnCancel;
    }
    
    private void Update()
    {
        HandleNavigation();
    }
    
    private void HandleNavigation()
    {
        // Check if enough time has passed since last navigation
        if (Time.unscaledTime - lastNavigationTime < navigationDelay)
            return;
            
        // Read navigation vector
        Vector2 navigationVector = navigationAction.action.ReadValue<Vector2>();
        
        if (navigationVector.magnitude > 0.5f)
        {
            // Determine navigation direction
            MoveSelection(navigationVector);
            lastNavigationTime = Time.unscaledTime;
        }
    }
    
    private void MoveSelection(Vector2 direction)
    {
        // Get the current selected GameObject
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        
        // If nothing is selected, select the first selectable element
        if (currentSelected == null)
        {
            SelectFirstButton();
            return;
        }
        
        // Find the selectable component
        Selectable selectable = currentSelected.GetComponent<Selectable>();
        if (selectable == null)
            return;
            
        // Determine the direction to move
        Selectable nextSelectable = null;
        
        if (direction.y > 0.5f)
            nextSelectable = selectable.FindSelectableOnUp();
        else if (direction.y < -0.5f)
            nextSelectable = selectable.FindSelectableOnDown();
        else if (direction.x < -0.5f)
            nextSelectable = selectable.FindSelectableOnLeft();
        else if (direction.x > 0.5f)
            nextSelectable = selectable.FindSelectableOnRight();
            
        // If we found a selectable in that direction, select it
        if (nextSelectable != null)
        {
            nextSelectable.Select();
            
            // Play navigation sound if needed
            // AudioManager.Instance.PlayNavigationSound();
        }
    }
    
    private void OnSubmit(InputAction.CallbackContext context)
    {
        // Get the current selected GameObject
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        
        if (currentSelected != null)
        {
            // Trigger a click on the button
            Button button = currentSelected.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
                
                // Play selection sound if needed
                // AudioManager.Instance.PlaySelectionSound();
            }
        }
    }
    
    private void OnCancel(InputAction.CallbackContext context)
    {
        // Return to the previous canvas
        if (previousCanvas != null)
        {
            SetActiveCanvas(previousCanvas);
            
            // Play back/cancel sound if needed
            // AudioManager.Instance.PlayCancelSound();
        }
    }
    
    public void SetActiveCanvas(GameObject canvas)
    {
        // Store the previously active canvas
        if (currentCanvas != null)
        {
            previousCanvas = currentCanvas;
            currentCanvas.SetActive(false);
        }
        
        // Activate the new canvas
        currentCanvas = canvas;
        currentCanvas.SetActive(true);
        
        // Select the first button in the new canvas
        SelectFirstButton();
    }
    
    private void SelectFirstButton()
    {
        // Find and select the first selectable element in the current canvas
        Selectable[] selectables = currentCanvas.GetComponentsInChildren<Selectable>();
        
        if (selectables.Length > 0)
        {
            // Find the first active and interactable selectable
            foreach (Selectable selectable in selectables)
            {
                if (selectable.gameObject.activeInHierarchy && selectable.interactable)
                {
                    selectable.Select();
                    break;
                }
            }
        }
    }
    
    // Public methods for button callbacks
    
    public void OpenPauseMenu()
    {
        SetActiveCanvas(pauseMenuCanvas);
    }
    
    public void OpenOptionsMenu()
    {
        SetActiveCanvas(optionsMenuCanvas);
    }
    
    public void ReturnToMainMenu()
    {
        SetActiveCanvas(mainMenuCanvas);
    }
    
    public void QuitGame()
    {
        // Add confirmation if needed
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}*/