using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// Place this on a GameObject with a Trigger Collider.
    /// It talks to DeliveryMissionManager to start the mission flow.
    /// Tag the player GameObject as "Player".
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MissionTriggerZone : MonoBehaviour
    {
        private bool _playerInside = false;

        private void Awake()
        {
            // Ensure the collider is a trigger
            Collider col = GetComponent<Collider>();
            if (!col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning($"MissionTriggerZone on '{name}': Collider was not a trigger — fixed automatically.");
            }
        }

        private void Update()
        {
            // Poll every frame while inside so the manager can detect the
            // player hopping on a bike while standing in the zone.
            if (_playerInside && DeliveryMissionManager.Instance != null)
                DeliveryMissionManager.Instance.OnPlayerInsideMissionZone();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = true;
            DeliveryMissionManager.Instance?.OnPlayerEnteredMissionZone();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = false;
            DeliveryMissionManager.Instance?.OnPlayerExitedMissionZone();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0.4f, 0.18f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Collider col = GetComponent<Collider>();
            if (col is BoxCollider box)
                Gizmos.DrawCube(box.center, box.size);
            else if (col is SphereCollider sphere)
                Gizmos.DrawSphere(sphere.center, sphere.radius);

            Gizmos.color = new Color(0f, 1f, 0.4f, 0.8f);
            if (col is BoxCollider box2)
                Gizmos.DrawWireCube(box2.center, box2.size);
            else if (col is SphereCollider sphere2)
                Gizmos.DrawWireSphere(sphere2.center, sphere2.radius);
        }
#endif
    }
}