using UnityEngine;
using UnityEngine.EventSystems;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;
    public float height = 2f;
    public float mouseSensitivity = 3f;
    public float touchSensitivity = 0.1f;
    public Vector3 offset = new Vector3(0, 1.8f, 0);

    [Header("Rotation Limits")]
    public float minPitch = -20f;
    public float maxPitch = 45f;

    private float yaw = 0f;
    private float pitch = 0f;

    private int _cameraTouchId = -1;

    void LateUpdate()
    {
        if (target == null || !target.gameObject.activeInHierarchy) return;

        HandleInput();

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        Vector3 dir = new Vector3(0, 0, -distance);
        Vector3 targetPos = target.position + offset + (Vector3.up * height);

        transform.position = targetPos + rot * dir;
        transform.LookAt(targetPos);
    }

    private void HandleInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // ── PC: Mouse input ───────────────────────────────────────────────
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

#else
        // ── Mobile: Touch input (RIGHT half of screen only) ───────────────
        float screenMidX = Screen.width * 0.5f;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                // ✅ Only allow touches that START on the right half
                if (touch.position.x < screenMidX) continue;

                // ✅ Skip touches over UI elements (buttons, joystick, etc.)
                if (IsTouchOverUI(touch.fingerId)) continue;

                // Claim this finger for camera control
                if (_cameraTouchId == -1)
                    _cameraTouchId = touch.fingerId;
            }

            // Only process the finger we claimed
            if (touch.fingerId != _cameraTouchId) continue;

            if (touch.phase == TouchPhase.Moved)
            {
                yaw   += touch.deltaPosition.x * touchSensitivity;
                pitch -= touch.deltaPosition.y * touchSensitivity;
                pitch  = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            // Release finger when lifted or cancelled
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                _cameraTouchId = -1;
        }
#endif
    }

    private bool IsTouchOverUI(int fingerId)
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject(fingerId);
    }
}