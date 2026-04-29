using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StarterAssets
{
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [Tooltip("The pause menu panel root — disabled by default in hierarchy")]
        public GameObject PausePanel;

        [Header("HUD / Player Controller Canvas")]
        [Tooltip("Assign the player controller canvas or HUD root to hide it while paused")]
        public GameObject PlayerControllerCanvas;

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
            if (PauseButton != null) PauseButton.onClick.AddListener(TogglePause);
            if (CloseButton != null) CloseButton.onClick.AddListener(TogglePause);
            if (ResumeButton != null) ResumeButton.onClick.AddListener(Resume);
            if (HomeButton != null) HomeButton.onClick.AddListener(GoHome);
            if (ExitButton != null) ExitButton.onClick.AddListener(ExitGame);

            SetPausePanel(false);
        }

        private void Update()
        {
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
            Time.timeScale = 0f;
            SetPausePanel(true);
            SetPlayerControllerCanvas(false);  // ← hide player UI
        }

        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            SetPausePanel(false);
            SetPlayerControllerCanvas(true);   // ← show player UI
        }

        // ── Home ──────────────────────────────────────────────────────────
        private void GoHome()
        {
            Time.timeScale = 1f;
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

        private void SetPlayerControllerCanvas(bool visible)
        {
            if (PlayerControllerCanvas != null)
                PlayerControllerCanvas.SetActive(visible);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}