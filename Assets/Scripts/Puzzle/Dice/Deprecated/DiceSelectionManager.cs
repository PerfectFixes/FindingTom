/*using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiceSelectionManager : MonoBehaviour
{
    [Header("Dice Settings")]
    [Tooltip("List of all dice that can be controlled")]
    public List<DieController> dice = new List<DieController>();
    
    [Header("Outline Settings")]
    [Tooltip("Outline color for the selected die")]
    public Color outlineColor = Color.yellow;
    
    [Tooltip("Outline width for the selected die")]
    [Range(0f, 10f)]
    public float outlineWidth = 5f;
    
    [Header("Input Settings")]
    [Tooltip("Reference to an Input Action asset")]
    public InputActionAsset inputActions;
    
    // Currently selected die index
    private int currentDieIndex = 0;
    
    // Input actions
    private InputAction horizontalAction;
    private InputAction verticalAction;
    private InputAction submitAction;
    
    // Previous input values for edge detection
    private float prevHorizontal = 0f;
    private float prevVertical = 0f;
    
    private void Awake()
    {
        // Setup input actions
        if (inputActions != null)
        {
            InputActionMap actionMap = inputActions.FindActionMap("DicePuzzle");
            if (actionMap == null)
            {
                // Try to find default action map
                actionMap = inputActions.actionMaps[0];
                Debug.Log($"Using action map: {actionMap.name}");
            }
            
            // Find actions
            horizontalAction = actionMap.FindAction("DiceNavigateHorizontal");
            verticalAction = actionMap.FindAction("DiceNavigateVertical");
            submitAction = actionMap.FindAction("DiceSubmit");
            
            // Enable actions
            actionMap.Enable();
        }
        else
        {
            Debug.LogError("Input Actions asset not assigned!");
        }
        
        // Select first die
        if (dice.Count > 0)
        {
            UpdateDieSelection();
        }
    }
    
    private void Update()
    {
        if (horizontalAction != null && verticalAction != null)
        {
            // Get input values
            float horizontal = horizontalAction.ReadValue<float>();
            float vertical = verticalAction.ReadValue<float>();
            
            // Handle horizontal input (die selection)
            if (Mathf.Abs(horizontal - prevHorizontal) > 0.5f)
            {
                bool moveLeft = horizontal < -0.5f && prevHorizontal >= -0.5f;
                bool moveRight = horizontal > 0.5f && prevHorizontal <= 0.5f;
                
                if (moveLeft && currentDieIndex > 0)
                {
                    currentDieIndex--;
                    UpdateDieSelection();
                    Debug.Log($"Selected die {currentDieIndex}");
                }
                else if (moveRight && currentDieIndex < dice.Count - 1)
                {
                    currentDieIndex++;
                    UpdateDieSelection();
                    Debug.Log($"Selected die {currentDieIndex}");
                }
            }
            
            // Handle vertical input (die value)
            if (Mathf.Abs(vertical - prevVertical) > 0.5f)
            {
                bool moveUp = vertical > 0.5f && prevVertical <= 0.5f;
                bool moveDown = vertical < -0.5f && prevVertical >= -0.5f;
                
                if (moveUp && currentDieIndex >= 0 && currentDieIndex < dice.Count)
                {
                    dice[currentDieIndex].IncrementValue();
                    Debug.Log($"Die {currentDieIndex} value: {dice[currentDieIndex].CurrentValue}");
                }
                else if (moveDown && currentDieIndex >= 0 && currentDieIndex < dice.Count)
                {
                    dice[currentDieIndex].DecrementValue();
                    Debug.Log($"Die {currentDieIndex} value: {dice[currentDieIndex].CurrentValue}");
                }
            }
            
            // Update previous values
            prevHorizontal = horizontal;
            prevVertical = vertical;
        }
    }
    
    private void UpdateDieSelection()
    {
        // Reset outline on all dice
        foreach (var die in dice)
        {
            if (die != null)
            {
                die.SetSelected(false, outlineColor, outlineWidth);
            }
        }
        
        // Enable outline on the selected die
        if (currentDieIndex >= 0 && currentDieIndex < dice.Count)
        {
            dice[currentDieIndex].SetSelected(true, outlineColor, outlineWidth);
        }
    }
    
    private void OnEnable()
    {
        // Enable input actions
        InputActionMap actionMap = inputActions?.FindActionMap("DicePuzzle");
        if (actionMap != null) actionMap.Enable();
    }
    public bool CheckSolution(List<int> solution)
    {
        if (solution.Count != dice.Count)
            return false;
    
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i].CurrentValue != solution[i])
                return false;
        }
    
        return true;
    }

// Get the current combination as a list of integers
    public List<int> GetCurrentCombination()
    {
        List<int> combination = new List<int>();
        foreach (var die in dice)
        {
            combination.Add(die.CurrentValue);
        }
        return combination;
    }
    private void OnDisable()
    {
        // Disable input actions
        InputActionMap actionMap = inputActions?.FindActionMap("DicePuzzle");
        if (actionMap != null) actionMap.Disable();
        
        // Disable all outlines
        foreach (var die in dice)
        {
            if (die != null)
            {
                die.SetSelected(false, outlineColor, outlineWidth);
            }
        }
    }
}*/