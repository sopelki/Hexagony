using System;
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

        public void OpenMenu()
        {
            lastPanel = mainMenuPanel;

            if (lastPanel) lastPanel.Hide(lastPanel.FadeDuration);

            if (difficultyPanel) difficultyPanel.Show();
        }

        public void CloseMenu()
        {
            difficultyPanel.Hide();

            if (lastPanel != null) lastPanel.Show(lastPanel.FadeDuration);
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

        private void Awake()
        {
            difficultyPanel = GetComponent<FadePanel>();
        }

        private void StartNewGame()
        {
            if (gameplaySoundData?.gameStartSound)
                AudioManager.Instance.PlaySfx(gameplaySoundData.gameStartSound, gameplaySoundData.gameStartVolume);

            SceneTransitions.LoadScene(gameSceneName, () =>
            {
                if (gameplaySoundData?.backgroundMusic)
                    AudioManager.Instance.PlayMusic(gameplaySoundData.backgroundMusic);

                SessionSaveManager.IsSaveLoaded = false;
                SessionSaveManager.DeleteSession();

                GC.Collect();
            });
        }
    }
}