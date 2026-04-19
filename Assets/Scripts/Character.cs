using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterController characterController;

    public float speed = 5.0f;
    public float sprintMultiplier = 2f; // 2x speed
    public float gravity = -9.81f;

    private Vector3 velocity;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Ground Check
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Check Sprint (Shift key)
        float currentSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= sprintMultiplier; // 2x speed
        }

        // Movement
        Vector3 move = transform.right * x + transform.forward * z;

        if (move.magnitude > 1f)
            move.Normalize();

        characterController.Move(move * currentSpeed * Time.deltaTime);

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}