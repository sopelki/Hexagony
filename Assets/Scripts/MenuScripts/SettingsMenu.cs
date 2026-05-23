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
            if (pausePanel != null && pausePanel.activeSelf)
                lastPanel = pausePanel;

            else if (gameOverPanel != null && gameOverPanel.activeSelf)
                lastPanel = gameOverPanel;

            else if (gameWonPanel != null && gameWonPanel.activeSelf)
                lastPanel = gameWonPanel;

            else if (mainMenuPanel != null && mainMenuPanel.activeSelf)
                lastPanel = mainMenuPanel;

            if (lastPanel != null)
            {
                lastPanel.GetComponent<Canvas>().enabled = false;
                lastPanel.GetComponent<GraphicRaycaster>().enabled = false;
            }

            settingsPanel.GetComponent<Canvas>().enabled = true;
            settingsPanel.GetComponent<GraphicRaycaster>().enabled = true;
        }

        public void CloseSettings()
        {
            PlayerPrefs.Save();
            Debug.Log("Settings Saved to Disk");

            if (settingsPanel != null)
            {
                settingsPanel.GetComponent<Canvas>().enabled = false;
                settingsPanel.GetComponent<GraphicRaycaster>().enabled = false;
            }

            if (lastPanel != null)
            {
                lastPanel.GetComponent<Canvas>().enabled = true;
                lastPanel.GetComponent<GraphicRaycaster>().enabled = true;
            }
        }
    }
}