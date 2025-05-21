using UnityEngine;
using UnityEditor;

// This script will only work in the Unity Editor
[ExecuteInEditMode]
public class EnablePlayerObjects : MonoBehaviour
{
    // Reference to the GameObject you want to toggle
    public GameObject playerObject;
    
    // Track if the F3 key was previously pressed
    private bool wasF3Pressed = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3) && !wasF3Pressed)
        {
            wasF3Pressed = true;
            playerObject.SetActive(!playerObject.activeSelf);
        }
       
    }

    // Optional: Add a custom editor script to show instructions
    #if UNITY_EDITOR
    [CustomEditor(typeof(EnablePlayerObjects))]
    public class EnablePlayerObjectsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Assign a GameObject to toggle with F3 key press (Editor only).", MessageType.Info);
        }
    }
    #endif
}