using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField]
        private GameObject gameWonPanel;
        [SerializeField]
        private GameObject mainMenuPanel;

        private GameObject lastPanel;

        public void OpenSettings()
        {
            lastPanel = FindActivePanel();

            if (lastPanel != null)
                lastPanel.SetActive(false);

            settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            PlayerPrefs.Save();

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            if (lastPanel != null)
                lastPanel.SetActive(true);
        }

        private GameObject FindActivePanel()
        {
            if (pausePanel != null && pausePanel.activeSelf)
                return pausePanel;

            if (gameOverPanel != null && gameOverPanel.activeSelf)
                return gameOverPanel;

            if (gameWonPanel != null && gameWonPanel.activeSelf)
                return gameWonPanel;

            if (mainMenuPanel != null && mainMenuPanel.activeSelf)
                return mainMenuPanel;

            return null;
        }
    }
}