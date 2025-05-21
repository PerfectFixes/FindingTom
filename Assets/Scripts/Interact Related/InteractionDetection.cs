using System;
using StarterAssets;
using UnityEngine;

public class InteractionDetection : MonoBehaviour
{
    [SerializeField] private LayerMask interactionLayer;

    [SerializeField] private Transform raycastCubeTransform;

    [SerializeField] private Mesh raycastCubeMesh;

    [SerializeField] private ThirdPersonController playerController;

    private Interactable currentInteractable;

    private void Awake()
    {
        playerController.InteractionRequested.AddListener(OnInteractionRequested);
    }

    private void OnInteractionRequested()
    {
        if (currentInteractable == null)
        {
            return;
        }

        currentInteractable.Interact();
    }

    // Update is called once per frame
    void Update()
    {
        DetectInteraction();
    }

    private void DetectInteraction()
    {
        var overlaps = Physics.OverlapBox(raycastCubeTransform.position, raycastCubeTransform.localScale * 0.5f,
            raycastCubeTransform.rotation, interactionLayer);
        if (overlaps == null || overlaps.Length == 0)
        {
            ClearTouchingInteractableObject();
        }
        else
        {
            var firstOverlap = overlaps[0];
            PlayerIsTouchingInteractableObject(firstOverlap);
        }
    }

    private void PlayerIsTouchingInteractableObject(Collider collider)
    {
        currentInteractable = collider.GetComponent<Interactable>();
        if (currentInteractable != null)
        {
            currentInteractable.PlayerTouch();
        }
    }

    private void ClearTouchingInteractableObject()
    {
        if (currentInteractable != null)
        {
            currentInteractable.InteractExit();
            currentInteractable = null;
        }
    }
/*#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireMesh(raycastCubeMesh, raycastCubeTransform.position, raycastCubeTransform.rotation,
            raycastCubeTransform.localScale);
#endif
    }*/


}
