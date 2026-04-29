using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// Place on the PICKUP waypoint collider trigger.
    /// When the player (on a bike) reaches this zone the parcel is collected.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PickupWaypointTrigger : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("Vehicle"))
            {
                DeliveryMissionManager.Instance?.OnReachedPickup();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.85f, 0f, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Collider col = GetComponent<Collider>();
            if (col is BoxCollider box) Gizmos.DrawCube(box.center, box.size);
            else if (col is SphereCollider s) Gizmos.DrawSphere(s.center, s.radius);

            Gizmos.color = new Color(1f, 0.85f, 0f, 0.9f);
            if (col is BoxCollider box2) Gizmos.DrawWireCube(box2.center, box2.size);
            else if (col is SphereCollider s2) Gizmos.DrawWireSphere(s2.center, s2.radius);
        }
#endif
    }
}