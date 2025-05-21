using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider))]
public class CameraSwapTrigger : MonoBehaviour
{
    public string playerTag = "Player";

    [Header("Camera Swaps ")]
    [Tooltip("המצלמה הפעילה כברירת מחדל")]
    public CinemachineCamera defaultCamera;

    [Tooltip("המצלמה שתידלק כשנכנסים לאזור")]
    public CinemachineCamera triggerCamera;

    private void Reset()
    {
        // מוודא שה-Collider של האובייקט הוא Trigger
        Collider col = GetComponent<Collider>();
        if (col != null) 
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (defaultCamera != null && triggerCamera != null)
            {
                // הגדרת עדיפות מתאימה
                defaultCamera.Priority = 0;
                triggerCamera.Priority = 10;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (defaultCamera != null && triggerCamera != null)
            {
                // החזרת העדיפות לקדמותה
                defaultCamera.Priority = 10;
                triggerCamera.Priority = 0;
            }
        }
    }
}
