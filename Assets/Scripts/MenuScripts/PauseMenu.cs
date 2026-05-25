using Core;
using UnityEngine;
using Misc;

namespace MenuScripts
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField]
        private FadePanel menuBackground;
        [SerializeField]
        private FadePanel pausePanel;

        public void OpenPause()
        {
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