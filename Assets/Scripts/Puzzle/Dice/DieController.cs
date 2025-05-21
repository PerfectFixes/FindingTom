using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DieController : MonoBehaviour
{
    [Header("Dice Settings")]
    [Tooltip("List of all dice that can be controlled")]
    public List<Transform> diceObjects = new List<Transform>();
    
    [Header("Dice Value Settings")]
    [Tooltip("Minimum value for dice")]
    public int minValue = 1;
    
    [Tooltip("Maximum value for dice")]
    public int maxValue = 6;
    
    [Header("Animation Settings")]
    [Tooltip("Time in seconds for dice rotation to complete")]
    [Range(0.1f, 5f)]
    public float rotationSpeed = 1f;
    
    [Header("Outline Settings")]
    [Tooltip("Outline color for the selected die")]
    public Color outlineColor = Color.yellow;
    
    [Tooltip("Outline width for the selected die")]
    [Range(0f, 10f)]
    public float outlineWidth = 5f;
    
    [Header("Input Settings")]
    [Tooltip("Reference to an Input Action asset with move and changeValue actions")]
    public InputActionAsset inputActions;
    
    [Header("Locked Dice")]
// Track which dice are locked (correct and can't be changed)
    public bool[] lockedDice;  // Changed from private to public for easier inspection
    
    [SerializeField]
    private PauseController pauseController;
    // Input action references
    private InputAction moveAction;
    private InputAction changeValueAction;
    private InputAction printAction;
    
    // Currently selected die index
    public int currentDieIndex = 0;
    
    // Dice values
    private int[] diceValues;
    
    // Target rotations for each die
    private Quaternion[] targetRotations;
    private Quaternion[] startRotations;
    private float[] rotationTimers;
    private bool[] isRotating;
    
    // Store monobehaviours for each die that handle outlines
    private Dictionary<Transform, MonoBehaviour> diceOutlines = new Dictionary<Transform, MonoBehaviour>();
    
    // Input system values
    private Vector2 moveInput = Vector2.zero;
    private Vector2 moveInputPrev = Vector2.zero;
    private Vector2 changeValueInput = Vector2.zero;
    
    [Header("SFX")] 
    [SerializeField] 
    private AudioClip[] rotationSFX;

    [SerializeField] 
    private AudioSource audioSource;
    private void Awake()
    {
        diceValues = new int[diceObjects.Count];
        targetRotations = new Quaternion[diceObjects.Count];
        startRotations = new Quaternion[diceObjects.Count];
        rotationTimers = new float[diceObjects.Count];
        isRotating = new bool[diceObjects.Count];
        lockedDice = new bool[diceObjects.Count];
    
        for (int i = 0; i < diceValues.Length; i++)
        {
            diceValues[i] = minValue;
            targetRotations[i] = Quaternion.Euler(0, 0, 0); // Default to showing "1"
            rotationTimers[i] = 0f;
            isRotating[i] = false;
            lockedDice[i] = false; // All dice start unlocked
        
            // Initialize the dice to show "1"
            if (diceObjects[i] != null)
            {
                diceObjects[i].rotation = Quaternion.Euler(0, 0, 0);
            }
        }
        
        // Set up outlines for each die
        foreach (Transform die in diceObjects)
        {
            if (die != null)
            {
                // Try to find an Outline component (from Quick Outline package)
                MonoBehaviour outlineComponent = FindOutlineComponent(die.gameObject);
                
                if (outlineComponent != null)
                {
                    // Store the outline component
                    diceOutlines[die] = outlineComponent;
                    
                    // Configure and disable initially
                    SetOutlineProperties(outlineComponent, outlineColor, outlineWidth);
                    EnableOutline(outlineComponent, false);
                }
                else
                {
                    Debug.LogWarning("No Outline component found on " + die.name + ". Make sure you've added the Quick Outline component to this object.");
                }
            }
     
        }
        
        SetupInputActions();
    
        // Select the first die by default if we have any
        if (diceObjects.Count > 0)
        {
            currentDieIndex = 0;
            UpdateDieOutline();
        }
    }
    
    //Method to lock a specific die
    public void LockDie(int dieIndex)
    {
        if (dieIndex >= 0 && dieIndex < lockedDice.Length)
        {
            // Lock the die
            lockedDice[dieIndex] = true;
        
            // If the currently selected die is being locked, find a new one to select
            if (currentDieIndex == dieIndex)
            {
                // First try to find the next unlocked die to the right
                int nextAvailable = -1;
                for (int i = currentDieIndex + 1; i < diceObjects.Count; i++)
                {
                    if (!lockedDice[i])
                    {
                        nextAvailable = i;
                        break;
                    }
                }
            
                // If we didn't find one to the right, try to the left
                if (nextAvailable == -1)
                {
                    for (int i = currentDieIndex - 1; i >= 0; i--)
                    {
                        if (!lockedDice[i])
                        {
                            nextAvailable = i;
                            break;
                        }
                    }
                }
            
                // If we found an unlocked die, select it
                if (nextAvailable != -1)
                {
                    // Remove outline from current die
                    DisableDieOutline(diceObjects[currentDieIndex]);
                
                    // Update selection
                    currentDieIndex = nextAvailable;
                
                    // Add outline to new die
                    UpdateDieOutline();
                }
            }
        }
    }
    
    //method to find the next available die
    private void FindNextUnlockedDie()
    {
        // First, try to find the next unlocked die
        for (int i = 0; i < lockedDice.Length; i++)
        {
            int checkIndex = (currentDieIndex + i) % lockedDice.Length;
            if (!lockedDice[checkIndex])
            {
                // Disable outline on current die
                if (currentDieIndex >= 0 && currentDieIndex < diceObjects.Count)
                {
                    DisableDieOutline(diceObjects[currentDieIndex]);
                }
            
                // Update to the new die
                currentDieIndex = checkIndex;
                UpdateDieOutline();
                return;
            }
        }
    }
    // Add this method to check if a die is locked
    public bool IsDieLocked(int dieIndex)
    {
        if (dieIndex >= 0 && dieIndex < lockedDice.Length)
        {
            return lockedDice[dieIndex];
        }
        return false;
    }
    
    
    // Set up input actions using the asset
    private void SetupInputActions()
    {
        if (inputActions != null)
        {
            // Try to find the Gameplay action map (common naming convention)
            InputActionMap actionMap = inputActions.FindActionMap("Gameplay");
            
            // If not found, try to use the first available action map
            if (actionMap == null && inputActions.actionMaps.Count > 0)
            {
                actionMap = inputActions.actionMaps[0];
            }
            
            if (actionMap != null)
            {
                // Find the necessary actions
                moveAction = actionMap.FindAction("Move");
                changeValueAction = actionMap.FindAction("ChangeValue");
                printAction = actionMap.FindAction("Print");
                
                // Register callbacks
                if (moveAction != null)
                {
                    moveAction.performed += OnMove;
                    moveAction.canceled += OnMove;
                }
                
                if (changeValueAction != null)
                {
                    changeValueAction.performed += OnChangeValue;
                    changeValueAction.canceled += OnChangeValue;
                }
                
                if (printAction != null)
                {
                    printAction.performed += OnPrint;
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
    
    // Helper method to set outline properties using reflection
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
        //If the game is paused don't select a different dice
        if (pauseController.isPaused)
        {
            return;
        }
        
        if (context.canceled)
        {
            moveInput = Vector2.zero;
        }
        else if (context.performed)
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }
    
    private void OnChangeValue(InputAction.CallbackContext context)
    {
        //If the game is paused don't change value
        if (pauseController.isPaused)
        {
            return;
        }
       
        if (context.canceled)
        {
            changeValueInput = Vector2.zero;
        }
        else if (context.performed)
        {
            changeValueInput = context.ReadValue<Vector2>();
            
            // Only process if there's a significant y-input
            if (Mathf.Abs(changeValueInput.y) > 0.5f)
            {
                if (changeValueInput.y > 0.5f)
                {
                    IncreaseDieValue();
                }
                else if (changeValueInput.y < -0.5f)
                {
                    DecreaseDieValue();
                }
            }
        } 
        
        
    }
    
    private void OnPrint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PrintDiceValues();
        }
    }
    
    private void Update()
    {
        // Handle die selection based on the move input
        HandleDieSelection();
        
        // Update the rotations of the dice
        UpdateDiceRotations();
    }
    
    private void UpdateDiceRotations()
    {
        // Update all dice rotations
        for (int i = 0; i < diceObjects.Count; i++)
        {
            if (diceObjects[i] != null && isRotating[i])
            {
                // Increment the timer
                rotationTimers[i] += Time.deltaTime;
                
                // Calculate the lerp factor (0 to 1) based on the timer and rotation speed
                float t = Mathf.Clamp01(rotationTimers[i] / rotationSpeed);
                
                // Apply the rotation using Slerp
                diceObjects[i].rotation = Quaternion.Slerp(startRotations[i], targetRotations[i], t);
                
                // Check if rotation is complete
                if (t >= 1.0f)
                {
                    // Rotation complete, set to exact target and stop rotating
                    diceObjects[i].rotation = targetRotations[i];
                    isRotating[i] = false;
                }
            }
        }
    }
    
    // Handle die selection based on move input
    private void HandleDieSelection()
{
    // Process changes in horizontal movement for die selection
    if (Mathf.Abs(moveInput.x - moveInputPrev.x) > 0.5f)
    {
        int previousIndex = currentDieIndex;
        
        // Detect direction change
        bool moveLeft = moveInput.x < -0.5f && moveInputPrev.x >= -0.5f;
        bool moveRight = moveInput.x > 0.5f && moveInputPrev.x <= 0.5f;
        
        // Track if we need to update
        bool needToUpdate = false;
        
        // Change selected die based on direction
        if (moveLeft && currentDieIndex > 0)
        {
            // Find the next unlocked die to the left
            int newIndex = currentDieIndex - 1;
            while (newIndex >= 0 && lockedDice[newIndex])
            {
                newIndex--;
            }
            
            // If we found an unlocked die
            if (newIndex >= 0)
            {
                currentDieIndex = newIndex;
                needToUpdate = true;
            }
        }
        else if (moveRight && currentDieIndex < diceObjects.Count - 1)
        {
            // Find the next unlocked die to the right
            int newIndex = currentDieIndex + 1;
            while (newIndex < diceObjects.Count && lockedDice[newIndex])
            {
                newIndex++;
            }
            
            // If we found an unlocked die
            if (newIndex < diceObjects.Count)
            {
                currentDieIndex = newIndex;
                needToUpdate = true;
            }
        }
        
        // Update die outline if selection changed
        if (needToUpdate && previousIndex != currentDieIndex)
        {
            // Disable outline on the previously selected die
            if (previousIndex >= 0 && previousIndex < diceObjects.Count)
            {
                Transform previousDie = diceObjects[previousIndex];
                DisableDieOutline(previousDie);
            }
            
            // Update the die outline
            UpdateDieOutline();
        }
    }
    
    // Store current value for next frame comparison
    moveInputPrev = moveInput;
}
    
    private int FindFirstUnlockedDieIndex()
    {
        for (int i = 0; i < lockedDice.Length; i++)
        {
            if (!lockedDice[i])
                return i;
        }
        return -1; // All dice are locked
    }
    private int FindLastUnlockedDieIndex()
    {
        for (int i = lockedDice.Length - 1; i >= 0; i--)
        {
            if (!lockedDice[i])
                return i;
        }
        return -1; // All dice are locked
    }

    private void IncreaseDieValue()
    {
        if (diceObjects.Count > 0 && currentDieIndex >= 0 && currentDieIndex < diceObjects.Count)
        {
            // Only change value if the die is not locked
            if (!IsDieLocked(currentDieIndex))
            {
                diceValues[currentDieIndex]++;
                if (diceValues[currentDieIndex] > maxValue)
                {
                    diceValues[currentDieIndex] = minValue;
                }
                PlayRotationSFX();
                UpdateDieVisual(currentDieIndex);
            }
        }
    }
    
    private void DecreaseDieValue()
    {
        if (diceObjects.Count > 0 && currentDieIndex >= 0 && currentDieIndex < diceObjects.Count)
        {
            // Only change value if the die is not locked
            if (!IsDieLocked(currentDieIndex))
            {
                diceValues[currentDieIndex]--;
                if (diceValues[currentDieIndex] < minValue)
                {
                    diceValues[currentDieIndex] = maxValue;
                }
                PlayRotationSFX();
                UpdateDieVisual(currentDieIndex);
            }
        }
    }
    public void UnlockDie(int dieIndex)
    {
        if (dieIndex >= 0 && dieIndex < lockedDice.Length)
        {
            lockedDice[dieIndex] = false;
        }
    }
    public void ResetAllLocks()
    {
        for (int i = 0; i < lockedDice.Length; i++)
        {
            lockedDice[i] = false;
        }
    }
    public void ResetDiceValue(Transform dieTransform)
    {
        // Check if the transform is in our dice collection
        if (dieTransform != null && diceObjects.Contains(dieTransform))
        {
            int dieIndex = diceObjects.IndexOf(dieTransform);
        
            // Only reset if the die is not locked
            if (!IsDieLocked(dieIndex))
            {
                // Set the value to 1 (minimum value)
                diceValues[dieIndex] = minValue;
            
                // Store the current rotation
                startRotations[dieIndex] = dieTransform.rotation;
            
                // Set the target rotation to show value 1 (0,0,0)
                targetRotations[dieIndex] = Quaternion.Euler(0, 0, 0);
            
                // Reset timer and mark the die as rotating
                rotationTimers[dieIndex] = 0f;
                isRotating[dieIndex] = true;
            }
        }
    }
    public void PlayRotationSFX()
    {
        if (rotationSFX == null || rotationSFX.Length <= 0)
        {
            Debug.LogWarning("PlayRotationSFX called but no rotation sfx available");
            return;
        }
        var index = Random.Range(0, rotationSFX.Length);
        audioSource.PlayOneShot(rotationSFX[index]);
    }
    private void PrintDiceValues()
    {
        string values = "Dice Values: ";
        for (int i = 0; i < diceValues.Length; i++)
        {
            values += "Die " + (i + 1) + ": " + diceValues[i];
            if (i < diceValues.Length - 1)
            {
                values += ", ";
            }
        }
        
        Debug.Log(values);
    }
    
    // Update the target rotation for a specific die based on its value
    private void UpdateDieVisual(int dieIndex)
    {
        if (dieIndex >= 0 && dieIndex < diceObjects.Count && diceObjects[dieIndex] != null)
        {
            int value = diceValues[dieIndex];
            Transform die = diceObjects[dieIndex];
            
            // Set the target rotation based on the die value using the provided rotation values
            Vector3 targetEuler = Vector3.zero;
            
            switch (value)
            {
                case 1:
                    targetEuler = new Vector3(0, 0, 0);
                    break;
                case 2:
                    targetEuler = new Vector3(90, 0, 0);
                    break;
                case 3:
                    targetEuler = new Vector3(90, 0, -90);
                    break;
                case 4:
                    targetEuler = new Vector3(90, 0, 90);
                    break;
                case 5:
                    targetEuler = new Vector3(90, 0, 180);
                    break;
                case 6:
                    targetEuler = new Vector3(180, 0, 180);
                    break;
            }
            
            // Store the current and target rotations
            startRotations[dieIndex] = die.rotation;
            targetRotations[dieIndex] = Quaternion.Euler(targetEuler);
            
            // Reset timer and mark the die as rotating
            rotationTimers[dieIndex] = 0f;
            isRotating[dieIndex] = true;
        }
    }
    
    public void UpdateDieOutline()
    {
        if (diceObjects.Count > 0 && currentDieIndex >= 0 && currentDieIndex < diceObjects.Count)
        {
            Transform currentDie = diceObjects[currentDieIndex];
            
            if (currentDie != null && diceOutlines.ContainsKey(currentDie))
            {
                // Get the outline component
                MonoBehaviour outline = diceOutlines[currentDie];
                
                // Update outline properties in case they were changed in the Inspector
                SetOutlineProperties(outline, outlineColor, outlineWidth);
                
                // Enable the outline
                EnableOutline(outline, true);
            }
        }
    }
    
    public void DisableDieOutline(Transform die)
    {
        if (die != null && diceOutlines.ContainsKey(die))
        {
            // Disable the outline component
            MonoBehaviour outline = diceOutlines[die];
            EnableOutline(outline, false);
        }
    }
    
    private void OnEnable()
    {
        // Enable input actions if they exist
        InputActionMap actionMap = inputActions?.FindActionMap("Gameplay");
        if (actionMap != null) actionMap.Enable();
    }


    private void OnDisable()
    {
        // Disable input actions if they exist
        InputActionMap actionMap = inputActions?.FindActionMap("Gameplay");
        if (actionMap != null) actionMap.Disable();
        
        // Disable all outlines
        foreach (Transform die in diceObjects)
        {
            DisableDieOutline(die);
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
        
        if (changeValueAction != null)
        {
            changeValueAction.performed -= OnChangeValue;
            changeValueAction.canceled -= OnChangeValue;
        }
        
        if (printAction != null)
        {
            printAction.performed -= OnPrint;
        }
        
        // Disable all outlines
        foreach (Transform die in diceObjects)
        {
            DisableDieOutline(die);
        }
    }
    public int GetDieValue(int index)
    {
        if (index >= 0 && index < diceValues.Length)
        {
            return diceValues[index];
        }
        return -1; // Invalid index
    }
}