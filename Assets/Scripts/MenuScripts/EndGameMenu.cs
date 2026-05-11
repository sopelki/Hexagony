using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logic.Castle;
using UnityEngine.EventSystems;

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
            FindObjectsByType<MonoBehaviour>()
                .Where(mb => mb is IBeginDragHandler or IEndDragHandler or IDragHandler)
                .ToList()
                .ForEach(mb => mb.enabled = false);
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