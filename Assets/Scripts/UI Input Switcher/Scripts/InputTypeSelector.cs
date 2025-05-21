using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InputTypeSelector : MonoBehaviour
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
    
    private GameObject currentSelectionIndicator;
    private static string currentInputType;
    private AsyncOperation sceneLoadOperation;
    
    // Singleton instance to access from other scripts
    private static InputTypeSelector instance;
    public static InputTypeSelector Instance { get { return instance; } }
    
    // Event that will be triggered when input type is selected
    public event Action<string> OnInputTypeSelected;

    private void Awake()
    {
        // Simple singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        // Load previously saved input type if available
        LoadSavedInputType();
        
        // Initialize all buttons
        InitializeButtons();
    }
    
    private void InitializeButtons()
    {
        foreach (var option in inputOptions)
        {
            if (option.button != null)
            {
                string inputType = option.inputTypeName; // Capture for lambda
                option.button.onClick.AddListener(() => SelectInputType(inputType, option.button));
                
                // If this option is already selected, show it as such
                if (inputType == currentInputType)
                {
                    HighlightSelectedButton(option.button);
                }
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
        
        // Update visual selection if needed
        HighlightSelectedButton(selectedButton);
        
        // Trigger event for any listeners
        OnInputTypeSelected?.Invoke(inputType);
    }
    
   
    
    private void HighlightSelectedButton(Button selectedButton)
    {
        // Remove previous indicator if any
        if (currentSelectionIndicator != null)
        {
            Destroy(currentSelectionIndicator);
        }
        
        // Add visual indicator to selected button (if we have an indicator prefab)
        if (selectionIndicatorPrefab != null)
        {
            currentSelectionIndicator = Instantiate(selectionIndicatorPrefab, selectedButton.transform);
        }
        
        // You could also change button colors or other visual indicators here
        // For example:
        // foreach (var option in inputOptions)
        // {
        //     ColorBlock colors = option.button.colors;
        //     colors.normalColor = (option.button == selectedButton) ? Color.green : Color.white;
        //     option.button.colors = colors;
        // }
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
        return currentInputType;
    }
    
    // Method to add new input types at runtime (if needed)
    public void AddInputType(InputActionData.InputDeviceType inputType, Button button)
    {
        InputOption newOption = new InputOption
        {
            inputType = inputType,
            button = button
        };
        
        button.onClick.AddListener(() => SelectInputType(newOption.inputTypeName, button));
        inputOptions.Add(newOption);
    }
}