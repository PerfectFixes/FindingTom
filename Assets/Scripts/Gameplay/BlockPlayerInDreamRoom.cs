using System;
using UnityEngine;

public class BlockPlayerInDreamRoom : MonoBehaviour
{
    private bool blockOnce = false;
    private BoxCollider parentCollider;

    void Start()
    {
        blockOnce = false;

        // Ensure we get only the parent's BoxCollider, not this object's collider
        if (transform.parent != null)
        {
            parentCollider = transform.parent.GetComponent<BoxCollider>();
        }

        if (parentCollider == null)
        {
            Debug.LogWarning("Parent does not have a BoxCollider!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !blockOnce && parentCollider != null)
        {
            blockOnce = true;
            parentCollider.isTrigger = false; // Set only the parent's collider to not be a trigger
        }
    }
}