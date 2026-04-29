using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StarterAssets
{
    public class DeliveryMissionManager : MonoBehaviour
    {
        public static DeliveryMissionManager Instance { get; private set; }

        public enum MissionState
        {
            Inactive,
            WaitingForBike,
            ShowingStory,
            WaitingToStart,
            GoToPickup,
            GoToDelivery,
            Complete,
            ShowingComplete     // ← new: showing complete image for 10 seconds
        }

        public MissionState CurrentState { get; private set; } = MissionState.Inactive;

        [Header("References")]
        public BikeController BikeController;
        public ThirdPersonController PlayerController;

        [Header("Waypoints")]
        public Transform PickupWaypoint;
        public Transform DeliveryWaypoint;

        [Header("Parcel")]
        public GameObject ParcelObject;

        [Header("Direction Indicator")]
        public Transform DirectionIndicatorPlane;

        [Header("Story Screen")]
        public CanvasGroup StoryPanelGroup;
        public Image StoryImage;
        public float StoryDuration = 5f;

        [Header("Complete Screen")]
        [Tooltip("The panel containing your mission complete image")]
        public CanvasGroup CompletePanelGroup;

        [Tooltip("The Image UI element — set your complete sprite in Inspector")]
        public Image CompleteImage;

        [Tooltip("Seconds to show the complete image before it disappears")]
        public float CompleteDuration = 10f;

        [Header("UI")]
        public CanvasGroup GetBikePromptGroup;
        public TextMeshProUGUI GetBikePromptText;
        public GameObject StartMissionButton;
        public CanvasGroup MissionHUDGroup;
        public TextMeshProUGUI ObjectiveText;
        public CanvasGroup MissionCompleteGroup;
        public float UIFadeSpeed = 3f;

        private float _storyTimer = 0f;
        private float _completeImageTimer = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (ParcelObject != null) ParcelObject.SetActive(false);
            if (StartMissionButton != null) StartMissionButton.SetActive(false);
            if (DirectionIndicatorPlane != null) DirectionIndicatorPlane.gameObject.SetActive(false);

            SetCanvasAlpha(GetBikePromptGroup, 0f);
            SetCanvasAlpha(MissionHUDGroup, 0f);
            SetCanvasAlpha(MissionCompleteGroup, 0f);
            SetCanvasAlpha(StoryPanelGroup, 0f);
            SetCanvasAlpha(CompletePanelGroup, 0f);
        }

        private void Update()
        {
            HandleUIFade();
            HandleStoryTimer();
            HandleCompleteImageTimer();

            if (CurrentState == MissionState.GoToPickup ||
                CurrentState == MissionState.GoToDelivery)
                UpdateDirectionIndicator();
        }

        // ── Story Timer ───────────────────────────────────────────────────
        private void HandleStoryTimer()
        {
            if (CurrentState != MissionState.ShowingStory) return;

            _storyTimer += Time.deltaTime;

            if (_storyTimer >= StoryDuration)
            {
                SetCanvasAlpha(StoryPanelGroup, 0f, instant: true);
                SetState(MissionState.WaitingToStart);
            }
        }

        // ── Complete Image Timer ──────────────────────────────────────────
        private void HandleCompleteImageTimer()
        {
            if (CurrentState != MissionState.ShowingComplete) return;

            _completeImageTimer += Time.deltaTime;

            if (_completeImageTimer >= CompleteDuration)
            {
                // Hide complete image and reset mission
                SetCanvasAlpha(CompletePanelGroup, 0f, instant: true);
                SetState(MissionState.Inactive);
            }
        }

        // ── Public API ────────────────────────────────────────────────────
        public void OnPlayerEnteredMissionZone()
        {
            if (CurrentState != MissionState.Inactive) return;

            bool isRiding = BikeController != null && IsPlayerRiding();

            if (!isRiding)
                SetState(MissionState.WaitingForBike);
            else
                SetState(MissionState.ShowingStory);
        }

        public void OnPlayerExitedMissionZone()
        {
            if (CurrentState == MissionState.WaitingForBike)
                SetState(MissionState.Inactive);
        }

        public void OnPlayerInsideMissionZone()
        {
            if (CurrentState == MissionState.WaitingForBike && IsPlayerRiding())
                SetState(MissionState.ShowingStory);
        }

        public void OnStartMissionButtonPressed()
        {
            if (CurrentState != MissionState.WaitingToStart) return;
            if (StartMissionButton != null) StartMissionButton.SetActive(false);
            SetState(MissionState.GoToPickup);
        }

        public void OnReachedPickup()
        {
            if (CurrentState != MissionState.GoToPickup) return;
            if (ParcelObject != null) ParcelObject.SetActive(true);
            SetState(MissionState.GoToDelivery);
        }

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

            if (DirectionIndicatorPlane != null)
                DirectionIndicatorPlane.gameObject.SetActive(
                    next == MissionState.GoToPickup ||
                    next == MissionState.GoToDelivery);

            switch (next)
            {
                case MissionState.Inactive:
                    SetCanvasAlpha(GetBikePromptGroup, 0f, instant: true);
                    SetCanvasAlpha(StoryPanelGroup, 0f, instant: true);
                    SetCanvasAlpha(CompletePanelGroup, 0f, instant: true);
                    if (StartMissionButton != null) StartMissionButton.SetActive(false);
                    break;

                case MissionState.WaitingForBike:
                    if (GetBikePromptText != null)
                        GetBikePromptText.text = "You need a bike for this mission!\nFind one and come back.";
                    break;

                case MissionState.ShowingStory:
                    SetCanvasAlpha(GetBikePromptGroup, 0f, instant: true);
                    if (StartMissionButton != null) StartMissionButton.SetActive(false);
                    SetCanvasAlpha(StoryPanelGroup, 1f);
                    _storyTimer = 0f;
                    break;

                case MissionState.WaitingToStart:
                    if (StartMissionButton != null) StartMissionButton.SetActive(true);
                    break;

                case MissionState.GoToPickup:
                    if (ObjectiveText != null)
                        ObjectiveText.text = "📦 Ride to the pickup point.";
                    break;

                case MissionState.GoToDelivery:
                    if (ObjectiveText != null)
                        ObjectiveText.text = "🚚 Deliver the parcel!";
                    break;

                case MissionState.Complete:
                    // Hide HUD, show complete image
                    if (DirectionIndicatorPlane != null)
                        DirectionIndicatorPlane.gameObject.SetActive(false);
                    SetCanvasAlpha(MissionHUDGroup, 0f, instant: true);
                    SetState(MissionState.ShowingComplete);
                    break;

                case MissionState.ShowingComplete:
                    // Show complete image for 10 seconds
                    SetCanvasAlpha(CompletePanelGroup, 1f);
                    _completeImageTimer = 0f;
                    break;
            }
        }

        // ── Direction Indicator ───────────────────────────────────────────
        private void UpdateDirectionIndicator()
        {
            if (DirectionIndicatorPlane == null) return;

            Transform target = CurrentState == MissionState.GoToPickup
                ? PickupWaypoint : DeliveryWaypoint;

            if (target == null) return;

            Transform bikeTransform = BikeController != null ? BikeController.transform : null;
            if (bikeTransform == null) return;

            Vector3 dir = target.position - bikeTransform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;

            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            DirectionIndicatorPlane.rotation = Quaternion.Euler(90f, angle, 0f);
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private bool IsPlayerRiding()
        {
            return PlayerController != null && !PlayerController.enabled;
        }

        private void HandleUIFade()
        {
            if (GetBikePromptGroup != null)
            {
                float target = CurrentState == MissionState.WaitingForBike ? 1f : 0f;
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
                float target = CurrentState == MissionState.Complete ? 1f : 0f;
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