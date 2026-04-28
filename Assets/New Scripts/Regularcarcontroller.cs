using UnityEngine;
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
        [Tooltip("Drag your ThirdPersonCamera GameObject here")]
        public ThirdPersonCamera CameraController;

        [Tooltip("The camera will follow this point on the car (create an empty child GameObject on your car roof/center)")]
        public Transform VehicleCameraTarget;

        private float _currentSpeed = 0f;
        private float _turnInput = 0f;
        private float _driveInput = 0f;
        private bool _isDriving = false;
        private Transform _playerTransform;
        private Transform _originalCameraTarget; // stores player target to restore on exit

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

            // Store the original camera target (the player)
            if (CameraController != null)
                _originalCameraTarget = CameraController.target;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            _kb = Keyboard.current;
            if (_kb == null) return;
#endif
            if (_isDriving)
                HandleDriving();
            else
                CheckForEntry();
        }

        private void CheckForEntry()
        {
            if (_playerTransform == null) return;
            float dist = Vector3.Distance(_playerTransform.position, transform.position);
            if (dist > EntryRadius) return;

#if ENABLE_INPUT_SYSTEM
            if (_kb.eKey.wasPressedThisFrame)
#else
            if (Input.GetKeyDown(KeyCode.E))
#endif
            {
                EnterVehicle();
            }
        }

        private void EnterVehicle()
        {
            _isDriving = true;

            if (PlayerModel != null) PlayerModel.SetActive(false);
            if (PlayerController != null) PlayerController.enabled = false;
            if (PlayerCharacterController != null) PlayerCharacterController.enabled = false;

            if (_playerTransform != null)
            {
                _playerTransform.SetParent(transform);
                _playerTransform.localPosition = Vector3.zero;
            }

            // ── Switch camera target to the vehicle ──
            if (CameraController != null)
            {
                // Use dedicated VehicleCameraTarget if assigned, otherwise follow car root
                CameraController.target = VehicleCameraTarget != null
                    ? VehicleCameraTarget
                    : transform;
            }
        }

        private void ExitVehicle()
        {
            _isDriving = false;
            _currentSpeed = 0f;

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

            // ── Restore camera target back to the player ──
            if (CameraController != null && _originalCameraTarget != null)
                CameraController.target = _originalCameraTarget;
        }

        private void HandleDriving()
        {
            ReadInput();

#if ENABLE_INPUT_SYSTEM
            if (_kb.eKey.wasPressedThisFrame)
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
#if ENABLE_INPUT_SYSTEM
            _driveInput = 0f;
            _turnInput = 0f;
            if (_kb.wKey.isPressed || _kb.upArrowKey.isPressed) _driveInput = 1f;
            if (_kb.sKey.isPressed || _kb.downArrowKey.isPressed) _driveInput = -1f;
            if (_kb.aKey.isPressed || _kb.leftArrowKey.isPressed) _turnInput = -1f;
            if (_kb.dKey.isPressed || _kb.rightArrowKey.isPressed) _turnInput = 1f;
#else
            _driveInput = Input.GetAxis("Vertical");
            _turnInput  = Input.GetAxis("Horizontal");
#endif
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