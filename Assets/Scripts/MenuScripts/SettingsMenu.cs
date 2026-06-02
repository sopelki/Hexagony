using UnityEngine;

namespace MenuScripts
{
    public class SettingsMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private FadePanel menuBackground;

        [Header("Panels")]
        [SerializeField]
        private FadePanel settingsPanel;
        [SerializeField]
        private FadePanel pausePanel;
        [SerializeField]
        private FadePanel gameOverPanel;
        [SerializeField]
        private FadePanel gameWonPanel;
        [SerializeField]
        private FadePanel mainMenuPanel;

        private FadePanel lastPanel;

        public bool IsOpen => settingsPanel && settingsPanel.GetComponent<CanvasGroup>().alpha > 0.5f;

        public void OpenSettings()
        {
            lastPanel = FindActivePanel();

            if (lastPanel)
                lastPanel.Hide(lastPanel.FadeDuration);

            if (settingsPanel)
                settingsPanel.Show();
        }

        public void CloseSettings()
        {
            PlayerPrefs.Save();

            if (settingsPanel)
                settingsPanel.Hide();

            if (lastPanel)
            {
                if (menuBackground)
                {
                    var bgCanvas = menuBackground.GetComponent<CanvasGroup>();

                    if (bgCanvas && bgCanvas.alpha <= 0)
                        menuBackground.Show(lastPanel.FadeDuration);
                }

                lastPanel.Show(lastPanel.FadeDuration);
            }
        }

        private FadePanel FindActivePanel()
        {
            if (gameOverPanel && gameOverPanel.GetComponent<CanvasGroup>().alpha > 0.1f)
                return gameOverPanel;

            if (gameWonPanel && gameWonPanel.GetComponent<CanvasGroup>().alpha > 0.1f)
                return gameWonPanel;

            if (pausePanel && pausePanel.GetComponent<CanvasGroup>().alpha > 0.1f)
                return pausePanel;

            if (mainMenuPanel && mainMenuPanel.GetComponent<CanvasGroup>().alpha > 0.1f)
                return mainMenuPanel;

            return null;
        }
    }
}