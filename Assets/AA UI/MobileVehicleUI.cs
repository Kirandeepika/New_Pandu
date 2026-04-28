using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace StarterAssets
{
    public class MobileVehicleUI : MonoBehaviour
    {
        [Header("UI Buttons")]
        public Button GetInButton;
        public Button ExitButton;
        public Button TurnLeftButton;
        public Button TurnRightButton;
        public Button AccelButton;
        public Button BrakeButton;

        [Header("UI Panels")]
        public GameObject TurnPanel;
        public GameObject ThrottlePanel;

        [Header("Player")]
        public ThirdPersonController PlayerController;
        [Tooltip("Drag your player HUD/joystick Canvas here — it will hide while driving")]
        public GameObject PlayerControllerCanvas;

        [Header("Vehicles — assign all vehicles in your scene")]
        public SmallCarController[] SmallCars;
        public RegularCarController[] RegularCars;
        public BikeController[] Bikes;

        // ── Runtime state ─────────────────────────────────────────────────
        private enum VehicleType { None, SmallCar, RegularCar, Bike }

        private VehicleType _activeType = VehicleType.None;
        private int _activeIndex = -1;
        private bool _isDriving = false;

        // Button hold states
        private bool _inputLeft, _inputRight, _inputAccel, _inputBrake;

        // ─────────────────────────────────────────────────────────────────
        private void Start()
        {
            if (GetInButton != null) GetInButton.onClick.AddListener(OnGetIn);
            if (ExitButton != null) ExitButton.onClick.AddListener(OnExit);

            AddHoldListener(TurnLeftButton, () => _inputLeft = true, () => _inputLeft = false);
            AddHoldListener(TurnRightButton, () => _inputRight = true, () => _inputRight = false);
            AddHoldListener(AccelButton, () => _inputAccel = true, () => _inputAccel = false);
            AddHoldListener(BrakeButton, () => _inputBrake = true, () => _inputBrake = false);

            ShowDrivingUI(false);
            SetGetInVisible(false);
        }

        private void Update()
        {
            if (_isDriving)
                PushInputToVehicle();
            else
                CheckProximity();
        }

        // ── Proximity ─────────────────────────────────────────────────────
        private void CheckProximity()
        {
            if (PlayerController == null) return;
            Vector3 pos = PlayerController.transform.position;

            float closestDist = float.MaxValue;
            VehicleType closestType = VehicleType.None;
            int closestIndex = -1;

            Check(SmallCars, VehicleType.SmallCar, ref closestDist, ref closestType, ref closestIndex, pos);
            Check(RegularCars, VehicleType.RegularCar, ref closestDist, ref closestType, ref closestIndex, pos);
            Check(Bikes, VehicleType.Bike, ref closestDist, ref closestType, ref closestIndex, pos);

            SetGetInVisible(closestType != VehicleType.None);

            if (closestType != VehicleType.None)
            {
                _activeType = closestType;
                _activeIndex = closestIndex;
            }
        }

        private void Check<T>(T[] arr, VehicleType type,
            ref float closestDist, ref VehicleType closestType, ref int closestIndex,
            Vector3 playerPos) where T : MonoBehaviour
        {
            if (arr == null) return;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == null) continue;
                float radius = GetEntryRadius(arr[i]);
                float d = Vector3.Distance(playerPos, arr[i].transform.position);
                if (d < radius && d < closestDist)
                {
                    closestDist = d;
                    closestType = type;
                    closestIndex = i;
                }
            }
        }

        private float GetEntryRadius(MonoBehaviour mb)
        {
            if (mb is SmallCarController sc) return sc.EntryRadius;
            if (mb is RegularCarController rc) return rc.EntryRadius;
            if (mb is BikeController bk) return bk.EntryRadius;
            return 3f;
        }

        // ── GET IN ────────────────────────────────────────────────────────
        private void OnGetIn()
        {
            if (_activeType == VehicleType.None || _activeIndex < 0) return;

            _isDriving = true;
            SetGetInVisible(false);
            ShowDrivingUI(true);

            // Hide player HUD immediately
            if (PlayerControllerCanvas != null)
                PlayerControllerCanvas.SetActive(false);

            // Enable mobile input mode BEFORE EnterVehicle
            SetMobileInputMode(true);

            // EnterVehicle on the vehicle itself
            GetActiveVehicleGO()?.SendMessage("EnterVehicle", SendMessageOptions.DontRequireReceiver);
        }

        // ── EXIT ──────────────────────────────────────────────────────────
        private void OnExit()
        {
            _inputLeft = _inputRight = _inputAccel = _inputBrake = false;
            PushInputToVehicle();

            SetMobileInputMode(false);

            // ExitVehicle on the vehicle itself
            GetActiveVehicleGO()?.SendMessage("ExitVehicle", SendMessageOptions.DontRequireReceiver);

            _isDriving = false;
            _activeType = VehicleType.None;
            _activeIndex = -1;

            ShowDrivingUI(false);

            // Show player HUD again
            if (PlayerControllerCanvas != null)
                PlayerControllerCanvas.SetActive(true);
        }

        // ── Push input every frame ────────────────────────────────────────
        private void PushInputToVehicle()
        {
            float drive = _inputAccel ? 1f : (_inputBrake ? -1f : 0f);
            float turn = _inputLeft ? -1f : (_inputRight ? 1f : 0f);

            switch (_activeType)
            {
                case VehicleType.SmallCar:
                    if (SmallCars != null && SmallCars[_activeIndex] != null)
                    {
                        SmallCars[_activeIndex].MobileDriveInput = drive;
                        SmallCars[_activeIndex].MobileTurnInput = turn;
                    }
                    break;

                case VehicleType.RegularCar:
                    if (RegularCars != null && RegularCars[_activeIndex] != null)
                    {
                        RegularCars[_activeIndex].MobileDriveInput = drive;
                        RegularCars[_activeIndex].MobileTurnInput = turn;
                    }
                    break;

                case VehicleType.Bike:
                    if (Bikes != null && Bikes[_activeIndex] != null)
                    {
                        Bikes[_activeIndex].MobileDriveInput = drive;
                        Bikes[_activeIndex].MobileTurnInput = turn;
                    }
                    break;
            }
        }

        private void SetMobileInputMode(bool on)
        {
            switch (_activeType)
            {
                case VehicleType.SmallCar:
                    if (SmallCars != null && SmallCars[_activeIndex] != null)
                        SmallCars[_activeIndex].UseMobileInput = on;
                    break;
                case VehicleType.RegularCar:
                    if (RegularCars != null && RegularCars[_activeIndex] != null)
                        RegularCars[_activeIndex].UseMobileInput = on;
                    break;
                case VehicleType.Bike:
                    if (Bikes != null && Bikes[_activeIndex] != null)
                        Bikes[_activeIndex].UseMobileInput = on;
                    break;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private GameObject GetActiveVehicleGO()
        {
            switch (_activeType)
            {
                case VehicleType.SmallCar: return SmallCars?[_activeIndex]?.gameObject;
                case VehicleType.RegularCar: return RegularCars?[_activeIndex]?.gameObject;
                case VehicleType.Bike: return Bikes?[_activeIndex]?.gameObject;
                default: return null;
            }
        }

        private void SetGetInVisible(bool visible)
        {
            if (GetInButton != null) GetInButton.gameObject.SetActive(visible);
        }

        private void ShowDrivingUI(bool show)
        {
            if (ExitButton != null) ExitButton.gameObject.SetActive(show);
            if (TurnPanel != null) TurnPanel.SetActive(show);
            if (ThrottlePanel != null) ThrottlePanel.SetActive(show);
        }

        private void AddHoldListener(Button btn,
            UnityEngine.Events.UnityAction onDown,
            UnityEngine.Events.UnityAction onUp)
        {
            if (btn == null) return;
            var trigger = btn.gameObject.GetComponent<EventTrigger>()
                          ?? btn.gameObject.AddComponent<EventTrigger>();

            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(_ => onDown());
            trigger.triggers.Add(down);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener(_ => onUp());
            trigger.triggers.Add(up);
        }
    }
}