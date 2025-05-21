using UnityEngine;
using Random = UnityEngine.Random;

public class BallController : MonoBehaviour
{
    [Header("Physics Settings")] [Tooltip("How much to dampen the ball's rolling")] [Range(0f, 10f)] [SerializeField]
    private float angularDrag = 2f;

    [Tooltip("How much to dampen the ball's movement")] [Range(0f, 10f)] [SerializeField]
    private float linearDrag = 0.5f;

    [Tooltip("Ball mass in kg")] [Range(0.1f, 10f)] [SerializeField]
    private float mass = 1f;

    [Tooltip("Extra downward force to keep ball grounded")] [Range(0f, 20f)] [SerializeField]
    private float extraGravity = 9.8f;

    [Tooltip("Maximum velocity magnitude")] [SerializeField]
    private float maxVelocity = 8f;

    // Component references
    private Rigidbody rb;
    private SphereCollider sphereCollider;

    private Vector3 previousPlatformPosition;
    private bool isInitialized = false;

    [Header("SFX")] 
    [SerializeField] 
    private AudioClip[] CollisionSFX;

    [SerializeField] 
    private AudioSource audioSource;

    private void Awake()
    {
        // Get required components
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody component missing from the ball!");
            return;
        }

        // Configure the rigidbody for better physics
        ConfigureRigidbody();
    }

    private void Start()
    {
        // Find the platform and store its initial position
        GameObject platform = GameObject.FindGameObjectWithTag("MazePlatform");
        if (platform != null)
        {
            previousPlatformPosition = platform.transform.position;
        }

        isInitialized = true;
    }

    private void ConfigureRigidbody()
    {
        // Apply physics settings to make ball more responsive
        rb.mass = mass;
        rb.linearDamping = linearDrag;
        rb.angularDamping = angularDrag;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = true;
    }

    private void FixedUpdate()
    {
        if (!isInitialized || rb == null) return;

        // Apply extra downward force to keep ball close to surface
        rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        // Limit maximum velocity to prevent excessive speed
        if (rb.linearVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }

        // Check if ball has fallen through the platform
        if (transform.position.y < -50f)
        {
            ResetBall();
        }
    }

    private void ResetBall()
    {
        // Reset position slightly above the maze platform
        GameObject platform = GameObject.FindGameObjectWithTag("MazePlatform");
        if (platform != null)
        {
            transform.position = platform.transform.position + Vector3.up * 1f;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void WallHittingSFX()
    {
        if (CollisionSFX.Length > 0)
        {
            if (audioSource.isPlaying)
            {
                return;
            }

            var index = Random.Range(0, CollisionSFX.Length);
            audioSource.PlayOneShot(CollisionSFX[index]);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("MazeWall"))
        {
            if (audioSource != null)
            {
                WallHittingSFX();
                print("Hitting Walls");
            }
        }
    }
}