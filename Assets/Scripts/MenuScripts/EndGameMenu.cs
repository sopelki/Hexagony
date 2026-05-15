using UnityEngine;
using UnityEngine.SceneManagement;
using Logic.Castle;
using Misc;

namespace MenuScripts
{
    public class EndGameMenu : MonoBehaviour
    {
        public GameObject gameOverPanel;
        public GameObject gameWonPanel;

        [SerializeField]
        private Core.GameInitializer gameInitializer;

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

        public void OpenGameOver() => OpenMenu(gameOverPanel);

        public void OpenWinMenu() => OpenMenu(gameWonPanel);

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

        private static void OpenMenu(GameObject menu)
        {
            UIBlocker.BlockAll();
    
            menu.SetActive(true);
            Time.timeScale = 0f;
        }

        private void OnDestroy()
        {
            if (model != null)
                model.OnChanged -= CheckGameOver;
        }
    }
}