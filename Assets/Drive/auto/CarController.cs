using UnityEngine;

public class CarController : MonoBehaviour
{
    public float maxSpeed = 12f;
    public float acceleration = 5f;
    public float reverseSpeed = 6f;
    public float turnSpeed = 70f;
    public float brakePower = 10f;

    private float currentSpeed = 0f;

    void Update()
    {
        float move = 0;

        // Forward / Reverse input
        if (Input.GetKey(KeyCode.W))
            move = 1;
        else if (Input.GetKey(KeyCode.S))
            move = -1;

        // Apply forward or reverse speed
        float targetSpeed = 0;

        if (move > 0)
            targetSpeed = maxSpeed;
        else if (move < 0)
            targetSpeed = -reverseSpeed;

        // Smooth acceleration
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        // Brake
        if (Input.GetKey(KeyCode.Space))
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, brakePower * Time.deltaTime);
        }

        // Move car
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        // Turning (only when moving)
        float turn = 0;
        if (Input.GetKey(KeyCode.A)) turn = -1;
        if (Input.GetKey(KeyCode.D)) turn = 1;

        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            // Reduce turning when reversing (realistic feel)
            float direction = currentSpeed > 0 ? 1 : -1;
            transform.Rotate(Vector3.up * turn * turnSpeed * direction * Time.deltaTime);
        }
    }
}