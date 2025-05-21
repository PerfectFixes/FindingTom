using UnityEngine;

public class PaperFoldTimeSetter : MonoBehaviour
{
    [SerializeField] 
    private Material material;

    [SerializeField] 
    private bool isTimeUnscaled = true;

    private float activationTime;

    private void OnEnable()
    {
        // Store the current time when this object gets enabled
        activationTime = isTimeUnscaled ? Time.unscaledTime : Time.time;
    }

    private void Update()
    {
        if (material != null)
        {
            float elapsedTime = (isTimeUnscaled ? Time.unscaledTime : Time.time) - activationTime;
            material.SetFloat("_UnscaledTime", elapsedTime);
        }
    }
}
