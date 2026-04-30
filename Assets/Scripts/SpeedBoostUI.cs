using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;
using System.Collections;

public class SpeedBoostUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject boostPanel;
    public TMP_InputField typingInput;
    public TextMeshProUGUI targetWordText;
    public Button boostButton;
    public Image timerBarImage;
    public TextMeshProUGUI speedStatusText;

    [Header("Boost Settings")]
    public float boostMultiplier = 5f;
    public float boostDuration = 4f;
    public float cooldownDuration = 10f;

    private ThirdPersonController _player;
    private string _currentWord;
    private bool _isBoosting;
    private bool _isOnCooldown;
    private float _boostTimer;
    private float _cooldownTimer;
    private TextMeshProUGUI _btnLabel;

    private readonly string[] _words =
    {
        "SPEED", "TURBO", "BOOST", "FLASH", "SURGE",
        "BLAZE", "SWIFT", "RAPID", "ZOOM",  "HYPER"
    };

    private void Start()
    {
        _player = FindObjectOfType<ThirdPersonController>();

        if (_player == null)
        {
            Debug.LogError("SpeedBoostUI: ThirdPersonController not found!");
            enabled = false;
            return;
        }

        _btnLabel = boostButton.GetComponentInChildren<TextMeshProUGUI>();

        boostPanel.SetActive(false);

        if (timerBarImage != null)
            timerBarImage.gameObject.SetActive(false);

        // ── Make sure input field is ready ────────────────────────────────
        typingInput.interactable = true;
        typingInput.onValueChanged.AddListener(OnTypingChanged);

        boostButton.onClick.AddListener(OnBoostButtonPressed);

        UpdateSpeedStatusText(1f);
    }

    private void Update()
    {
        if (_isBoosting)
        {
            _boostTimer -= Time.deltaTime;

            if (timerBarImage != null)
                timerBarImage.fillAmount = Mathf.Clamp01(_boostTimer / boostDuration);

            if (_boostTimer <= 0f)
                DeactivateBoost();
        }

        if (_isOnCooldown)
        {
            _cooldownTimer -= Time.deltaTime;

            if (_btnLabel != null)
                _btnLabel.text = $"WAIT {Mathf.CeilToInt(_cooldownTimer)}s";

            if (_cooldownTimer <= 0f)
            {
                _isOnCooldown = false;
                boostButton.interactable = true;

                if (_btnLabel != null)
                    _btnLabel.text = "SPEED BOOST";

                UpdateSpeedStatusText(1f);
            }
        }
    }

    // ── Button Pressed ────────────────────────────────────────────────────
    private void OnBoostButtonPressed()
    {
        if (_isBoosting || _isOnCooldown) return;

        _currentWord = _words[Random.Range(0, _words.Length)];
        targetWordText.text = _currentWord;
        typingInput.text = "";
        typingInput.textComponent.color = Color.white;

        boostPanel.SetActive(true);

        // Wait a frame then focus — fixes mobile keyboard not opening
        StartCoroutine(FocusInputNextFrame());
    }

    private IEnumerator FocusInputNextFrame()
    {
        yield return null; // frame 1
        yield return null; // frame 2 — extra safety for Android

        typingInput.interactable = true;
        typingInput.ActivateInputField();
        typingInput.Select();

        // Force open mobile keyboard
#if UNITY_ANDROID || UNITY_IOS
        TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
#endif
    }

    // ── Typing ────────────────────────────────────────────────────────────
    private void OnTypingChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            typingInput.textComponent.color = Color.white;
            return;
        }

        string upper = value.ToUpper();

        if (_currentWord.StartsWith(upper))
            typingInput.textComponent.color = Color.green;
        else
            typingInput.textComponent.color = Color.red;

        if (upper == _currentWord)
        {
            typingInput.textComponent.color = Color.white;
            boostPanel.SetActive(false);
            typingInput.DeactivateInputField();
            ActivateBoost();
        }
    }

    // ── Boost On ──────────────────────────────────────────────────────────
    private void ActivateBoost()
    {
        _isBoosting = true;
        _boostTimer = boostDuration;

        _player.ApplySpeedBoost(boostMultiplier);

        boostButton.interactable = false;

        if (timerBarImage != null)
        {
            timerBarImage.gameObject.SetActive(true);
            timerBarImage.fillAmount = 1f;
        }

        UpdateSpeedStatusText(boostMultiplier);
        Debug.Log($"[SpeedBoost] ACTIVE {boostMultiplier}x for {boostDuration}s");
    }

    // ── Boost Off ─────────────────────────────────────────────────────────
    private void DeactivateBoost()
    {
        _isBoosting = false;
        _boostTimer = 0f;

        _player.RemoveSpeedBoost();

        if (timerBarImage != null)
            timerBarImage.gameObject.SetActive(false);

        _isOnCooldown = true;
        _cooldownTimer = cooldownDuration;
        boostButton.interactable = false;

        if (_btnLabel != null)
            _btnLabel.text = $"WAIT {Mathf.CeilToInt(_cooldownTimer)}s";

        Debug.Log("[SpeedBoost] ENDED — cooldown started");
    }

    private void UpdateSpeedStatusText(float multiplier)
    {
        if (speedStatusText != null)
            speedStatusText.text = multiplier > 1f
                ? $"SPEED x{multiplier}"
                : "SPEED x1";
    }
}