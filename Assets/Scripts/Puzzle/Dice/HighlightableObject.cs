using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SimpleHighlighter : MonoBehaviour
{
    [Header("Outline Settings")]
    [Tooltip("Outline color for the highlighted object")]
    public Color outlineColor = Color.yellow;
    
    [Tooltip("Outline width for the highlighted object")]
    [Range(0f, 50f)]
    public float outlineWidth = 5f;
    
    public bool canHightlight = true;

    // Reference to the outline component
    private MonoBehaviour outlineComponent;

    private void Awake()
    {
        // Try to find or add an Outline component
        outlineComponent = FindOutlineComponent(gameObject);
        
        if (outlineComponent != null)
        {
            // Configure and disable initially
            SetOutlineProperties(outlineComponent, outlineColor, outlineWidth);
            EnableOutline(outlineComponent, false);
        }
        else
        {
            Debug.LogWarning("No Outline component found on " + gameObject.name + 
                ". Make sure you've added the Quick Outline component to this object.");
        }
    }

    /// <summary>
    /// Highlights this object by enabling its outline
    /// </summary>
    public void Highlight()
    {
        if (outlineComponent != null && canHightlight)
        {
            // Update outline properties in case they were changed in the Inspector
            SetOutlineProperties(outlineComponent, outlineColor, outlineWidth);
            
            // Enable the outline
            EnableOutline(outlineComponent, true);
        }
    }

    /// <summary>
    /// Unhighlights this object by disabling its outline
    /// </summary>
    public void Unhighlight()
    {
        if (outlineComponent != null && canHightlight)
        {
            // Disable the outline
            EnableOutline(outlineComponent, false);
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

    public void EnableHighlight()
    {
        canHightlight = true;
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

    public void DestroyObject()
    {
        Destroy(gameObject);
    }
    private void OnDisable()
    {
        // Make sure to unhighlight when disabled
        Unhighlight();
    }

    private void OnDestroy()
    {
        // Make sure to unhighlight when destroyed
        Unhighlight();
    }
}