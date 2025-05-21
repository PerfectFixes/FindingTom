using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SimpleFader : MonoBehaviour
{
    [SerializeField] private float defaultDuration = 1.0f;
    [SerializeField] private float defaultHoldTime = 0.5f;
    
    [SerializeField] private Image image;
    private Coroutine fadeCoroutine;
    private bool isFading = false;
    
    private void Awake()
    {
        // Get the image component
        image = gameObject.GetComponent<Image>();
    }

    // Fade in (from transparent to opaque)
    public void FadeIn(float duration = -1f, float holdTime = -1f)
    {
        float time = duration > 0 ? duration : defaultDuration;
        float hold = holdTime > 0 ? holdTime : defaultHoldTime;
        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
            
        isFading = true;
        fadeCoroutine = StartCoroutine(DoFade(0f, 1f, time, hold));
    }
    
    // Fade out (from opaque to transparent)
    public void FadeOut(float duration = -1f, float holdTime = -1f)
    {
        float time = duration > 0 ? duration : defaultDuration;
        float hold = holdTime > 0 ? holdTime : defaultHoldTime;
        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
            
        isFading = true;
        fadeCoroutine = StartCoroutine(DoFade(1f, 0f, time, hold));
    }
    
    // Set alpha instantly
    public void SetAlpha(float alpha)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        isFading = false;
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
    
    // Check if the fader is currently animating
    public bool IsFading()
    {
        return isFading;
    }
    
    // Get the current alpha value
    public float GetCurrentAlpha()
    {
        return image.color.a;
    }
    
    // Get the default duration
    public float GetDefaultDuration()
    {
        return defaultDuration;
    }
    
    // Get the default hold time
    public float GetDefaultHoldTime()
    {
        return defaultHoldTime;
    }
    
    // Wait for fade to complete and then execute an action
    public Coroutine WaitForFade(System.Action onComplete)
    {
        return StartCoroutine(WaitForFadeComplete(onComplete));
    }
    
    private IEnumerator WaitForFadeComplete(System.Action onComplete)
    {
        // Wait until the fading operation is complete
        while (isFading)
        {
            yield return null;
        }
        
        // Execute the callback
        onComplete?.Invoke();
    }
    
    private IEnumerator DoFade(float startAlpha, float endAlpha, float duration, float holdTime)
    {
        // Set starting alpha
        Color color = image.color;
        color.a = startAlpha;
        image.color = color;
        
        // Hold at start alpha if requested (black screen hold)
        if (holdTime > 0)
        {
            yield return new WaitForSeconds(holdTime);
        }
        
        // Do the fade
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            color = image.color;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            image.color = color;
            
            yield return null;
        }
        
        // Ensure final alpha is exact
        color = image.color;
        color.a = endAlpha;
        image.color = color;
        
        isFading = false;
        fadeCoroutine = null;
    }
}