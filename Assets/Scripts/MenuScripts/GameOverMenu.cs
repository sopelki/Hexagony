using UnityEngine;
using UnityEngine.SceneManagement;
using Logic.Castle;

namespace MenuScripts
{
    public class GameOverMenu : MonoBehaviour
    {
        public GameObject gameOverPanel;
        private CastleModel model;
        
        public void Initialize(CastleModel castleModel)
        {
            model = castleModel;
            model.OnChanged += CheckGameOver;
        }

        private void CheckGameOver()
        {
            if (model.IsDead)
                OpenGameOver();
        }

        public void OpenGameOver()
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            if (model != null)
                model.OnChanged -= CheckGameOver;
        }
        
    }
}