using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuScripts
{
    public class GameInputController : MonoBehaviour
    {
        [Header("Ссылки на меню")]
        [SerializeField]
        private PauseMenu pauseMenu;
        [SerializeField]
        private SettingsMenu settingsMenu;
        [SerializeField]
        private HelpMenu helpMenu;
        [SerializeField]
        private EndGameMenu endGameMenu;

        [Header("Настройки скорости")]
        [SerializeField]
        private float speedUpMultiplier = 3.0f;
        private bool isSpedUp;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                HandleEscape();

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                HandleSpeedToggle();

            if (isSpedUp && Mathf.Approximately(Time.timeScale, 1f) && !IsAnyMenuOpen())
                Time.timeScale = speedUpMultiplier;
        }

        private void HandleEscape()
        {
            if (helpMenu != null && helpMenu.IsOpen)
            {
                helpMenu.CloseHelp();
                return;
            }

            if (settingsMenu != null && settingsMenu.IsOpen)
            {
                settingsMenu.CloseSettings();
                return;
            }

            if (pauseMenu != null && pauseMenu.IsOpen)
            {
                pauseMenu.ResumeGame();
                return;
            }

            if (endGameMenu != null && endGameMenu.IsAnyEndGameOpen)
                return;

            if (pauseMenu != null)
                pauseMenu.OpenPause();
        }

        private void HandleSpeedToggle()
        {
            if (IsAnyMenuOpen())
                return;

            isSpedUp = !isSpedUp;
            Time.timeScale = isSpedUp ? speedUpMultiplier : 1f;

            Debug.Log(isSpedUp ? "Game is sped up to: " + speedUpMultiplier : "Game is slowed down to regular speed");
        }

        private bool IsAnyMenuOpen()
        {
            return helpMenu && helpMenu.IsOpen ||
                   settingsMenu && settingsMenu.IsOpen ||
                   pauseMenu && pauseMenu.IsOpen ||
                   endGameMenu && endGameMenu.IsAnyEndGameOpen;
        }
    }
}