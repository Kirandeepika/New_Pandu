using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class RegularCarController : MonoBehaviour
    {
        [Header("Regular Car — Driving Settings")]
        public float MaxSpeed = 25f;
        public float MaxReverseSpeed = 8f;
        public float Acceleration = 5f;
        public float BrakeForce = 18f;
        public float CoastDeceleration = 4f;
        public float TurnSpeedLow = 90f;
        public float TurnSpeedHigh = 40f;

        [Header("Entry / Exit")]
        public float EntryRadius = 3.5f;
        public Transform ExitPoint;
        public GameObject PlayerModel;
        public ThirdPersonController PlayerController;
        public CharacterController PlayerCharacterController;

        [Header("Camera")]
        public ThirdPersonCamera CameraController;
        public Transform VehicleCameraTarget;

        [Header("Mobile UI")]
        [Tooltip("Drag your player HUD/joystick Canvas here — it will hide while driving")]
        public GameObject PlayerControllerCanvas;

        // ── Public mobile input — set by MobileVehicleUI every frame ─────
        // MobileVehicleUI writes directly to these instead of using Reflection
        [HideInInspector] public float MobileDriveInput = 0f;
        [HideInInspector] public float MobileTurnInput = 0f;
        [HideInInspector] public bool UseMobileInput = false;

        // ── Private state ─────────────────────────────────────────────────
        private float _currentSpeed = 0f;
        private float _turnInput = 0f;
        private float _driveInput = 0f;
        private bool _isDriving = false;
        private Transform _playerTransform;
        private Transform _originalCameraTarget;

#if ENABLE_INPUT_SYSTEM
        private Keyboard _kb;
#endif

        private void Start()
        {
#if ENABLE_INPUT_SYSTEM
            _kb = Keyboard.current;
#endif
            if (PlayerController != null)
                _playerTransform = PlayerController.transform;

            if (CameraController != null)
                _originalCameraTarget = CameraController.target;
        }

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
            if (PlayerController != null) PlayerController.enabled = false;
            if (PlayerCharacterController != null) PlayerCharacterController.enabled = false;

            // ── Hide player HUD/joystick canvas ──
            if (PlayerControllerCanvas != null) PlayerControllerCanvas.SetActive(false);

            if (_playerTransform != null)
            {
                _playerTransform.SetParent(transform);
                _playerTransform.localPosition = Vector3.zero;
            }

            if (CameraController != null)
                CameraController.target = VehicleCameraTarget != null ? VehicleCameraTarget : transform;
        }

        // ── Exit ──────────────────────────────────────────────────────────
        public void ExitVehicle()
        {
            _isDriving = false;
            _currentSpeed = 0f;
            UseMobileInput = false;
            MobileDriveInput = 0f;
            MobileTurnInput = 0f;

            if (_playerTransform != null)
            {
                _playerTransform.SetParent(null);

                Vector3 exitPos = ExitPoint != null
                    ? ExitPoint.position
                    : transform.position + transform.right * 2.5f;

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

            if (PlayerModel != null) PlayerModel.SetActive(true);
            if (PlayerController != null) PlayerController.enabled = true;

            // ── Show player HUD/joystick canvas again ──
            if (PlayerControllerCanvas != null) PlayerControllerCanvas.SetActive(true);

            if (CameraController != null && _originalCameraTarget != null)
                CameraController.target = _originalCameraTarget;
        }

        // ── Driving ───────────────────────────────────────────────────────
        private void HandleDriving()
        {
            ReadInput();

            // Keyboard exit (PC fallback)
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

            float speedRatio = Mathf.Abs(_currentSpeed) / MaxSpeed;
            float adaptiveTurn = Mathf.Lerp(TurnSpeedLow, TurnSpeedHigh, speedRatio);

            if (Mathf.Abs(_currentSpeed) > 0.3f)
            {
                float direction = _currentSpeed > 0f ? 1f : -1f;
                float turnAmount = _turnInput * adaptiveTurn * direction * Time.deltaTime;
                transform.Rotate(0f, turnAmount, 0f);
            }

            transform.position += transform.forward * (_currentSpeed * Time.deltaTime);
        }

        private void ReadInput()
        {
            if (UseMobileInput)
            {
                // ── Mobile: read public fields written by MobileVehicleUI ──
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.25f);
            Gizmos.DrawSphere(transform.position, EntryRadius);
            Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, EntryRadius);
        }
    }
}