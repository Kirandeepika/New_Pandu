using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StarterAssets
{
    /// <summary>
    /// Central state machine for the delivery mission.
    /// Attach this to any persistent GameObject in the scene (e.g. GameManager).
    /// </summary>
    public class DeliveryMissionManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────
        public static DeliveryMissionManager Instance { get; private set; }

        // ── Mission State ────────────────────────────────────────────────
        public enum MissionState
        {
            Inactive,           // Mission not started
            WaitingForBike,     // Player entered trigger but not on bike
            WaitingToStart,     // Player on bike, "Start Mission" button visible
            GoToPickup,         // Riding to the pickup/start waypoint
            GoToDelivery,       // Parcel collected, riding to delivery point
            Complete            // Mission finished
        }

        public MissionState CurrentState { get; private set; } = MissionState.Inactive;

        // ── Inspector References ─────────────────────────────────────────
        [Header("References")]
        [Tooltip("The BikeController in the scene")]
        public BikeController BikeController;

        [Tooltip("The ThirdPersonController (player)")]
        public ThirdPersonController PlayerController;

        [Header("Waypoints")]
        [Tooltip("Where the player must ride to accept/start the delivery")]
        public Transform PickupWaypoint;

        [Tooltip("Where the player must deliver the parcel")]
        public Transform DeliveryWaypoint;

        [Header("Parcel")]
        [Tooltip("The parcel box GameObject on the bike — hidden until collected")]
        public GameObject ParcelObject;

        [Header("Direction Indicator")]
        [Tooltip("The plane/quad under the bike that arrow is painted on")]
        public Transform DirectionIndicatorPlane;

        [Header("UI")]
        [Tooltip("Canvas Group for the 'Get on a bike!' prompt")]
        public CanvasGroup GetBikePromptGroup;

        [Tooltip("TextMeshPro label inside the get-bike prompt")]
        public TextMeshProUGUI GetBikePromptText;

        [Tooltip("The 'Start Mission' button shown once player is on bike")]
        public GameObject StartMissionButton;

        [Tooltip("Canvas Group for the on-screen mission HUD (objective text)")]
        public CanvasGroup MissionHUDGroup;

        [Tooltip("Objective label shown during the mission")]
        public TextMeshProUGUI ObjectiveText;

        [Tooltip("Canvas Group for the mission-complete panel")]
        public CanvasGroup MissionCompleteGroup;

        [Tooltip("How quickly UI panels fade in/out")]
        public float UIFadeSpeed = 3f;

        // ── Private ──────────────────────────────────────────────────────
        private float _getPromptAlpha = 0f;
        private float _hudAlpha = 0f;
        private float _completeAlpha = 0f;

        // ─────────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // Safe initial state
            if (ParcelObject != null) ParcelObject.SetActive(false);
            if (StartMissionButton != null) StartMissionButton.SetActive(false);
            if (DirectionIndicatorPlane != null) DirectionIndicatorPlane.gameObject.SetActive(false);

            SetCanvasAlpha(GetBikePromptGroup, 0f);
            SetCanvasAlpha(MissionHUDGroup, 0f);
            SetCanvasAlpha(MissionCompleteGroup, 0f);
        }

        private void Update()
        {
            HandleUIFade();

            if (CurrentState == MissionState.GoToPickup || CurrentState == MissionState.GoToDelivery)
                UpdateDirectionIndicator();
        }

        // ── Public API called by trigger zones ───────────────────────────

        /// <summary>Called by MissionTriggerZone when the player enters.</summary>
        public void OnPlayerEnteredMissionZone()
        {
            if (CurrentState != MissionState.Inactive) return;

            bool isRiding = BikeController != null && IsPlayerRiding();

            if (!isRiding)
            {
                SetState(MissionState.WaitingForBike);
            }
            else
            {
                SetState(MissionState.WaitingToStart);
            }
        }

        /// <summary>Called by MissionTriggerZone when player exits (optional cleanup).</summary>
        public void OnPlayerExitedMissionZone()
        {
            if (CurrentState == MissionState.WaitingForBike || CurrentState == MissionState.WaitingToStart)
            {
                SetState(MissionState.Inactive);
                if (StartMissionButton != null) StartMissionButton.SetActive(false);
            }
        }

        /// <summary>Called every frame by MissionTriggerZone while player is inside.</summary>
        public void OnPlayerInsideMissionZone()
        {
            if (CurrentState == MissionState.WaitingForBike && IsPlayerRiding())
                SetState(MissionState.WaitingToStart);
        }

        /// <summary>Hooked to the Start Mission UI button's OnClick event.</summary>
        public void OnStartMissionButtonPressed()
        {
            if (CurrentState != MissionState.WaitingToStart) return;
            if (StartMissionButton != null) StartMissionButton.SetActive(false);
            SetState(MissionState.GoToPickup);
        }

        /// <summary>Called by PickupWaypointTrigger when player arrives.</summary>
        public void OnReachedPickup()
        {
            if (CurrentState != MissionState.GoToPickup) return;
            if (ParcelObject != null) ParcelObject.SetActive(true);
            SetState(MissionState.GoToDelivery);
        }

        /// <summary>Called by DeliveryWaypointTrigger when player delivers.</summary>
        public void OnReachedDelivery()
        {
            if (CurrentState != MissionState.GoToDelivery) return;
            if (ParcelObject != null) ParcelObject.SetActive(false);
            SetState(MissionState.Complete);
        }

        // ── State Machine ─────────────────────────────────────────────────
        private void SetState(MissionState next)
        {
            CurrentState = next;

            // Reset indicator
            if (DirectionIndicatorPlane != null)
                DirectionIndicatorPlane.gameObject.SetActive(
                    next == MissionState.GoToPickup || next == MissionState.GoToDelivery);

            switch (next)
            {
                case MissionState.Inactive:
                    SetCanvasAlpha(GetBikePromptGroup, 0f, instant: true);
                    break;

                case MissionState.WaitingForBike:
                    if (GetBikePromptText != null)
                        GetBikePromptText.text = "You need a bike for this mission!\nFind one and come back.";
                    _getPromptAlpha = 1f; // trigger fade-in
                    break;

                case MissionState.WaitingToStart:
                    SetCanvasAlpha(GetBikePromptGroup, 0f, instant: true);
                    if (StartMissionButton != null) StartMissionButton.SetActive(true);
                    break;

                case MissionState.GoToPickup:
                    if (ObjectiveText != null)
                        ObjectiveText.text = "Ride to the pickup point.";
                    _hudAlpha = 1f;
                    break;

                case MissionState.GoToDelivery:
                    if (ObjectiveText != null)
                        ObjectiveText.text = "Deliver the parcel!";
                    break;

                case MissionState.Complete:
                    if (DirectionIndicatorPlane != null)
                        DirectionIndicatorPlane.gameObject.SetActive(false);
                    _hudAlpha = 0f;
                    _completeAlpha = 1f;
                    Invoke(nameof(FadeOutComplete), 3f);
                    break;
            }
        }

        // ── Direction Indicator ───────────────────────────────────────────
        private void UpdateDirectionIndicator()
        {
            if (DirectionIndicatorPlane == null) return;

            Transform target = CurrentState == MissionState.GoToPickup
                ? PickupWaypoint
                : DeliveryWaypoint;

            if (target == null) return;

            // Parent the indicator to the bike so it moves with it
            Transform bikeTransform = BikeController != null ? BikeController.transform : null;
            if (bikeTransform == null) return;

            Vector3 directionToTarget = target.position - bikeTransform.position;
            directionToTarget.y = 0f;

            if (directionToTarget.sqrMagnitude < 0.01f) return;

            // Rotate only on Y axis to point toward target
            float angle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
            DirectionIndicatorPlane.rotation = Quaternion.Euler(90f, angle, 0f);
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private bool IsPlayerRiding()
        {
            // Relies on BikeController's _isRiding — expose via property or use this workaround:
            // We check if the PlayerController is disabled (it gets disabled when riding)
            return PlayerController != null && !PlayerController.enabled;
        }

        private void HandleUIFade()
        {
            if (GetBikePromptGroup != null)
            {
                float target = (CurrentState == MissionState.WaitingForBike) ? 1f : 0f;
                GetBikePromptGroup.alpha = Mathf.MoveTowards(
                    GetBikePromptGroup.alpha, target, UIFadeSpeed * Time.deltaTime);
            }

            if (MissionHUDGroup != null)
            {
                float target = (CurrentState == MissionState.GoToPickup ||
                                CurrentState == MissionState.GoToDelivery) ? 1f : 0f;
                MissionHUDGroup.alpha = Mathf.MoveTowards(
                    MissionHUDGroup.alpha, target, UIFadeSpeed * Time.deltaTime);
            }

            if (MissionCompleteGroup != null)
            {
                float target = (CurrentState == MissionState.Complete) ? 1f : 0f;
                MissionCompleteGroup.alpha = Mathf.MoveTowards(
                    MissionCompleteGroup.alpha, target, UIFadeSpeed * Time.deltaTime);
            }
        }

        private void FadeOutComplete()
        {
            SetState(MissionState.Inactive);
        }

        private static void SetCanvasAlpha(CanvasGroup group, float alpha, bool instant = false)
        {
            if (group == null) return;
            group.alpha = alpha;
            group.interactable = alpha > 0f;
            group.blocksRaycasts = alpha > 0f;
        }
    }
}