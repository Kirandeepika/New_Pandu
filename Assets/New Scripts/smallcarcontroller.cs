using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class SmallCarController : MonoBehaviour
    {
        [Header("Small Car — Driving Settings")]
        public float MaxSpeed = 14f;
        public float Acceleration = 8f;
        public float Deceleration = 12f;
        public float TurnSpeed = 120f;
        public float CoastDrag = 3f;

        [Header("Entry / Exit")]
        public float EntryRadius = 3f;
        public Transform ExitPoint;
        public GameObject PlayerModel;
        public ThirdPersonController PlayerController;
        public CharacterController PlayerCharacterController;
        public GameObject SeatModel;

        [Header("Camera")]
        public ThirdPersonCamera CameraController;
        public Transform VehicleCameraTarget;

        [Header("Mobile UI")]
        public GameObject PlayerControllerCanvas;

        [HideInInspector] public float MobileDriveInput = 0f;
        [HideInInspector] public float MobileTurnInput = 0f;
        [HideInInspector] public bool UseMobileInput = false;

        private float _currentSpeed = 0f;
        private float _turnInput = 0f;
        private float _driveInput = 0f;
        private bool _isDriving = false;

        public AudioSource AudioSource;
        private Transform _playerTransform;
        private Transform _originalCameraTarget;

        [Header("Minimap")]
        public GameObject MiniMap;
        public GameObject Logo;

        // 🔥 AUTO RESET (ADDED)
        [Header("Auto Reset")]
        public float resetTime = 2f;
        private float _flipTimer = 0f;

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

            if (SeatModel != null) SeatModel.SetActive(false);

            if (Logo != null) Logo.SetActive(false);
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            _kb = Keyboard.current;
            if (_kb == null && !UseMobileInput) return;
#endif
            if (_isDriving)
            {
                HandleDriving();
                AutoResetCheck(); // 🔥 ADDED
            }
            else
            {
                CheckForEntry();
            }
        }

        // 🔥 AUTO RESET CHECK
        void AutoResetCheck()
        {
            // If car is tilted or flipped
            if (transform.up.y < 0.3f)
            {
                _flipTimer += Time.deltaTime;

                if (_flipTimer >= resetTime)
                {
                    ResetCar();
                }
            }
            else
            {
                _flipTimer = 0f;
            }
        }

        // 🔥 RESET FUNCTION
        void ResetCar()
        {
            _flipTimer = 0f;

            _currentSpeed = 0f;

            // Lift slightly
            transform.position += Vector3.up * 1.5f;

            // Reset rotation upright
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

            // If Rigidbody exists (important safety)
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

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

                CameraController.vehicleTransform = transform;
            }

            if (AudioSource != null)
                AudioSource.Play();

            if (Logo != null) Logo.SetActive(true);
            if (MiniMap != null) MiniMap.SetActive(true);
        }

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

            if (PlayerControllerCanvas != null) PlayerControllerCanvas.SetActive(true);

            if (CameraController != null && _originalCameraTarget != null)
            {
                CameraController.target = _originalCameraTarget;
                CameraController.vehicleTransform = null;
            }

            if (AudioSource != null)
                AudioSource.Stop();

            if (Logo != null) Logo.SetActive(false);
            if (MiniMap != null) MiniMap.SetActive(false);
        }

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

        private void ReadInput()
        {
            if (UseMobileInput)
            {
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
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
            Gizmos.DrawSphere(transform.position, EntryRadius);
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, EntryRadius);
        }
    }
}