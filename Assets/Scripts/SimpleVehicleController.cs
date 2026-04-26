using UnityEngine;

[RequireComponent(typeof(Rigidbody))] // Automatically adds a Rigidbody if you don't have one
public class SimpleVehicleController : MonoBehaviour
{
    [Header("Vehicle Settings")]
    public float moveSpeed = 15f;
    public float turnSpeed = 100f;

    [Tooltip("Lowers the center of mass to prevent the vehicle from flipping over easily")]
    public Vector3 centerOfMassOffset = new Vector3(0, -0.5f, 0);

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Lower the center of mass so the bike/auto doesn't tip over on tight turns
        rb.centerOfMass = centerOfMassOffset;
    }

    void Update()
    {
        // Get WASD / Arrow Key input
        // Note: Update only runs when the script is enabled (which our Interaction script handles!)
        moveInput = Input.GetAxis("Vertical");   // W / S keys
        turnInput = Input.GetAxis("Horizontal"); // A / D keys
    }

    void FixedUpdate()
    {
        Drive();
        Steer();
    }

    private void Drive()
    {
        // Calculate forward/backward movement
        Vector3 targetVelocity = transform.forward * moveInput * moveSpeed;

        // Apply the movement, but keep the current Y velocity so gravity still pulls it down
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    private void Steer()
    {
        // Only allow steering if the vehicle is actually moving forward or backward
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            // If reversing, flip the steering direction so it feels natural
            float turnMultiplier = moveInput > 0 ? 1 : -1;

            // Calculate how much to turn this frame
            float turnAmount = turnInput * turnSpeed * turnMultiplier * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);

            // Apply the rotation
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }
}