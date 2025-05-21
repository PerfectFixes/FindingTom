using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New Input Action Data", menuName = "Input System/Input Action Data")]
public class InputActionData : ScriptableObject
{
    // Define an enum for the specific input types
    public enum InputDeviceType
    {
        Keyboard,
        ProController,
        PlayStation,
        Xbox
    }

    [System.Serializable]
    public class InputTypeSprite
    {
        public InputDeviceType inputType;
        public Sprite iconSprite; // The button/key image for this input type
    }
    
    [Header("Input Action")]
    [Tooltip("Reference to the Input Action from Unity's Input System")]
    [SerializeField] private InputActionReference _actionReference;
    
    // Property that automatically updates the action name when changed
    public InputActionReference actionReference
    {
        get { return _actionReference; }
        set 
        { 
            _actionReference = value;
            // Update action name when reference changes
            if (_actionReference != null && _actionReference.action != null)
            {
                _actionName = _actionReference.action.name;
            }
        }
    }
    
    [Header("Display Info")]
    [Tooltip("Name automatically generated from the action reference")]
    [SerializeField] private string _actionName;
    
    // Public property to access the action name
    public string actionName
    {
        get 
        { 
            // If we have a reference but name is empty, try to get it
            if (string.IsNullOrEmpty(_actionName) && _actionReference != null && _actionReference.action != null)
            {
                _actionName = _actionReference.action.name;
            }
            return _actionName; 
        }
    }
    
    [Tooltip("Description of what this action does")]
    [TextArea(2, 4)]
    public string actionDescription;
    
    [Header("Input Type Specific Icons")]
    [Tooltip("Icons for each input type")]
    public List<InputTypeSprite> inputTypeIcons = new List<InputTypeSprite>();
    
    // Called when the scriptable object is created or modified in the editor
    private void OnValidate()
    {
        // Update name if reference exists
        if (_actionReference != null && _actionReference.action != null)
        {
            _actionName = _actionReference.action.name;
        }
    }
    
    // Convenience method to get the appropriate sprite for the selected input type
    public Sprite GetSpriteForInputType(string inputTypeString)
    {
        // Convert string to enum
        if (System.Enum.TryParse<InputDeviceType>(inputTypeString, true, out InputDeviceType inputType))
        {
            return GetSpriteForInputType(inputType);
        }
        
        // If parsing fails, return the first sprite as fallback
        if (inputTypeIcons.Count > 0)
        {
            Debug.LogWarning($"No valid input type found for '{inputTypeString}' in action '{actionName}'. Using fallback icon.");
            return inputTypeIcons[0].iconSprite;
        }
        
        return null;
    }
    
    // Overload that accepts the enum directly
    public Sprite GetSpriteForInputType(InputDeviceType inputType)
    {
        foreach (var item in inputTypeIcons)
        {
            if (item.inputType == inputType)
            {
                return item.iconSprite;
            }
        }
        
        // If no matching type is found, return the first one as fallback (if any exist)
        if (inputTypeIcons.Count > 0)
        {
            Debug.LogWarning($"No icon found for input type '{inputType}' in action '{actionName}'. Using fallback icon.");
            return inputTypeIcons[0].iconSprite;
        }
        
        // If no sprites at all, return null
        Debug.LogError($"No icons defined for action '{actionName}'!");
        return null;
    }
    
    // Get the binding display string for the current input type
    public string GetBindingDisplayString(string inputTypeString)
    {
        if (_actionReference == null || _actionReference.action == null)
        {
            return "Action not set";
        }
        
        // In a real implementation, you would have different binding groups for each device type
        // This simplified version just returns the general binding string
        return _actionReference.action.GetBindingDisplayString();
    }
}