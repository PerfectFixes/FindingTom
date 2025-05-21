/*using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LockPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("The correct combination to solve the puzzle")]
    public List<int> solution = new List<int>();
    
    [Tooltip("Reference to the dice selection manager")]
    public DiceSelectionManager diceManager;
    
    [Header("Events")]
    [Tooltip("Event triggered when the puzzle is solved")]
    public UnityEvent onPuzzleSolved;
    
    [Tooltip("Event triggered when an incorrect combination is submitted")]
    public UnityEvent onWrongCombination;
    
    [Header("UI References")]
    [Tooltip("Optional UI object to show when puzzle is solved")]
    public GameObject solvedUI;
    
    private bool isPuzzleSolved = false;
    
    private void Start()
    {
        // Hide solved UI initially
        if (solvedUI != null)
        {
            solvedUI.SetActive(false);
        }
        
        // Validate references
        if (diceManager == null)
        {
            Debug.LogError("DiceSelectionManager reference is missing!");
        }
    }
    
    // Call this method to check if the current combination is correct
    public void CheckCombination()
    {
        if (isPuzzleSolved || diceManager == null)
            return;
        
        if (diceManager.CheckSolution(solution))
        {
            SolvePuzzle();
        }
        else
        {
            onWrongCombination?.Invoke();
            Debug.Log("Wrong combination: " + string.Join(", ", diceManager.GetCurrentCombination()));
        }
    }
    
    private void SolvePuzzle()
    {
        if (isPuzzleSolved)
            return;
        
        isPuzzleSolved = true;
        
        // Show the solved UI if available
        if (solvedUI != null)
        {
            solvedUI.SetActive(true);
        }
        
        // Trigger the solved event
        onPuzzleSolved?.Invoke();
        
        Debug.Log("Puzzle solved! Correct combination: " + string.Join(", ", solution));
    }
    
    // Method to check the combination via button press or other trigger
    public void OnSubmitCombination()
    {
        CheckCombination();
    }
    
    // Can be called externally to reset the puzzle
    public void ResetPuzzle()
    {
        isPuzzleSolved = false;
        
        if (solvedUI != null)
        {
            solvedUI.SetActive(false);
        }
    }
}*/