using UnityEngine;
using Audio;
using UI;

namespace MenuScripts
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject settingsPanel;
        [SerializeField]
        private GameObject pausePanel;
        [SerializeField]
        private GameObject gameOverPanel;

        private GameObject lastPanel;

        public void OpenSettings()
        {
            if (pausePanel != null && pausePanel.activeSelf)
            {
                lastPanel = pausePanel;
                pausePanel.SetActive(false);
            }
            else if (gameOverPanel != null && gameOverPanel.activeSelf)
            {
                lastPanel = gameOverPanel;
                gameOverPanel.SetActive(false);
            }

            settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            if (lastPanel != null)
                lastPanel.SetActive(true);
        }
    }
}