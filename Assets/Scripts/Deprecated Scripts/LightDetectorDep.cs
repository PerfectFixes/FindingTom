/*using UnityEngine;

public class LightDetectorDep : MonoBehaviour
{
    public float timeThreshold = 1f; // Time before triggering an event
    private float outOfLightTime = 0f;
    private bool isInLight = true;

    void Update()
    {
        if (IsPlayerInLight())
        {
            isInLight = true;
            outOfLightTime = 0f; // Reset timer
        }
        else
        {
            if (isInLight)
            {
                isInLight = false;
                outOfLightTime = Time.time; // Start tracking time
            }
            else if (Time.time - outOfLightTime >= timeThreshold)
            {
                Debug.Log("Player is out of the light for too long!");
                // Implement the monster attack logic here
            }
        }
    }

    bool IsPlayerInLight()
    {
        Light[] allLights = FindObjectsOfType<Light>(); // Get all lights in the scene

        foreach (Light light in allLights)
        {
            if (!light.enabled) continue; // Ignore disabled lights

            float distanceToLight = Vector3.Distance(transform.position, light.transform.position);

            if (distanceToLight <= light.range) // Check if the player is within the light's range
            {
                return true;
            }
        }
        return false;
    }
}*/