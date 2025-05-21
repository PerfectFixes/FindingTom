using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private bool startPaused = false;
    
    [Header("Input Settings")]
    [Tooltip("Reference to the Pause action from your Input Actions asset")]
    [SerializeField] private InputActionReference pauseAction;
    
    [Header("Player Control")]
    [Tooltip("The ThirdPersonController to disable during pause")]
    [SerializeField] private MonoBehaviour thirdPersonController;
    
    [Tooltip("The PlayerInput component to disable during pause")]
    [SerializeField] private PlayerInput playerInput;
    
    [Tooltip("The camera input handler to disable during pause (StarterAssetsInputs)")]
    [SerializeField] private MonoBehaviour cameraInputHandler;
    
    [Header("Events (Optional)")]
    [SerializeField] private UnityEvent onPause;
    [SerializeField] private UnityEvent onResume;
    
    // Store disabled action maps to re-enable them later
    private List<InputActionMap> disabledActionMaps = new List<InputActionMap>();
    public bool isPaused = false;
    private float previousTimeScale = 1f;
    
    private void Awake()
    {
        // Make sure the pause menu is initially inactive
        if (pauseMenuCanvas != null && !startPaused)
        {
            pauseMenuCanvas.SetActive(false);
        }
        
        // If player input is not assigned, try to find it
        if (playerInput == null)
        {
            playerInput = FindFirstObjectByType<PlayerInput>();
        }
    }
    
    private void OnEnable()
    {
        // Enable the pause action and subscribe to it
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPauseActionPerformed;
        }
        else
        {
            Debug.LogError("Pause action not assigned in " + gameObject.name);
        }
        
        // If we're supposed to start paused, pause the game
        if (startPaused)
        {
            PauseGame();
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from the pause action when disabled
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.performed -= OnPauseActionPerformed;
        }
    }
    
    // Called when the pause action is triggered
    private void OnPauseActionPerformed(InputAction.CallbackContext context)
    {
        TogglePause();
    }
    
    // Toggle between paused and unpaused states
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PauseGame();
        }
    }
    
    // Disable gameplay inputs during pause
    private void DisableGameplayInputs()
    {
        disabledActionMaps.Clear();
        
        // Disable ThirdPersonController
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }
        
        // Disable camera input handler
        if (cameraInputHandler != null)
        {
            cameraInputHandler.enabled = false;
        }
        
        // Disable player input if available
        if (playerInput != null && pauseAction != null && pauseAction.action != null)
        {
            // Get the action map that contains the pause action
            InputActionMap pauseActionMap = pauseAction.action.actionMap;
            
            // Disable all other action maps
            foreach (var actionMap in playerInput.actions.actionMaps)
            {
                // Skip the map containing the pause action to ensure we can unpause
                if (actionMap == pauseActionMap)
                    continue;
                
                if (actionMap.enabled)
                {
                    disabledActionMaps.Add(actionMap);
                    actionMap.Disable();
                }
            }
        }
    }
    
    // Re-enable gameplay inputs when resuming
    private void ReenableGameplayInputs()
    {
        // Re-enable ThirdPersonController
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = true;
        }
        
        // Re-enable camera input handler
        if (cameraInputHandler != null)
        {
            cameraInputHandler.enabled = true;
        }
        
        // Re-enable all previously disabled action maps
        foreach (var map in disabledActionMaps)
        {
            map.Enable();
        }
        disabledActionMaps.Clear();
    }
    
    // Pause the game
    public void PauseGame()
    {
        // Remember the current time scale (in case it's not 1)
        previousTimeScale = Time.timeScale;
        
        // Set time scale to 0 (pause physics/animations)
        Time.timeScale = 0f;
        
        // Disable gameplay inputs including camera
        DisableGameplayInputs();
        
        // Show the pause menu
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
        }
        
        // Set the pause flag
        isPaused = true;
        
        // Invoke the pause event
        onPause?.Invoke();

    }
    
    // Resume the game
    public void ResumeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Restore the previous time scale
        Time.timeScale = previousTimeScale;
        
        // Hide the pause menu
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }
        
        // Re-enable gameplay inputs
        ReenableGameplayInputs();
        
        // Clear the pause flag
        isPaused = false;
        
        // Invoke the resume event
        onResume?.Invoke();
    }

    [Obsolete("Obsolete")]
    public void LoadMainMenu()
    {
        // Log the exit in the editor

        // In editor, this doesn't actually quit
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    // Public method to check if the game is paused
    public bool IsPaused()
    {
        return isPaused;
    }
}