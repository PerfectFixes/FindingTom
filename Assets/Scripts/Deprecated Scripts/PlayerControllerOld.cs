/*using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    
    private Vector2 moveInput;
    private CharacterController characterController;
    private Transform cameraTransform;
    private PlayerInput playerInput;
    
    private void Awake()
    {
        // Get required components
        characterController = GetComponent<CharacterController>();
        
        // If camera reference is not set, try to find main camera
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        if (mainCamera != null)
            cameraTransform = mainCamera.transform;
    }

    // This is the method that will be called by the PlayerInput component
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    
    private void Update()
    {
        Move();
    }
    
    private void Move()
    {
        if (moveInput == Vector2.zero) return;
        
        // Convert input to world space direction based on camera orientation
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        // Project vectors on the horizontal plane
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        // Calculate movement direction
        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
        
        // Move the character
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        
        // Rotate the character to face movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}*/