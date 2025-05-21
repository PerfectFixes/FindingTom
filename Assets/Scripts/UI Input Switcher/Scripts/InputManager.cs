using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Singleton pattern
    private static InputManager instance;
    public static InputManager Instance { get { return instance; } }
    
    // Event that will be triggered when the input type changes
    public static event Action OnInputTypeChanged;
    
    [Header("Input Settings")]
    [Tooltip("Currently selected input type")]
    [SerializeField] private InputActionData.InputDeviceType currentInputTypeEnum = InputActionData.InputDeviceType.Keyboard;
    
    // Property to get/set the input type as a string
    public string currentInputType 
    { 
        get { return currentInputTypeEnum.ToString(); }
        set 
        {
            if (System.Enum.TryParse<InputActionData.InputDeviceType>(value, true, out InputActionData.InputDeviceType result))
            {
                currentInputTypeEnum = result;
            }
        }
    }
    
    [Tooltip("Default input type to use if none is saved")]
    [SerializeField] private InputActionData.InputDeviceType defaultInputType = InputActionData.InputDeviceType.Keyboard;
    
    [Header("Control Schemes")]
    [Tooltip("All available input types")]
    [SerializeField] private List<InputActionData.InputDeviceType> availableInputTypes = new List<InputActionData.InputDeviceType>() 
    { 
        InputActionData.InputDeviceType.Keyboard, 
        InputActionData.InputDeviceType.Xbox, 
        InputActionData.InputDeviceType.PlayStation, 
        InputActionData.InputDeviceType.ProController 
    };
    
    // The PlayerInput component to manage
    private PlayerInput playerInput;
    
    private void Awake()
    {
        // Singleton setup
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        // Optional: Make this object persistent across scenes
        // DontDestroyOnLoad(gameObject);
        
        // Get PlayerInput component if one exists
        playerInput = GetComponent<PlayerInput>();
        
        // Load the saved input type or use the default
        LoadInputType();
    }
    
    private void Start()
    {
        // Apply the loaded input type
        ApplyInputType(currentInputType);
    }
    
    // Load the saved input type from PlayerPrefs
    private void LoadInputType()
    {
        // Check if we have a saved preference
        string savedType = InputTypeSelector.GetCurrentInputType();
        
        if (!string.IsNullOrEmpty(savedType) && System.Enum.TryParse<InputActionData.InputDeviceType>(savedType, true, out InputActionData.InputDeviceType result))
        {
            currentInputTypeEnum = result;
        }
        else
        {
            currentInputTypeEnum = defaultInputType;
        }

    }
    
    // Apply the input type to the game's systems
    private void ApplyInputType(string inputTypeStr)
    {
        if (string.IsNullOrEmpty(inputTypeStr))
        {
            Debug.LogError("Attempted to apply null or empty input type");
            return;
        }
        
        // Convert string to enum
        if (System.Enum.TryParse<InputActionData.InputDeviceType>(inputTypeStr, true, out InputActionData.InputDeviceType inputType))
        {
            // Set the current input type
            currentInputTypeEnum = inputType;
            
            // If we have a PlayerInput component, switch control scheme
            if (playerInput != null)
            {
                // Map your input type to the appropriate control scheme name
                string controlScheme = MapInputTypeToControlScheme(inputTypeStr);
                
                // Try to switch the active control scheme
                try
                {
                    playerInput.SwitchCurrentControlScheme(controlScheme);
                    Debug.Log($"Switched control scheme to: {controlScheme}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to switch control scheme to '{controlScheme}': {e.Message}");
                }
            }
            
            // Notify all listeners that the input type has changed
            OnInputTypeChanged?.Invoke();
        }
        else
        {
            Debug.LogError($"Failed to parse input type: {inputTypeStr}");
        }
    }
    
    // Map input type to control scheme name (you may need to customize this)
    private string MapInputTypeToControlScheme(string inputType)
    {
        // This is a simple mapping - update as needed based on your input action asset setup
        switch (inputType.ToLower())
        {
            case "keyboard": return "Keyboard&Mouse";
            case "xbox": return "Gamepad";
            case "playstation": return "Gamepad";
            case "procontroller": return "Gamepad";
            default: return "Keyboard&Mouse";
        }
    }
    
    // Public method to change input type 
    public void ChangeInputType(string newInputType)
    {
        if (System.Enum.TryParse<InputActionData.InputDeviceType>(newInputType, true, out InputActionData.InputDeviceType result) && 
            availableInputTypes.Contains(result))
        {
            ApplyInputType(newInputType);
        }
        else
        {
            Debug.LogWarning($"Input type '{newInputType}' is not valid or not in the list of available types");
        }
    }
    
    // Public method to get the current input type
    public string GetCurrentInputType()
    {
        return currentInputType;
    }
    
    // Called when the input type selector changes the input type
    private void HandleInputTypeChanged(string newInputType)
    {
        ApplyInputType(newInputType);
    }
    
    // Called when the game object is enabled
    private void OnEnable()
    {
        // Subscribe to the input type selection event
        if (InputTypeSelector.Instance != null)
        {
            // Connect to the event we added to InputTypeSelector
            InputTypeSelector.Instance.OnInputTypeSelected += HandleInputTypeChanged;
        }
    }
    
    // Called when the game object is disabled
    private void OnDisable()
    {
        // Unsubscribe from the input type selection event
        if (InputTypeSelector.Instance != null)
        {
            InputTypeSelector.Instance.OnInputTypeSelected -= HandleInputTypeChanged;
        }
    }
}