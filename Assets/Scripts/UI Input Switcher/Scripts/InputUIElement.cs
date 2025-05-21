using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class InputUIElement : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("The scriptable object containing the input action data")]
    [SerializeField] private InputActionData inputData;
    
    [Header("UI Components")]
    [Tooltip("Image component to display the input icon (will use this GameObject's Image if not set)")]
    [SerializeField] private Image iconImage;
    
    [Tooltip("Optional text component to display the action name")]
    [SerializeField] private TMP_Text actionNameText;
    
    [Tooltip("Optional text component to display the binding (e.g., 'E' or 'X button')")]
    [SerializeField] private TMP_Text bindingText;
    
    [Header("Settings")]
    [Tooltip("Should this update automatically when the input type changes?")]
    [SerializeField] private bool autoUpdate = true;
    
    
    [Tooltip("Should the icon preserve aspect ratio when fitting?")]
    [SerializeField] private bool preserveAspect = true;
    
    // Store the previous input type to check for changes
    private string previousInputType;
    
    private void Awake()
    {
        // If no icon image is assigned, try to get it from this GameObject
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }
        
        // Initialize the previous input type
        previousInputType = PlayerPrefs.GetString("SelectedInputType", "keyboard");
    }
    
    private void OnEnable()
    {
        // Subscribe to input type change events if we want auto-updates
        if (autoUpdate)
        {
            InputManager.OnInputTypeChanged += UpdateVisuals;
        }
        
        // Initial update
        UpdateVisuals();
    }
    
    private void OnDisable()
    {
        if (autoUpdate)
        {
            InputManager.OnInputTypeChanged -= UpdateVisuals;
        }
    }
    
    private void Update()
    {
        // Check if PlayerPrefs value has changed
        string currentInputType = PlayerPrefs.GetString("SelectedInputType", "keyboard");
        if (currentInputType != previousInputType)
        {
            previousInputType = currentInputType;
            UpdateVisuals();
        }
    }
    
    // Update the visual elements based on the current input type
    public void UpdateVisuals()
    {
        if (inputData == null)
        {
            Debug.Log("No InputActionData assigned to " + gameObject.name);
            iconImage.enabled = false;
            return;
        }
        
        // Get the current input type
        string currentInputType = ChangePlayerControls.GetCurrentInputType();
        
        // Update the icon
        if (iconImage != null)
        {
            Sprite icon = inputData.GetSpriteForInputType(currentInputType);
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
                
                // Preserve aspect ratio if specified
                iconImage.preserveAspect = preserveAspect;
            }
            else
            {
                iconImage.enabled = false; // Hide if no icon found
            }
        }
        
        // Update the action name text
        if (actionNameText != null)
        {
            actionNameText.text = inputData.actionName;
        }
        
        // Update the binding text
        if (bindingText != null)
        {
            bindingText.text = inputData.GetBindingDisplayString(currentInputType);
        }
    }
    
    // Method to manually force an update (e.g., after changing settings)
    public void ForceUpdate()
    {
        UpdateVisuals();
    }
}