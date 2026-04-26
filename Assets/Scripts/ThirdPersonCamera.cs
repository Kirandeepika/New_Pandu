using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;
    public float height = 2f;
    public float mouseSensitivity = 3f;
    public Vector3 offset = new Vector3(0, 1.8f, 0);

    [Header("Rotation Limits")]
    public float minPitch = -20f; // How far you can look up
    public float maxPitch = 45f;  // How far you can look down

    private float yaw = 0f;
    private float pitch = 0f; // New variable for vertical rotation

    void LateUpdate()
    {
        if (target == null || !target.gameObject.activeInHierarchy) return;

        // 1. Get Mouse Inputs
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity; // Negative because Y is usually inverted

        // 2. Clamp the pitch so the camera doesn't flip over
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 3. Calculate Rotation and Position
        // Combined rotation for both horizontal and vertical
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);

        // Direction vector from the target
        Vector3 dir = new Vector3(0, 0, -distance);

        // Final Position: Target + Offset + (Rotation applied to direction)
        // We add the height inside the offset logic for cleaner math
        Vector3 targetPos = target.position + offset + (Vector3.up * height);

        transform.position = targetPos + rot * dir;

        // 4. Look at the target
        transform.LookAt(targetPos);
    }
}