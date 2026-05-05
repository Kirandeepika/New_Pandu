using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class SmallCarController : MonoBehaviour
    {
        [Header("Small Car — Driving Settings")]
        [Tooltip("Top speed in m/s")]
        public float MaxSpeed = 14f;

        [Tooltip("How quickly the car accelerates")]
        public float Acceleration = 8f;

        [Tooltip("How quickly the car brakes / decelerates")]
        public float Deceleration = 12f;

        [Tooltip("How fast the car turns (higher = snappier)")]
        public float TurnSpeed = 120f;

        [Tooltip("Drag applied when no input is given (coast friction)")]
        public float CoastDrag = 3f;

        [Header("Entry / Exit")]
        [Tooltip("How close the player must be to press E and enter")]
        public float EntryRadius = 3f;

        [Tooltip("Where the player is placed when they exit the vehicle")]
        public Transform ExitPoint;

        [Tooltip("The player's root GameObject — hidden while driving")]
        public GameObject PlayerModel;

        [Tooltip("The ThirdPersonController script on the player")]
        public ThirdPersonController PlayerController;

        [Tooltip("The player's CharacterController component")]
        public CharacterController PlayerCharacterController;

        [Tooltip("The sample boy/dummy model sitting in the vehicle — shown while driving, hidden otherwise")]
        public GameObject SeatModel;

        [Header("Camera")]
        [Tooltip("Drag your ThirdPersonCamera GameObject here")]
        public ThirdPersonCamera CameraController;

        [Tooltip("Empty child GameObject on the car at roof/seat height for camera to follow")]
        public Transform VehicleCameraTarget;

        [Header("Mobile UI")]
        [Tooltip("Drag your player HUD/joystick Canvas here — it will hide while driving")]
        public GameObject PlayerControllerCanvas;

        // ── Public mobile input — set by MobileVehicleUI every frame ─────
        [HideInInspector] public float MobileDriveInput = 0f;
        [HideInInspector] public float MobileTurnInput = 0f;
        [HideInInspector] public bool UseMobileInput = false;

        // ── Private state ─────────────────────────────────────────────────
        private float _currentSpeed = 0f;
        private float _turnInput = 0f;
        private float _driveInput = 0f;
        private bool _isDriving = false;

        public AudioSource AudioSource;
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

            if (CameraController != null)
                _originalCameraTarget = CameraController.target;

            if (SeatModel != null) SeatModel.SetActive(false);
        }

        // ─────────────────────────────────────────────────────────────────
        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            _kb = Keyboard.current;
            if (_kb == null && !UseMobileInput) return;
#endif
            if (_isDriving)
                HandleDriving();
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
            _isDriving = true;

            if (PlayerModel != null) PlayerModel.SetActive(false);
            if (SeatModel != null) SeatModel.SetActive(true);

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

            if (AudioSource != null) ;
            {
                AudioSource.Play();
            }

            if (CameraController != null)
                CameraController.vehicleTransform = transform;
        }

        // ── Exit ──────────────────────────────────────────────────────────
        public void ExitVehicle()
        {
            _isDriving = false;
            _currentSpeed = 0f;
            UseMobileInput = false;
            MobileDriveInput = 0f;
            MobileTurnInput = 0f;

            if (SeatModel != null) SeatModel.SetActive(false);
            if (PlayerModel != null) PlayerModel.SetActive(true);

            if (_playerTransform != null)
            {
                _playerTransform.SetParent(null);

                Vector3 exitPos = ExitPoint != null
                    ? ExitPoint.position
                    : transform.position + transform.right * 2f;

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

            if (PlayerController != null) PlayerController.enabled = true;

            // Show player HUD/joystick canvas again
            if (PlayerControllerCanvas != null) PlayerControllerCanvas.SetActive(true);

            if (CameraController != null && _originalCameraTarget != null)
                CameraController.target = _originalCameraTarget;

            AudioSource.Stop();

            if (CameraController != null)
                CameraController.vehicleTransform = null;
        }

        // ── Driving ───────────────────────────────────────────────────────
        private void HandleDriving()
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

            if (_driveInput != 0f)
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed,
                    _driveInput * MaxSpeed,
                    Acceleration * Time.deltaTime);
            }
            else
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f,
                    CoastDrag * Time.deltaTime);
            }

            if (Mathf.Abs(_currentSpeed) > 0.1f)
            {
                float direction = _currentSpeed > 0f ? 1f : -1f;
                float turnAmount = _turnInput * TurnSpeed * direction * Time.deltaTime;
                transform.Rotate(0f, turnAmount, 0f);
            }

            transform.position += transform.forward * (_currentSpeed * Time.deltaTime);
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
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
            Gizmos.DrawSphere(transform.position, EntryRadius);
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, EntryRadius);
        }
    }
}