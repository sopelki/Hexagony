using Audio;
using Core;
using SaveSystem;
using UnityEngine;

namespace MenuScripts
{
    public class DifficultyMenu : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField]
        private string gameSceneName = "GameScene";
        [SerializeField]
        private SoundData gameplaySoundData;

        [Header("Other Panels")]
        [SerializeField]
        private FadePanel mainMenuPanel;

        private FadePanel difficultyPanel;
        private FadePanel lastPanel;

        private void Awake()
        {
            difficultyPanel = GetComponent<FadePanel>();
        }

        public void OpenMenu()
        {
            lastPanel = mainMenuPanel;

            if (lastPanel != null)
                lastPanel.Hide(lastPanel.FadeDuration);

            gameObject.SetActive(true);
            difficultyPanel.Show();
        }

        public void CloseMenu()
        {
            difficultyPanel.Hide();

            if (lastPanel != null)
                lastPanel.Show(lastPanel.FadeDuration);

            Invoke(nameof(DeactivatePanel), difficultyPanel.FadeDuration);
        }

        public void StartEasyGame()
        {
            DifficultyManager.CurrentDifficulty = GameDifficulty.Easy;
            StartNewGame();
        }

        public void StartNormalGame()
        {
            DifficultyManager.CurrentDifficulty = GameDifficulty.Normal;
            StartNewGame();
        }

        public void StartHardGame()
        {
            DifficultyManager.CurrentDifficulty = GameDifficulty.Hard;
            StartNewGame();
        }

        private void DeactivatePanel() => gameObject.SetActive(false);

        private void StartNewGame()
        {
            if (gameplaySoundData?.gameStartSound)
                AudioManager.Instance.PlaySfx(gameplaySoundData.gameStartSound, gameplaySoundData.gameStartVolume);

            SessionSaveManager.IsSaveLoaded = false;
            SessionSaveManager.DeleteSession();

            if (gameplaySoundData?.backgroundMusic)
                AudioManager.Instance.PlayMusic(gameplaySoundData.backgroundMusic);

            SceneTransitions.LoadScene(gameSceneName);
        }
    }
}