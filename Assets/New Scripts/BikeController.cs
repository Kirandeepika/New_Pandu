using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class BikeController : MonoBehaviour
    {
        [Header("Bike — Driving Settings")]
        [Tooltip("Top speed in m/s")]
        public float MaxSpeed = 20f;

        [Tooltip("Top speed when reversing (bikes barely reverse)")]
        public float MaxReverseSpeed = 3f;

        [Tooltip("Acceleration from rest")]
        public float Acceleration = 6f;

        [Tooltip("Deceleration when braking (pressing opposite direction to travel)")]
        public float BrakeForce = 14f;

        [Tooltip("Deceleration when coasting with no throttle")]
        public float CoastDeceleration = 5f;

        [Tooltip("How fast the bike turns in degrees/sec")]
        public float TurnSpeed = 100f;

        [Header("Lean Settings")]
        [Tooltip("Your existing bike GameObject (child of this BikePivot). ALL of it will lean.")]
        public Transform BikeRoot;

        [Tooltip("Maximum lean angle in degrees on the Z axis")]
        public float MaxLeanAngle = 30f;

        [Tooltip("How smoothly the lean follows input — lower = snappier (try 0.10 - 0.20)")]
        public float LeanSmoothTime = 0.15f;

        [Tooltip("Bike won't reach full lean until moving at least this fast (m/s)")]
        public float MinSpeedForFullLean = 3f;

        [Header("Entry / Exit")]
        [Tooltip("How close the player must be to press E and enter")]
        public float EntryRadius = 3f;

        [Tooltip("Empty GameObject placed beside the bike — player spawns here on exit")]
        public Transform ExitPoint;

        [Tooltip("The player's root GameObject — hidden while riding")]
        public GameObject PlayerModel;

        [Tooltip("The rider/boy model sitting ON the bike — shown while riding, hidden on foot. " +
                 "Place it as a child of BikeRoot at seat position and disable it by default.")]
        public GameObject RiderModel;

        [Tooltip("The ThirdPersonController script on the player")]
        public ThirdPersonController PlayerController;

        [Tooltip("The player's CharacterController component")]
        public CharacterController PlayerCharacterController;

        [Header("Camera")]
        [Tooltip("Drag your ThirdPersonCamera GameObject here")]
        public ThirdPersonCamera CameraController;

        [Tooltip("Empty child GameObject on the bike at seat height for camera to follow")]
        public Transform VehicleCameraTarget;

        [Header("Mobile UI")]
        [Tooltip("Drag your player HUD/joystick Canvas here — it will hide while riding")]
        public GameObject PlayerControllerCanvas;

        // ── Public mobile input — set by MobileVehicleUI every frame ─────
        [HideInInspector] public float MobileDriveInput = 0f;
        [HideInInspector] public float MobileTurnInput = 0f;
        [HideInInspector] public bool UseMobileInput = false;

        // ── Private state ─────────────────────────────────────────────────
        private float _currentSpeed = 0f;
        private float _turnInput = 0f;
        private float _driveInput = 0f;
        private bool _isRiding = false;

        // Lean
        private float _currentLeanAngle = 0f;
        private float _leanVelocity = 0f;

        private Transform _playerTransform;
        private Transform _originalCameraTarget;

#if ENABLE_INPUT_SYSTEM
        private Keyboard _kb;
#endif

        // ─────────────────────────────────────────────────────────────────
        private void Start()
        {
#if ENABLE_INPUT_SYSTEM
            _kb = Keyboard.current;
#endif
            if (PlayerController != null)
                _playerTransform = PlayerController.transform;

            if (BikeRoot == null)
                Debug.LogError("BikeController: BikeRoot is not assigned!");

            if (RiderModel != null)
                RiderModel.SetActive(false);

            if (CameraController != null)
                _originalCameraTarget = CameraController.target;
        }

        // ─────────────────────────────────────────────────────────────────
        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            _kb = Keyboard.current;
            if (_kb == null && !UseMobileInput) return;
#endif
            if (_isRiding)
                HandleRiding();
            else
                CheckForEntry();
        }

        // ── Entry ─────────────────────────────────────────────────────────
        private void CheckForEntry()
        {
            if (_playerTransform == null) return;

            float dist = Vector3.Distance(_playerTransform.position, transform.position);
            if (dist > EntryRadius) return;

#if ENABLE_INPUT_SYSTEM
            if (_kb != null && _kb.eKey.wasPressedThisFrame)
#else
            if (Input.GetKeyDown(KeyCode.E))
#endif
            {
                EnterVehicle();
            }
        }

        public void EnterVehicle()
        {
            _isRiding = true;

            if (PlayerModel != null) PlayerModel.SetActive(false);
            if (RiderModel != null) RiderModel.SetActive(true);

            if (PlayerController != null) PlayerController.enabled = false;
            if (PlayerCharacterController != null) PlayerCharacterController.enabled = false;

            // Hide player HUD/joystick canvas
            if (PlayerControllerCanvas != null) PlayerControllerCanvas.SetActive(false);

            if (_playerTransform != null)
            {
                _playerTransform.SetParent(transform);
                _playerTransform.localPosition = Vector3.zero;
            }

            if (CameraController != null)
            {
                CameraController.target = VehicleCameraTarget != null
                    ? VehicleCameraTarget
                    : transform;
            }
        }

        // ── Exit ──────────────────────────────────────────────────────────
        public void ExitVehicle()
        {
            _isRiding = false;
            _currentSpeed = 0f;
            UseMobileInput = false;
            MobileDriveInput = 0f;
            MobileTurnInput = 0f;

            // Reset lean to upright instantly on exit
            _currentLeanAngle = 0f;
            if (BikeRoot != null)
            {
                Vector3 e = BikeRoot.localEulerAngles;
                BikeRoot.localEulerAngles = new Vector3(e.x, e.y, 0f);
            }

            if (_playerTransform != null)
            {
                _playerTransform.SetParent(null);

                Vector3 exitPos = ExitPoint != null
                    ? ExitPoint.position
                    : transform.position + transform.right * 1.5f;

                if (PlayerCharacterController != null)
                {
                    PlayerCharacterController.enabled = false;
                    _playerTransform.position = exitPos;
                    PlayerCharacterController.enabled = true;
                }
                else
                {
                    _playerTransform.position = exitPos;
                }

                _playerTransform.rotation = transform.rotation;
            }

            if (RiderModel != null) RiderModel.SetActive(false);
            if (PlayerModel != null) PlayerModel.SetActive(true);

            if (PlayerController != null) PlayerController.enabled = true;

            // Show player HUD/joystick canvas again
            if (PlayerControllerCanvas != null) PlayerControllerCanvas.SetActive(true);

            if (CameraController != null && _originalCameraTarget != null)
                CameraController.target = _originalCameraTarget;
        }

        // ── Riding ────────────────────────────────────────────────────────
        private void HandleRiding()
        {
            ReadInput();

#if ENABLE_INPUT_SYSTEM
            if (_kb != null && _kb.eKey.wasPressedThisFrame)
#else
            if (Input.GetKeyDown(KeyCode.E))
#endif
            {
                ExitVehicle();
                return;
            }

            bool movingForward = _currentSpeed > 0.1f;
            bool movingBackward = _currentSpeed < -0.1f;
            bool pressingForward = _driveInput > 0f;
            bool pressingBackward = _driveInput < 0f;

            if ((movingForward && pressingBackward) || (movingBackward && pressingForward))
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, BrakeForce * Time.deltaTime);
            else if (pressingForward)
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, MaxSpeed, Acceleration * Time.deltaTime);
            else if (pressingBackward)
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, -MaxReverseSpeed, Acceleration * Time.deltaTime);
            else
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, CoastDeceleration * Time.deltaTime);

            // Turning
            if (Mathf.Abs(_currentSpeed) > 0.2f)
            {
                float direction = _currentSpeed > 0f ? 1f : -1f;
                float turnAmount = _turnInput * TurnSpeed * direction * Time.deltaTime;
                transform.Rotate(0f, turnAmount, 0f);
            }

            // Move
            transform.position += transform.forward * (_currentSpeed * Time.deltaTime);

            // Lean
            ApplyLean();
        }

        private void ApplyLean()
        {
            if (BikeRoot == null) return;

            float speedFactor = Mathf.Clamp01(Mathf.Abs(_currentSpeed) / MinSpeedForFullLean);
            float targetLean = -_turnInput * MaxLeanAngle * speedFactor;

            _currentLeanAngle = Mathf.SmoothDamp(
                _currentLeanAngle, targetLean,
                ref _leanVelocity, LeanSmoothTime);

            Vector3 euler = BikeRoot.localEulerAngles;
            BikeRoot.localEulerAngles = new Vector3(euler.x, euler.y, _currentLeanAngle);
        }

        // ── Input ─────────────────────────────────────────────────────────
        private void ReadInput()
        {
            if (UseMobileInput)
            {
                // Mobile: read public fields written by MobileVehicleUI every frame
                _driveInput = MobileDriveInput;
                _turnInput = MobileTurnInput;
            }
            else
            {
#if ENABLE_INPUT_SYSTEM
                _driveInput = 0f;
                _turnInput = 0f;
                if (_kb == null) return;
                if (_kb.wKey.isPressed || _kb.upArrowKey.isPressed) _driveInput = 1f;
                if (_kb.sKey.isPressed || _kb.downArrowKey.isPressed) _driveInput = -1f;
                if (_kb.aKey.isPressed || _kb.leftArrowKey.isPressed) _turnInput = -1f;
                if (_kb.dKey.isPressed || _kb.rightArrowKey.isPressed) _turnInput = 1f;
#else
                _driveInput = Input.GetAxis("Vertical");
                _turnInput  = Input.GetAxis("Horizontal");
#endif
            }
        }

        // ── Gizmo ─────────────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
            Gizmos.DrawSphere(transform.position, EntryRadius);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, EntryRadius);
        }
    }
}