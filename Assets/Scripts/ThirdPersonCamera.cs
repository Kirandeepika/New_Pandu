using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    [Header("Camera Distance")]
    public float distance = 5f;
    public float height = 2f;

    [Header("Mouse Control")]
    public float mouseSensitivity = 3f;

    [Header("Position Offset")]
    public Vector3 offset = new Vector3(0, 1.5f, 0); // 🔥 NEW

    float currentX = 0f;
    float currentY = 20f;

    void Update()
    {
        currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentY = Mathf.Clamp(currentY, 10f, 50f);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 dir = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // 🔥 Apply offset
        Vector3 targetPosition = target.position + offset;

        transform.position = targetPosition + rotation * dir;

        transform.LookAt(targetPosition);
    }
}