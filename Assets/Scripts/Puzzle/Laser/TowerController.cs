using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerController : MonoBehaviour
{
    [Header("Tower Settings")]
    [Tooltip("List of all towers/deflectors that can be controlled")]
    public List<Transform> towers = new List<Transform>();
    
    [Tooltip("Movement speed of the towers")]
    public float moveSpeed = 1.0f;
    
    [Tooltip("Minimum Z position of towers")]
    public float minZPosition = -0.5f;
    
    [Tooltip("Maximum Z position of towers")]
    public float maxZPosition = 0.5f;
    
    [Header("Outline Settings")]
    [Tooltip("Outline color for the selected tower")]
    public Color outlineColor = Color.yellow;
    
    [Tooltip("Outline width for the selected tower")]
    [Range(0f, 10f)]
    public float outlineWidth = 5f;
    
    [Header("Input Settings")]
    [Tooltip("Reference to an Input Action asset with Move and Select actions")]
    public InputActionAsset inputActions;
    
    [Header("Puzzle References")]
    [Tooltip("Reference to the LaserReceiver that determines if the puzzle is solved")]
    public LaserReceiver laserReceiver;
    
    // Input action references
    private InputAction moveAction;
    private InputAction selectAction;
    
    // Currently selected tower index
    private int currentTowerIndex = -1;
    
    // Movement value
    private float verticalInput = 0f;
    
    // Store monobehaviours for each tower that handle outlines
    private Dictionary<Transform, MonoBehaviour> towerOutlines = new Dictionary<Transform, MonoBehaviour>();
    
    // Input system values
    private float selectInputValue = 0f;
    private float selectInputPrevValue = 0f;
    
    private void Awake()
    {
        // Set all towers to starting position and set up outlines
        foreach (Transform tower in towers)
        {
            if (tower != null)
            {
                // Set starting Z position
                Vector3 startPosition = tower.position;
                startPosition.z = minZPosition;
                tower.position = startPosition;
                
                // Try to find an Outline component (from Quick Outline package)
                MonoBehaviour outlineComponent = FindOutlineComponent(tower.gameObject);
                
                if (outlineComponent != null)
                {
                    // Store the outline component
                    towerOutlines[tower] = outlineComponent;
                    
                    // Configure and disable initially
                    SetOutlineProperties(outlineComponent, outlineColor, outlineWidth);
                    EnableOutline(outlineComponent, false);
                }
                else
                {
                    Debug.LogWarning("No Outline component found on " + tower.name + ". Make sure you've added the Quick Outline component to this object.");
                }
            }
        }
        
        // Set up input actions from the asset
        SetupInputActions();
        
        // Check LaserReceiver reference
        if (laserReceiver == null)
        {
            Debug.LogWarning("LaserReceiver reference not set. Tower movement will not be disabled when puzzle is solved.");
        }
        
        // Select the first tower by default if we have any
        if (towers.Count > 0)
        {
            currentTowerIndex = 0;
            UpdateTowerOutline();
        }
    }
    
    // Set up input actions using the asset
    private void SetupInputActions()
    {
        if (inputActions != null)
        {
            // Try to find the Player action map (common naming convention)
            InputActionMap actionMap = inputActions.FindActionMap("Player");
            
            // If not found, try to use the first available action map
            if (actionMap == null && inputActions.actionMaps.Count > 0)
            {
                actionMap = inputActions.actionMaps[0];
            }
            
            if (actionMap != null)
            {
                // Find the Move and Select actions
                moveAction = actionMap.FindAction("Move");
                selectAction = actionMap.FindAction("Select");
                
                // Register callbacks
                if (moveAction != null)
                {
                    moveAction.performed += OnMove;
                    moveAction.canceled += OnMove;
                }
                
                if (selectAction != null)
                {
                    selectAction.performed += OnSelect;
                    selectAction.canceled += OnSelect;
                }
                
                // Enable the action map
                actionMap.Enable();
            }
            else
            {
                Debug.LogError("No action maps found in the input actions asset!");
            }
        }
        else
        {
            Debug.LogError("Input Actions asset not assigned! Add a reference to an Input Action Asset in the Inspector.");
        }
    }
    
    // Helper method to find any Outline component on the given game object
    private MonoBehaviour FindOutlineComponent(GameObject obj)
    {
        // Try to find a component named "Outline" using GetComponents
        MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            if (component.GetType().Name == "Outline")
            {
                return component;
            }
        }
        
        // If no Outline component found, try to add one
        // Note: This will only work if Quick Outline is properly imported
        try
        {
            // Use reflection to create the component without direct reference
            System.Type outlineType = System.Type.GetType("Outline, Assembly-CSharp");
            if (outlineType != null)
            {
                return obj.AddComponent(outlineType) as MonoBehaviour;
            }
            else
            {
                // Try alternative namespace
                outlineType = System.Type.GetType("QuickOutline.Outline, Assembly-CSharp");
                if (outlineType != null)
                {
                    return obj.AddComponent(outlineType) as MonoBehaviour;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error adding Outline component: " + e.Message);
        }
        
        return null;
    }
    
    // Helper method to set outline properties using reflection (works with any Quick Outline implementation)
    private void SetOutlineProperties(MonoBehaviour outline, Color color, float width)
    {
        try
        {
            // Use reflection to set properties without direct reference
            System.Reflection.PropertyInfo colorProperty = outline.GetType().GetProperty("OutlineColor");
            System.Reflection.PropertyInfo widthProperty = outline.GetType().GetProperty("OutlineWidth");
            
            if (colorProperty != null)
            {
                colorProperty.SetValue(outline, color);
            }
            
            if (widthProperty != null)
            {
                widthProperty.SetValue(outline, width);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error setting outline properties: " + e.Message);
        }
    }
    
    // Helper method to enable/disable the outline
    private void EnableOutline(MonoBehaviour outline, bool enabled)
    {
        if (outline != null)
        {
            outline.enabled = enabled;
        }
    }

   

    private void OnMove(InputAction.CallbackContext context)
    {
        // If puzzle is solved, ignore movement input
        if (laserReceiver.isPuzzleSolved)
            return;
        // Read the input value - this works for any binding type
        if (context.valueType == typeof(Vector2))
        {
            // For 2D Vector input like gamepad sticks
            Vector2 input = context.ReadValue<Vector2>();
            verticalInput = input.y;
        }
        else if (context.valueType == typeof(float))
        {
            // For 1D axis input
            verticalInput = context.ReadValue<float>();
        }
        else if (context.canceled)
        {
            // When the action is canceled (input released)
            verticalInput = 0f;
        }
    }
    
    private void OnSelect(InputAction.CallbackContext context)
    {
        // If puzzle is solved, ignore selection input
        if (laserReceiver.isPuzzleSolved)
            return;
        
        // Read the selection input value
        if (context.canceled)
        {
            selectInputValue = 0f;
        }
        else if (context.performed)
        {
            // Get the input value based on its type
            if (context.valueType == typeof(Vector2))
            {
                Vector2 input = context.ReadValue<Vector2>();
                selectInputValue = input.x;
            }
            else if (context.valueType == typeof(float))
            {
                selectInputValue = context.ReadValue<float>();
            }
        }
    }
    
    // Check if the puzzle is solved, and update the internal state
    
    private void Update()
    {
        // Only proceed if puzzle is not solved
        if (laserReceiver.isPuzzleSolved)
        {
            // Disable all outlines
            foreach (Transform tower in towers)
            {
                DisableTowerOutline(tower);
            }
            return;
        }
           
        
        // Handle tower selection (done in Update to handle edge detection properly)
        HandleTowerSelection();
        
        // Handle tower movement
        if (towers.Count > 0 && currentTowerIndex >= 0 && currentTowerIndex < towers.Count)
        {
            Transform currentTower = towers[currentTowerIndex];
            
            // Move the selected tower forward/backward based on input
            if (currentTower != null && verticalInput != 0)
            {
                // Get the current position
                Vector3 position = currentTower.position;
                
                // Calculate the new Z position
                float newZ = position.z + (verticalInput * moveSpeed * Time.deltaTime);
                
                // Clamp the Z position within the limits
                newZ = Mathf.Clamp(newZ, minZPosition, maxZPosition);
                
                // Apply the new position
                position.z = newZ;
                currentTower.position = position;
            }
        }
    }
    
    // Handle tower selection based on input
    private void HandleTowerSelection()
    {
        // Only process if there's a difference in input (rising/falling edge detection)
        if (Mathf.Abs(selectInputValue - selectInputPrevValue) > 0.5f)
        {
            int previousIndex = currentTowerIndex;
            
            // Detect direction change
            bool moveLeft = selectInputValue < -0.5f && selectInputPrevValue >= -0.5f;
            bool moveRight = selectInputValue > 0.5f && selectInputPrevValue <= 0.5f;
            
            // Change selected tower based on direction
            if (moveLeft && currentTowerIndex > 0)
            {
                currentTowerIndex--;
            }
            else if (moveRight && currentTowerIndex < towers.Count - 1)
            {
                currentTowerIndex++;
            }
            
            // Update tower outline if selection changed
            if (previousIndex != currentTowerIndex)
            {
                // Disable outline on the previously selected tower
                if (previousIndex >= 0 && previousIndex < towers.Count)
                {
                    Transform previousTower = towers[previousIndex];
                    DisableTowerOutline(previousTower);
                }
                
                // Update the tower outline
                UpdateTowerOutline();
            }
        }
        
        // Store current value for next frame comparison
        selectInputPrevValue = selectInputValue;
    }
    
    private void UpdateTowerOutline()
    {
        if (towers.Count > 0 && currentTowerIndex >= 0 && currentTowerIndex < towers.Count)
        {
            Transform currentTower = towers[currentTowerIndex];
            
            if (currentTower != null && towerOutlines.ContainsKey(currentTower))
            {
                // Get the outline component
                MonoBehaviour outline = towerOutlines[currentTower];
                
                // Update outline properties in case they were changed in the Inspector
                SetOutlineProperties(outline, outlineColor, outlineWidth);
                
                // Enable the outline
                EnableOutline(outline, true);
            }
        }
    }
    
    private void DisableTowerOutline(Transform tower)
    {
        if (tower != null && towerOutlines.ContainsKey(tower))
        {
            // Disable the outline component
            MonoBehaviour outline = towerOutlines[tower];
            EnableOutline(outline, false);
        }
    }
    
    private void OnEnable()
    {
        // Enable input actions if they exist
        InputActionMap actionMap = inputActions?.FindActionMap("Player");
        if (actionMap != null) actionMap.Enable();
    }
    
    private void OnDisable()
    {
        // Disable input actions if they exist
        InputActionMap actionMap = inputActions?.FindActionMap("Player");
        if (actionMap != null) actionMap.Disable();
        
        // Disable all outlines
        foreach (Transform tower in towers)
        {
            DisableTowerOutline(tower);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up callbacks
        if (moveAction != null)
        {
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
        }
        
        if (selectAction != null)
        {
            selectAction.performed -= OnSelect;
            selectAction.canceled -= OnSelect;
        }
        
        // Disable all outlines
        foreach (Transform tower in towers)
        {
            DisableTowerOutline(tower);
        }
    }
}