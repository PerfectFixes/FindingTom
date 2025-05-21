using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DiceCombinationPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("The dice controller script")]
    public DieController dieController;
    
    [Tooltip("The secret combination to unlock the puzzle")]
    public int[] secretCombination = new int[5] { 1, 2, 3, 4, 5 };
    
    [Header("Feedback Settings")]
    [Tooltip("Time between each die feedback effect (seconds)")]
    public float timeBetweenEffects = 0.5f;
    
    /*[Tooltip("GameObject that will show correct answer effects")]
    public ParticleSystem correctEffectPrefab;
    
    [Tooltip("GameObject that will show incorrect answer effects")]
    public ParticleSystem incorrectEffectPrefab;*/
    
    [Tooltip("Additional delay after checking before returning control")]
    public float returnControlDelay = 1.0f;
    
    [Header("Input Settings")]
    [Tooltip("Input Action Asset with the 'Select' action")]
    public InputActionAsset inputActions;
    
    [Tooltip("Name of the action map containing the Select action")]
    public string actionMapName = "Gameplay";
    
    [Tooltip("Name of the action to check combination")]
    public string selectActionName = "Select";

    [Tooltip("Name of the action to exit the puzzle")]
    public string exitActionName = "Exit";
    
    [Tooltip("Fallback key in case input system fails")]
    public KeyCode fallbackKey = KeyCode.E;
    
    [Tooltip("Fallback key for exit in case input system fails")]
    public KeyCode fallbackExitKey = KeyCode.Escape;
    
    [Header("State Management")]
    [Tooltip("Player GameObject to enable/disable during checking")]
    public PlayerStateControl playerStateControl;
    
    // Add this to the class properties
    [Header("Child GameObject Settings")]
    [Tooltip("Name of the child GameObject to activate when a die is correct")]
    public string correctChildObjectName = "Point Light";

    
    // Input action references
    private InputAction selectAction;
    private InputAction exitAction;
    private InputActionMap actionMap;
    private bool inputSystemInitialized = false;
    
    private bool isPuzzleSolved = false;
    private bool isCheckingCombination = false;
    private Coroutine checkCoroutine;
    
    // Track original selection to restore it
    private int originalSelectedDieIndex = -1;
    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip correctAudioClip;
    public AudioClip incorrectAudioClip;
    public AudioClip unloadPuzzleAudioClip;
    [Range(0, 1)] public float interactAudioVolume = 0.5f;
    
    [SerializeField]
    private UnityEvent puzzleCompleted;
    
    [SerializeField]
    private UnityEvent onPuzzleExit;

    private bool isCheckOnCooldown;
    
    private void Awake()
    {
        // Validate references
        if (dieController == null)
        {
            Debug.LogError("DieController reference is missing! Please assign it in the Inspector.");
            enabled = false;
            return;
        }
        
        // Set up input actions
        inputSystemInitialized = SetupInputActions();
        
        if (!inputSystemInitialized)
        {
            Debug.LogWarning("Input system not initialized correctly. Using fallback keys " + fallbackKey.ToString() + " and " + fallbackExitKey.ToString());
        }
    }

    private void Start()
    {
        // Ensure outline is visible on the initial die
        if (dieController != null && dieController.diceObjects.Count > 0)
        {
            // Force update outline on first selected die
            dieController.UpdateDieOutline();
        }
    }
    
    private bool SetupInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Actions asset not assigned! Using fallback keys instead.");
            return false;
        }
        
        try
        {
            // Try to find the specified action map
            actionMap = inputActions.FindActionMap(actionMapName);
            
            // If not found, try to use the first available action map
            if (actionMap == null)
            {
                if (inputActions.actionMaps.Count > 0)
                {
                    actionMap = inputActions.actionMaps[0];
                    Debug.LogWarning("Action map '" + actionMapName + "' not found. Using '" + actionMap.name + "' instead.");
                }
                else
                {
                    Debug.LogError("No action maps found in the input actions asset!");
                    return false;
                }
            }
            
            // Find the Select action
            selectAction = actionMap.FindAction(selectActionName);
            
            if (selectAction == null)
            {
                Debug.LogError("Could not find '" + selectActionName + "' action in the action map!");
                return false;
            }
            
            // Find the Exit action
            exitAction = actionMap.FindAction(exitActionName);
            
            if (exitAction == null)
            {
                Debug.LogWarning("Could not find '" + exitActionName + "' action in the action map! Exit functionality will use fallback key only.");
            }
            else
            {
                // Register callback for the exit action
                exitAction.performed += OnExit;
            }
            
            // Register callback for the select action and enable
            selectAction.performed += OnSelect;
            
            // Enable the action map
            actionMap.Enable();
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error setting up input actions: " + e.Message);
            return false;
        }
    }
    
    private void Update()
    {
        // Fallback input detection if input system isn't working
        if (!inputSystemInitialized)
        {
            // Check for select fallback key
            if (Input.GetKeyDown(fallbackKey) && !isCheckingCombination)
            {
                CheckCombinationManually();
            }
            
            // Check for exit fallback key
            if (Input.GetKeyDown(fallbackExitKey))
            {
                ReturnToPlayerFromPuzzle();
            }
        }
    }
    
    private void OnSelect(InputAction.CallbackContext context)
    {
        // Only respond if not already checking
        if (context.performed && !isCheckingCombination)
        {
            
            // Cancel any running check
            if (checkCoroutine != null)
            {
                StopCoroutine(checkCoroutine);
            }

            if (isCheckOnCooldown)
            {
                return;
            }
            // Start a new check
            checkCoroutine = StartCoroutine(CheckCombination());
        }
    }
    
    private void OnExit(InputAction.CallbackContext context)
    {
        // Only respond if not already checking
        if (context.performed)
        {
            ReturnToPlayerFromPuzzle();
        }
    }
    
    private void CheckCombinationManually()
    {
        if (!isCheckingCombination)
        {
            // Cancel any running check
            if (checkCoroutine != null)
            {
                StopCoroutine(checkCoroutine);
            }
            if (isCheckOnCooldown)
            {
                return;
            }
            // Start a new check
            checkCoroutine = StartCoroutine(CheckCombination());
        }
    }
    
    // Disable all die interactions and hide outline
    private void DisableDicePuzzleState()
    {
        if (dieController != null)
        {
            
            // Save the current selected die index to restore later if needed
            originalSelectedDieIndex = dieController.currentDieIndex;
            
            // Disable die outline by calling DisableTowerOutline on all dice
            HideAllDiceOutlines();
            
            // Disable input to prevent value changes and selection movement
            DisableDiceInput();
        }
    }
    
    // Re-enable die interactions and show outline
    private void EnableDicePuzzleState()
    {
        if (dieController != null && !isPuzzleSolved)
        {
            
            // Re-enable the die input
            EnableDiceInput();
            
            // Restore the selected die outline if we have a valid index
            if (originalSelectedDieIndex >= 0 && originalSelectedDieIndex < dieController.diceObjects.Count)
            {
                // Set the die controller's current die index back to the original
                RestoreSelectedDieOutline();
            }
        }
    }
    
    // Hide all dice outlines 
    private void HideAllDiceOutlines()
    {
        // Call a method to hide outlines in the die controller
        // This will use the accessibility of your DieController class members
        
        // First save the current selected die 
        if (dieController.diceObjects.Count > 0 && dieController.currentDieIndex >= 0 && 
            dieController.currentDieIndex < dieController.diceObjects.Count)
        {
            originalSelectedDieIndex = dieController.currentDieIndex;
            
            // Access the outline dictionary and disable all outlines
            foreach (Transform die in dieController.diceObjects)
            {
                dieController.DisableDieOutline(die);
            }
        }
    }
    
    // Restore the selected die outline
    private void RestoreSelectedDieOutline()
    {
        if (originalSelectedDieIndex >= 0 && originalSelectedDieIndex < dieController.diceObjects.Count)
        {
            // Set the current die index back
            dieController.currentDieIndex = originalSelectedDieIndex;
            
            // Update the outline
            dieController.UpdateDieOutline();
        }
    }
    
    // Disable the dice input
    private void DisableDiceInput()
    {
        // Temporarily disable all input in the die controller
        if (dieController.inputActions != null)
        {
            foreach (InputActionMap map in dieController.inputActions.actionMaps)
            {
                map.Disable();
            }
        }
    }
    
    // Re-enable the dice input
    private void EnableDiceInput()
    {
        // Re-enable all input in the die controller
        if (dieController.inputActions != null)
        {
            foreach (InputActionMap map in dieController.inputActions.actionMaps)
            {
                map.Enable();
            }
        }
    }
    
    // Toggle player movement state (switching to moving state)
    private void ReturnToPlayerFromPuzzle()
    {
        if (playerStateControl != null)
        {
            audioSource.PlayOneShot(unloadPuzzleAudioClip);
            playerStateControl.SetPlayerState(PlayerStateControl.PlayerState.Moving);

            if (isPuzzleSolved)
            {
                return;
            }
            onPuzzleExit?.Invoke();
        }
    }
    
 private IEnumerator CheckCombination()
{
    // Set state to checking
    isCheckingCombination = true;
    
    // Disable dice puzzle state (hide outline, disable interaction)
    DisableDicePuzzleState();

    // Make sure all outlines are disabled during the check
    foreach (Transform die in dieController.diceObjects)
    {
        dieController.DisableDieOutline(die);
    }

    // Get current dice values
    int[] currentValues = new int[dieController.diceObjects.Count];
    for (int i = 0; i < dieController.diceObjects.Count; i++)
    {
        currentValues[i] = dieController.GetDieValue(i);
    }
    
    // Check if there are enough dice for the combination
    if (currentValues.Length < secretCombination.Length)
    {
        // Return control
        isCheckingCombination = false;
        yield break;
    }
    
    bool allCorrect = true;
    // Store which dice are newly correct in this check
    bool[] newlyCorrectDice = new bool[dieController.diceObjects.Count];
    
    // Check each die one by one with a delay between
    for (int i = 0; i < secretCombination.Length; i++)
    {
        // Skip already locked dice (they are already correct)
        if (dieController.IsDieLocked(i))
        {
            continue;
        }
        
        bool isCorrect = currentValues[i] == secretCombination[i];
        
        // Show the appropriate effect
        ShowEffect(dieController.diceObjects[i], isCorrect);
        
        // Track if this die is newly correct
        if (isCorrect)
        {
            // Play incorrect sound for incorrect dice
            if (incorrectAudioClip != null)
            {
                audioSource.PlayOneShot(correctAudioClip);
            }

            newlyCorrectDice[i] = true;
        }
        else
        {
            // Play incorrect sound for incorrect dice
            if (incorrectAudioClip != null)
            {
                audioSource.PlayOneShot(incorrectAudioClip);
            }
            
            // Reset the incorrect die value
            dieController.ResetDiceValue(dieController.diceObjects[i]);
            allCorrect = false;
        }
        
        // Process all newly correct dice immediately
        for (int j = 0; j < newlyCorrectDice.Length; j++)
        {
            if (newlyCorrectDice[j])
            {
                // Lock the die so it can't be changed
                dieController.LockDie(j);
                
                // Ensure no outline is shown on this locked die
                dieController.DisableDieOutline(dieController.diceObjects[j]);
                
                // Activate the first child GameObject if it exists
                Transform dieTransform = dieController.diceObjects[j];
                if (dieTransform.childCount > 0)
                {
                    // Activate the first child GameObject - no name needed
                    Transform childObject = dieTransform.GetChild(0);
                    childObject.gameObject.SetActive(true);
                }
            }
        }
        
        // Clear the newly correct dice array for the next iteration
        for (int j = 0; j < newlyCorrectDice.Length; j++)
        {
            newlyCorrectDice[j] = false;
        }
        
        // Wait before checking the next die
        yield return new WaitForSeconds(timeBetweenEffects);
    }
    
    // If all correct, mark the puzzle as solved
    if (allCorrect)
    {
        isPuzzleSolved = true;
        puzzleCompleted.Invoke();
    }

    // Wait a bit before returning control
    yield return new WaitForSeconds(returnControlDelay);
    
    // Additional delay of 0.5 seconds before returning to player movement
    yield return new WaitForSeconds(0.5f);
    
    // Return control based on puzzle state
    isCheckingCombination = false;
    
    // If not all correct, we need to properly set up the next available die
    if (!isPuzzleSolved)
    {
        // Find the first unlocked die to select
        int firstUnlockedDie = -1;
        for (int i = 0; i < dieController.diceObjects.Count; i++)
        {
            if (!dieController.IsDieLocked(i))
            {
                firstUnlockedDie = i;
                break;
            }
        }
        
        // If we found an unlocked die, select it
        if (firstUnlockedDie >= 0)
        {
            dieController.currentDieIndex = firstUnlockedDie;
            dieController.UpdateDieOutline();
        }
    }
    
    // Switch to player movement
    ReturnToPlayerFromPuzzle();
}
private void ShowEffect(Transform dieTransform, bool isCorrect)
{
    // Create the appropriate effect at the die's position
    /*ParticleSystem effectPrefab = isCorrect ? correctEffectPrefab : incorrectEffectPrefab;
    
    if (effectPrefab != null)
    {
        // Instantiate the effect slightly above the die
        Vector3 effectPosition = dieTransform.position + Vector3.up * 0.5f;
        ParticleSystem effectInstance = Instantiate(effectPrefab, effectPosition, Quaternion.identity);
        
        // Parent to the die so it moves with it
        effectInstance.transform.SetParent(dieTransform);

        if (isCorrect)
        {
            // Play the correct audio
            if (correctAudioClip != null)
            {
                audioSource.PlayOneShot(correctAudioClip);
            }
            else
            {
                Debug.LogWarning("Audio Clip Not Found");
            }
            
            // Play particle effect
            correctEffectPrefab.Play();
        }
    }
    else
    {
        Debug.LogWarning("No effect prefab assigned for " + (isCorrect ? "correct" : "incorrect") + " answer!");
    }*/
}
private void OnEnable()
{
    StartCoroutine(AddCooldownForStartCheck());
    if (inputSystemInitialized && actionMap != null)
    {
        actionMap.Enable();
    }

    // Reset checking state
    isCheckingCombination = false;

    // Only re-enable dice control if puzzle is not solved
    if (!isPuzzleSolved)
    {
        // Re-enable dice input first
        EnableDiceInput();
        
        // Find the first unlocked die and select it
        if (dieController != null && dieController.diceObjects.Count > 0)
        {
            // Find the first unlocked die
            int firstUnlockedIndex = FindFirstUnlockedDieIndex();
            
            // If we found an unlocked die, select it
            if (firstUnlockedIndex >= 0)
            {
                // First clear all outlines
                foreach (Transform die in dieController.diceObjects)
                {
                    dieController.DisableDieOutline(die);
                }
                
                // Set the current die index to the first unlocked die
                dieController.currentDieIndex = firstUnlockedIndex;
                
                // Update the outline to make it visible
                dieController.UpdateDieOutline();
            }
        }
    }
}
// Helper method to find the first unlocked die
private int FindFirstUnlockedDieIndex()
{
    for (int i = 0; i < dieController.diceObjects.Count; i++)
    {
        if (!dieController.IsDieLocked(i))
        {
            return i;
        }
    }
    
    // If all dice are locked (shouldn't happen in normal gameplay), return 0
    return 0;
}
    private void OnDisable()
       {
           if (inputSystemInitialized && actionMap != null)
           {
               actionMap.Disable();
           }
       }
    
    private void OnDestroy()
    {
        // Clean up callbacks
        if (inputSystemInitialized && selectAction != null)
        {
            selectAction.performed -= OnSelect;
        }
        
        if (inputSystemInitialized && exitAction != null)
        {
            exitAction.performed -= OnExit;
        }
    }
    // Utility method to help reset the puzzle (pass this to DieController)
    private void ResetDieLock(int dieIndex)
    {
        if (dieController != null)
        {
            // Get the private field through reflection (not ideal but works for this context)
            System.Reflection.FieldInfo field = typeof(DieController).GetField("lockedDice", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
            if (field != null)
            {
                bool[] lockedDice = field.GetValue(dieController) as bool[];
                if (lockedDice != null && dieIndex >= 0 && dieIndex < lockedDice.Length)
                {
                    lockedDice[dieIndex] = false;
                }
            }
        }
    }
    // Public method to reset the puzzle if needed
    // Modify the ResetPuzzle method to also reset the locked state and child objects
    public void ResetPuzzle()
    {
        isPuzzleSolved = false;
        isCheckingCombination = false;
    
        // Reset all dice
        for (int i = 0; i < dieController.diceObjects.Count; i++)
        {
            Transform dieTransform = dieController.diceObjects[i];
        
            // Reset locked state
            if (dieController.IsDieLocked(i))
            {
                // Reset the die lock
                dieController.UnlockDie(i);
            
                // Find and deactivate the child object
                Transform childObject = dieTransform.Find(correctChildObjectName);
                if (childObject != null)
                {
                    childObject.gameObject.SetActive(false);
                }
            }
        
            // Reset to minimum value
            dieController.ResetDiceValue(dieTransform);
        }
    
        EnableDicePuzzleState();
    }
    IEnumerator AddCooldownForStartCheck()
    {
        isCheckOnCooldown = true;
        yield return new WaitForSeconds(2f);
        isCheckOnCooldown = false;
    }
}