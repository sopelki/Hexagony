using UnityEngine;
using UnityEngine.SceneManagement;
using Misc;

namespace MenuScripts
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;

        public void OpenPause()
        {
            UIBlocker.BlockAll();
            
            if (pausePanel == null) pausePanel = gameObject;
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            UIBlocker.UnblockAll();
            
            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}