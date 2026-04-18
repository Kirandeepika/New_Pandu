using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterController characterController;
    public float speed = 5.0f;
    public float gravity = -9.81f;

    private Vector3 velocity; // Stores our vertical movement

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Ground Check: Reset velocity when touching the floor
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Horizontal Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        characterController.Move(move * speed * Time.deltaTime);

        // Apply Gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}