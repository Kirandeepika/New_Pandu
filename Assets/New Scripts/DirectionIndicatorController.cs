using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// Attach this to the arrow plane/quad that sits under the bike.
    /// It keeps the plane flat on the ground and rotates it to face the
    /// active mission waypoint. The DeliveryMissionManager enables/disables
    /// this GameObject automatically — no extra setup needed.
    ///
    /// Setup:
    ///   1. Create a Quad (or Plane) child of the bike root, just below wheel level.
    ///   2. Give it an arrow material/texture pointing in local +Z (forward).
    ///   3. Attach this script.
    ///   4. Drag this Transform into DeliveryMissionManager.DirectionIndicatorPlane.
    /// </summary>
    public class DirectionIndicatorController : MonoBehaviour
    {
        [Tooltip("How smoothly the arrow rotates — lower = snappier (try 5-10)")]
        public float RotationSpeed = 8f;

        [Tooltip("Fixed height offset below the bike centre (should be just below wheels)")]
        public float HeightOffset = -0.05f;

        private DeliveryMissionManager _mgr;
        private Transform _bikeTransform;

        private void Start()
        {
            _mgr = DeliveryMissionManager.Instance;

            // Parent is expected to be the bike root
            _bikeTransform = transform.parent;
        }

        private void LateUpdate()
        {
            if (_mgr == null) return;

            Transform target = GetActiveTarget();
            if (target == null || _bikeTransform == null) return;

            // Keep the plane flat and at wheel level
            Vector3 worldPos = _bikeTransform.position + Vector3.up * HeightOffset;
            transform.position = worldPos;

            // Direction on the XZ plane only
            Vector3 dir = target.position - worldPos;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.001f) return;

            // Smooth rotation — the plane is horizontal so we spin around Y
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            // Tilt 90° so the textured face points up (Quad default normal = +Z → rotate X by 90)
            targetRot *= Quaternion.Euler(90f, 0f, 0f);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                RotationSpeed * Time.deltaTime);
        }

        private Transform GetActiveTarget()
        {
            if (_mgr == null) return null;

            return _mgr.CurrentState == DeliveryMissionManager.MissionState.GoToPickup
                ? _mgr.PickupWaypoint
                : _mgr.CurrentState == DeliveryMissionManager.MissionState.GoToDelivery
                    ? _mgr.DeliveryWaypoint
                    : null;
        }
    }
}