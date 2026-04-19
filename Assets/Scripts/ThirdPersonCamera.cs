using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    public float distance = 5f;
    public float height = 2f;
    public float mouseSensitivity = 3f;

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
        Vector3 dir = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        transform.position = target.position + Vector3.up * height + rotation * dir;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}