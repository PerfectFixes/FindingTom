using UnityEngine;

public class PuzzleInteraction : MonoBehaviour
{
    private Canvas interactCanvas;
    public bool hasInteracted = false;
    
    [SerializeField]
    private PlayerStateControl playerStateControl;
    
    [SerializeField]
    private PlayerStateControl.PlayerState puzzleType;
    
    
    public bool isPuzzleLocked = false; 
   
    private void Awake()
    {
        interactCanvas = GetComponentInChildren<Canvas>();
        interactCanvas.gameObject.SetActive(false);
    }

    public void StartPuzzle()
    {
        if (hasInteracted || isPuzzleLocked)
        {
            return;
        }
        playerStateControl.SetPlayerState(puzzleType);
    }

    public void PuzzleComplete()
    {
        HideUI();
        hasInteracted = true;
        // Change this GameObject's layer
        gameObject.layer = LayerMask.NameToLayer(default);
    }
    public void DisplayUI()
    {
        if (hasInteracted || isPuzzleLocked)
        {
            return;
        }
        interactCanvas.gameObject.SetActive(true);
    }
    public void UnlockPuzzle()
    {
        isPuzzleLocked = false;
    }
    public void HideUI()
    {
        interactCanvas.gameObject.SetActive(false);
    }
}