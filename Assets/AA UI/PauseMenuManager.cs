using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StarterAssets
{
    /// <summary>
    /// PANDU — Pause Menu Manager
    ///
    /// CANVAS HIERARCHY:
    ///
    ///  Canvas
    ///  └── HUD (always visible)
    ///      └── PauseButton          ← top-right, Pause.png icon, calls TogglePause()
    ///  └── PausePanel (hidden by default)
    ///      ├── Background           ← Rectangle_8.png (dark frame), stretch fill
    ///      ├── PauseTitle           ← "PAUSE" text or Pause.png label
    ///      ├── CloseButton          ← Multiply.png (X), calls TogglePause()
    ///      ├── ResumeButton         ← Rectangle_9.png bg + Resume.png label
    ///      ├── HomeButton           ← Rectangle_10.png bg + Home.png label
    ///      └── ExitButton           ← Rectangle_13.png bg + Exit.png label
    ///
    /// SETUP:
    /// 1. Attach this script to an empty GameObject "PauseMenuManager"
    /// 2. Assign all references in the Inspector
    /// 3. Set MainMenuSceneName to your main menu scene name (e.g. "MainMenu")
    /// 4. PausePanel starts disabled in the hierarchy — this script shows/hides it
    /// </summary>
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [Tooltip("The pause menu panel root — disabled by default in hierarchy")]
        public GameObject PausePanel;

        [Header("Buttons")]
        public Button PauseButton;
        public Button CloseButton;
        public Button ResumeButton;
        public Button HomeButton;
        public Button ExitButton;

        [Header("Scene")]
        [Tooltip("Name of your main menu scene in Build Settings")]
        public string MainMenuSceneName = "MainMenu";

        private bool _isPaused = false;

        // ─────────────────────────────────────────────────────────────────
        private void Start()
        {
            // Wire buttons
            if (PauseButton != null) PauseButton.onClick.AddListener(TogglePause);
            if (CloseButton != null) CloseButton.onClick.AddListener(TogglePause);
            if (ResumeButton != null) ResumeButton.onClick.AddListener(Resume);
            if (HomeButton != null) HomeButton.onClick.AddListener(GoHome);
            if (ExitButton != null) ExitButton.onClick.AddListener(ExitGame);

            // Make sure pause panel starts hidden
            SetPausePanel(false);
        }

        private void Update()
        {
            // Allow Escape key on PC / keyboard
            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }

        // ── Pause / Resume ────────────────────────────────────────────────
        public void TogglePause()
        {
            if (_isPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;   // freeze game
            SetPausePanel(true);
        }

        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;   // unfreeze game
            SetPausePanel(false);
        }

        // ── Home ──────────────────────────────────────────────────────────
        private void GoHome()
        {
            Time.timeScale = 1f;   // always reset before scene load
            SceneManager.LoadScene(MainMenuSceneName);
        }

        // ── Exit ──────────────────────────────────────────────────────────
        private void ExitGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private void SetPausePanel(bool visible)
        {
            if (PausePanel != null)
                PausePanel.SetActive(visible);
        }

        // Make sure time is restored if this object is destroyed (e.g. scene change)
        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}