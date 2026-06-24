using Core;
using Misc;
using UnityEngine;

namespace MenuScripts
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField]
        private FadePanel menuBackground;
        [SerializeField]
        private FadePanel pausePanel;
        [SerializeField]
        private EndGameMenu endGameMenu;

        public bool IsOpen => pausePanel != null && pausePanel.GetComponent<CanvasGroup>().alpha > 0.5f;

        public void OpenPause()
        {
            if (endGameMenu != null && (endGameMenu.endGameSequenceStarted || endGameMenu.IsAnyEndGameOpen))
                return;

            UIBlocker.BlockAll();

            if (pausePanel == null)
                pausePanel = GetComponent<FadePanel>();

            if (menuBackground != null)
                menuBackground.Show();

            pausePanel.Show();

            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            UIBlocker.UnblockAll();

            if (menuBackground != null)
                menuBackground.Hide();

            if (pausePanel != null)
                pausePanel.Hide();

            Time.timeScale = 1f;
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneTransitions.LoadScene("MainMenu");
        }
    }
}