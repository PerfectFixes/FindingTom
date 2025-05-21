using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CreditsScroller : MonoBehaviour
{
    [Header("Credits Settings")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private float scrollSpeed = 40f;
    [Tooltip("Space above the screen where credits start")]
    [SerializeField] private float topOffset = 100f;
    [Tooltip("Space below the screen where credits end")]
    [SerializeField] private float bottomOffset = 100f;
    
    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private float delayAfterCredits = 3f;
    [SerializeField] private bool useLoadSceneByName = true;
    
    [Header("Skip Controls")]
    [SerializeField] private InputActionReference skipAction;
    [SerializeField] private bool enableSkipping = true;
    
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeInDuration = 1.5f; // Initial fade-in (from black)
    [SerializeField] private float fadeOutDuration = 1.5f; // Final fade-out (to black)
    [SerializeField] private float initialHoldTime = 0.5f; // Hold black screen before starting fade in
    [SerializeField] private Color fadeColor = Color.black;
    
    private RectTransform textRectTransform;
    private float textHeight;
    private float screenHeight;
    private float totalScrollDistance;
    private bool creditsFinished = false;
    private bool scrollingEnabled = false;
    
    void Start()
    {
        if (creditsText == null)
        {
            Debug.LogError("Credits Text component is not assigned!");
            return;
        }
        
        textRectTransform = creditsText.GetComponent<RectTransform>();
        screenHeight = Screen.height;
        textHeight = textRectTransform.rect.height;
        
        // Position the text below the screen to start
        Vector3 startPosition = textRectTransform.localPosition;
        startPosition.y = -screenHeight / 2 - textHeight / 2 - bottomOffset;
        textRectTransform.localPosition = startPosition;
        
        // Calculate total distance to scroll
        totalScrollDistance = textHeight + screenHeight + topOffset + bottomOffset;
        
        // Set up input action for skipping
        if (enableSkipping && skipAction != null)
        {
            skipAction.action.Enable();
            skipAction.action.performed += ctx => SkipCredits();
        }
        
        // Set up fade image if assigned
        if (fadeImage != null)
        {
            // Ensure the fade image covers the whole screen and starts fully black
            fadeImage.rectTransform.anchorMin = Vector2.zero;
            fadeImage.rectTransform.anchorMax = Vector2.one;
            fadeImage.rectTransform.offsetMin = Vector2.zero;
            fadeImage.rectTransform.offsetMax = Vector2.zero;
            
            // Start with a black screen
            Color startColor = fadeColor;
            startColor.a = 1f; // Fully opaque
            fadeImage.color = startColor;
            
            // Begin initial fade-out (from black to transparent)
            StartCoroutine(InitialFadeOut());
        }
        else
        {
            Debug.LogWarning("Fade Image not assigned. Fade effect will be skipped.");
            // If no fade image, enable scrolling immediately
            scrollingEnabled = true;
        }
    }
    
    void Update()
    {
        if (creditsFinished || !scrollingEnabled)
            return;
            
        // Move the text upward
        Vector3 position = textRectTransform.localPosition;
        position.y += scrollSpeed * Time.deltaTime;
        textRectTransform.localPosition = position;
        
        // Check if credits have finished scrolling
        if (position.y > screenHeight / 2 + textHeight / 2 + topOffset)
        {
            creditsFinished = true;
            StartCoroutine(TransitionToNextScene());
        }
        
        // Legacy input system backup skip option
        if (enableSkipping && skipAction == null && Input.GetKeyDown(KeyCode.Escape))
        {
            SkipCredits();
        }
    }
    
    private IEnumerator InitialFadeOut()
    {
        // Hold the black screen for initialHoldTime seconds
        if (initialHoldTime > 0)
        {
            yield return new WaitForSeconds(initialHoldTime);
        }
        
        float elapsedTime = 0f;
        Color startColor = fadeImage.color; // Should be opaque black
        Color targetColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f); // Transparent
        
        // Gradually decrease alpha over the fade duration
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeInDuration);
            
            fadeImage.color = Color.Lerp(startColor, targetColor, normalizedTime);
            yield return null;
        }
        
        // Ensure we end at fully transparent
        fadeImage.color = targetColor;
        
        // Enable scrolling after fade completes
        scrollingEnabled = true;
    }
    
    private IEnumerator TransitionToNextScene()
    {
        // If we have a fade image, fade to black
        if (fadeImage != null)
        {
            yield return StartCoroutine(FinalFadeIn());
        }
        
        // Wait the specified delay time
        yield return new WaitForSeconds(delayAfterCredits);
        
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("Next scene name is not set!");
            yield break;
        }
        
        if (useLoadSceneByName)
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // Try to load by build index
            try
            {
                int sceneIndex = int.Parse(nextSceneName);
                SceneManager.LoadScene(sceneIndex);
            }
            catch
            {
                Debug.LogError("Failed to parse scene index. Falling back to scene name.");
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
    
    private IEnumerator FinalFadeIn()
    {
        float elapsedTime = 0f;
        Color startColor = fadeImage.color; // Should be transparent
        Color targetColor = fadeColor;
        targetColor.a = 1f; // Fully opaque
        
        // Gradually increase alpha over the fade duration
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeOutDuration);
            
            fadeImage.color = Color.Lerp(startColor, targetColor, normalizedTime);
            yield return null;
        }
        
        // Ensure we end at the target color
        fadeImage.color = targetColor;
    }
    
    private void OnDestroy()
    {
        // Clean up input action subscription
        if (enableSkipping && skipAction != null)
        {
            skipAction.action.performed -= ctx => SkipCredits();
            skipAction.action.Disable();
        }
    }
    
    // Public methods to control scrolling during runtime
    public void SetScrollSpeed(float newSpeed)
    {
        scrollSpeed = newSpeed;
    }
    
    public void SkipCredits()
    {
        if (!creditsFinished)
        {
            creditsFinished = true;
            StartCoroutine(TransitionToNextScene());
        }
    }
    
    // Method to create a fade image at runtime if one wasn't assigned
    public void CreateFadeImageIfNeeded()
    {
        if (fadeImage == null)
        {
            // Create a new Canvas for the fade if needed
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("FadeCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999; // Make sure it renders on top
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create the fade image
            GameObject fadeObj = new GameObject("FadeImage");
            fadeObj.transform.SetParent(canvas.transform, false);
            
            // Set up the image component - start with black
            fadeImage = fadeObj.AddComponent<Image>();
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f); // Start fully opaque
            
            // Make it cover the whole screen
            RectTransform rectTransform = fadeImage.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            Debug.Log("Created fade image automatically");
            
            // Begin the initial fade out
            StartCoroutine(InitialFadeOut());
        }
    }
}