using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StarterAssets
{
    /// <summary>
    /// PANDU — Main Menu Manager
    ///
    /// SETUP:
    /// 1. Attach this script to an empty GameObject called "MenuManager"
    /// 2. Drag your 3 buttons into the Inspector slots below
    /// 3. Set GameSceneName to the exact name of your game scene (e.g. "GameScene")
    /// 4. The Continue button auto-disables if no saved game exists
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Buttons")]
        public Button NewGameButton;
        public Button ContinueButton;
        public Button ExitButton;

        [Header("Scene")]
        [Tooltip("Exact name of your gameplay scene as shown in Build Settings")]
        public string GameSceneName = "GameScene";

        [Header("Save Key (must match your save system)")]
        [Tooltip("PlayerPrefs key used to check if a save exists")]
        public string SaveKey = "HasSave";

        private void Start()
        {
            // Wire button clicks
            if (NewGameButton != null)
                NewGameButton.onClick.AddListener(OnNewGame);

            if (ContinueButton != null)
                ContinueButton.onClick.AddListener(OnContinue);

            if (ExitButton != null)
                ExitButton.onClick.AddListener(OnExit);

            // Dim Continue button if no save data exists
            RefreshContinueButton();
        }

        // ── New Game ──────────────────────────────────────────────────────
        private void OnNewGame()
        {
            // Clear old save so the game starts fresh
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();

            LoadGameScene();
        }

        // ── Continue ──────────────────────────────────────────────────────
        private void OnContinue()
        {
            if (!HasSaveData())
            {
                Debug.LogWarning("MainMenuManager: No save data found. Start a new game first.");
                return;
            }

            LoadGameScene();
        }

        // ── Exit ──────────────────────────────────────────────────────────
        private void OnExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private void LoadGameScene()
        {
            if (string.IsNullOrEmpty(GameSceneName))
            {
                Debug.LogError("MainMenuManager: GameSceneName is empty! Set it in the Inspector.");
                return;
            }

            SceneManager.LoadScene(GameSceneName);
        }

        private bool HasSaveData()
        {
            return PlayerPrefs.HasKey(SaveKey);
        }

        private void RefreshContinueButton()
        {
            if (ContinueButton == null) return;

            bool hasSave = HasSaveData();

            // Visually dim the button if no save exists
            var colors = ContinueButton.colors;
            colors.normalColor = hasSave ? Color.white : new Color(1f, 1f, 1f, 0.4f);
            ContinueButton.colors = colors;

            ContinueButton.interactable = hasSave;
        }
    }
}