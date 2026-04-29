using UnityEngine;

namespace StarterAssets
{
    public class DirectionIndicatorController : MonoBehaviour
    {
        [Tooltip("How smoothly the arrow rotates")]
        public float RotationSpeed = 8f;

        [Header("Position Offset from Bike")]
        [Tooltip("Adjust these to move the arrow. Y should be negative (below bike)")]
        public Vector3 PositionOffset = new Vector3(0f, -0.05f, 0f);

        [Header("Rotation Offset")]
        [Tooltip("Fine-tune the arrow orientation. Try Y=90 or Y=180 if arrow points wrong way")]
        public Vector3 RotationOffset = new Vector3(90f, 0f, 0f);

        private DeliveryMissionManager _mgr;
        private Transform _bikeTransform;

        private void Start()
        {
            _mgr = DeliveryMissionManager.Instance;
            _bikeTransform = transform.parent;
        }

        private void LateUpdate()
        {
            if (_mgr == null || _bikeTransform == null) return;

            // Always follow bike with offset
            transform.position = _bikeTransform.position + PositionOffset;

            Transform target = GetActiveTarget();
            if (target == null) return;

            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            targetRot *= Quaternion.Euler(RotationOffset); // ← uses inspector value
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