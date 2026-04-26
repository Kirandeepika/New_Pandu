using UnityEngine;

public class TestMove : MonoBehaviour
{
    public float maxSpeed = 6f;
    public float acceleration = 4f;
    public float turnSpeed = 100f;
    public float brakePower = 8f;

    [Header("Lean Settings")]
    public float maxLeanAngle = 20f;
    public float leanSpeed = 5f;

    private float currentSpeed = 0f;
    private float currentLean = 0f;

    void OnEnable()
    {
        // Reset speed and lean when scooty is activated
        currentSpeed = 0f;
        currentLean = 0f;
    }

    void Update()
    {
        float move = 0;

        if (Input.GetKey(KeyCode.W))
            move = 1;
        else if (Input.GetKey(KeyCode.S))
            move = -1;

        currentSpeed = Mathf.Lerp(currentSpeed, move * maxSpeed, acceleration * Time.deltaTime);

        if (Input.GetKey(KeyCode.Space))
            currentSpeed = Mathf.Lerp(currentSpeed, 0, brakePower * Time.deltaTime);

        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        float turn = 0;
        if (Input.GetKey(KeyCode.A)) turn = -1;
        if (Input.GetKey(KeyCode.D)) turn = 1;

        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            transform.Rotate(Vector3.up * turn * turnSpeed * Time.deltaTime);

            float targetLean = -turn * maxLeanAngle;
            currentLean = Mathf.Lerp(currentLean, targetLean, leanSpeed * Time.deltaTime);
        }
        else
        {
            currentLean = Mathf.Lerp(currentLean, 0, leanSpeed * Time.deltaTime);
        }

        Vector3 currentRotation = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentLean);
    }
}