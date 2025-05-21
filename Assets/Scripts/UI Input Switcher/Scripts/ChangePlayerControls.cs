using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangePlayerControls : MonoBehaviour
{
    [System.Serializable]
    public class InputOption
    {
        public InputActionData.InputDeviceType inputType;
        public Button button;
        
        // Property to get the string representation of the enum
        public string inputTypeName { get { return inputType.ToString(); } }
    }
    
    [Header("Input Options")]
    [SerializeField] private List<InputOption> inputOptions = new List<InputOption>();
    
    [Header("Selected Indicator (Optional)")]
    [SerializeField] private GameObject selectionIndicatorPrefab;
    
    [Header("References")]
    [SerializeField] private PauseController pauseController;
    
    private GameObject currentSelectionIndicator;
    private static string currentInputType;
    
    // Event that will be triggered when input type is selected
    public event Action<string> OnInputTypeSelected;

    private void Awake()
    {
        // Load the saved input type to know which one is currently active
        LoadSavedInputType();
    }
    
    private void OnEnable()
    {
        // Initialize buttons every time the panel is enabled
        // (ensures everything works properly when coming back to the pause menu)
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        foreach (var option in inputOptions)
        {
            if (option.button != null)
            {
                // Clear previous listeners to avoid duplicates
                option.button.onClick.RemoveAllListeners();
                
                string inputType = option.inputTypeName; // Capture for lambda
                option.button.onClick.AddListener(() => SelectInputType(inputType, option.button));
                
            }
            else
            {
                Debug.LogWarning($"Button for input type '{option.inputTypeName}' is not assigned!");
            }
        }
    }
    
    public void SelectInputType(string inputType, Button selectedButton)
    {
        // Save the selected input type
        currentInputType = inputType;
        SaveInputType(inputType);
        
        Debug.Log($"Selected input type: {inputType} (saved to PlayerPrefs)");
        
        // Trigger event for any listeners
        OnInputTypeSelected?.Invoke(inputType);
        
        // Resume the game after selecting input type
        if (pauseController != null)
        {
        }
        else
        {
            Debug.LogWarning("PauseController reference not set on ChangePlayerControls!");
        }
    }
    
    

    private void SaveInputType(string inputType)
    {
        PlayerPrefs.SetString("SelectedInputType", inputType);
        PlayerPrefs.Save();
    }

    private void LoadSavedInputType()
    {
        currentInputType = PlayerPrefs.GetString("SelectedInputType", "");
        
        // If no type has been saved yet, default to the first option in our list
        if (string.IsNullOrEmpty(currentInputType) && inputOptions.Count > 0)
        {
            currentInputType = inputOptions[0].inputTypeName;
        }
    }

    // Public method to get the current input type from other scripts
    public static string GetCurrentInputType()
    {
        return PlayerPrefs.GetString("SelectedInputType");
    }
}