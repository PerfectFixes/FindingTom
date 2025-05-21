using UnityEngine;

[CreateAssetMenu(fileName = "LaserMaterial", menuName = "Laser Puzzle/Laser Material")]
public class LaserMaterial : ScriptableObject
{
    [Header("Emission Settings")]
    [Tooltip("Color of the laser")]
    public Color color = Color.red;
    
    [Tooltip("Emission intensity")]
    [Range(0, 10)]
    public float intensity = 3.0f;
    
    [Header("Material Settings")]
    [Tooltip("Base material to modify (optional)")]
    public Material baseMaterial;
    
    // Runtime cached material
    private Material _cachedMaterial;
    
    /// <summary>
    /// Create or get a material with laser-like properties
    /// </summary>
    public Material GetMaterial()
    {
        if (_cachedMaterial == null)
        {
            CreateMaterial();
        }
        return _cachedMaterial;
    }
    
    /// <summary>
    /// Create a material with laser-like properties
    /// </summary>
    private void CreateMaterial()
    {
        // Create a new material based on the standard shader
        if (baseMaterial != null)
        {
            _cachedMaterial = new Material(baseMaterial);
        }
        else
        {
            _cachedMaterial = new Material(Shader.Find("Standard"));
            _cachedMaterial.SetFloat("_Mode", 1); // Set to transparent
            _cachedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _cachedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _cachedMaterial.SetInt("_ZWrite", 0);
            _cachedMaterial.DisableKeyword("_ALPHATEST_ON");
            _cachedMaterial.EnableKeyword("_ALPHABLEND_ON");
            _cachedMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            _cachedMaterial.renderQueue = 3000;
        }
        
        // Set emission properties
        _cachedMaterial.EnableKeyword("_EMISSION");
        _cachedMaterial.SetColor("_EmissionColor", color * intensity);
        _cachedMaterial.SetColor("_Color", new Color(color.r, color.g, color.b, 0.8f));
        
        /*// Make it glow in the scene view too
        #if UNITY_EDITOR
        UnityEditor.MaterialPropertyBlock block = new UnityEditor.MaterialPropertyBlock();
        block.SetColor("_EmissionColor", color * intensity);
        #endif*/
    }
}