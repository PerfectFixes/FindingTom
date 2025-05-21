using UnityEngine;

public class DestroyChildOnDisable : MonoBehaviour
{
    private void OnDisable()
    {
        foreach (Transform child in transform)
        {
            if (child.name != "Point Light")
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}