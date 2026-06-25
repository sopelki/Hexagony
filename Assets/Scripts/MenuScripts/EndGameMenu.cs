using System.Collections;
using Audio;
using Core;
using Logic.Castle;
using Logic.Monster;
using Misc;
using SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuScripts
{
    [RequireComponent(typeof(AudioSource))]
    public class EndGameMenu : MonoBehaviour
    {
        public FadePanel gameOverPanel;
        public FadePanel gameWonPanel;

        [Header("Settings")]
        [SerializeField]
        private float fadeDuration = 0.2f;
        [SerializeField]
        private float startDelay = 0.2f;
        [SerializeField]
        private float volume = 0.5f;

        [Header("Audio")]
        [SerializeField]
        private AudioClip gameOverSound;
        [SerializeField]
        private AudioClip gameWonSound;

        [Header("References")]
        [SerializeField]
        private GameInitializer gameInitializer;
        [SerializeField]
        private FadePanel menuBackground;
        [SerializeField]
        private PauseMenu pauseMenu;

        [Header("Infinite Mode")]
        [SerializeField]
        private InfiniteModeSettings infiniteSettings;
        private AudioSource audioSource;

        private CastleModel model;
        private WaveManager waveManager;

        public bool IsGameOverOpen => gameOverPanel != null && gameOverPanel.GetComponent<CanvasGroup>().alpha > 0.5f;
        public bool IsGameWonOpen => gameWonPanel != null && gameWonPanel.GetComponent<CanvasGroup>().alpha > 0.5f;

        public bool IsAnyEndGameOpen => IsGameOverOpen || IsGameWonOpen;

        public bool endGameSequenceStarted;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.ignoreListenerPause = true;
        }

        public void Initialize(CastleModel castleModel, WaveManager waveManager)
        {
            model = castleModel;
            model.OnChanged += CheckGameOver;
            this.waveManager = waveManager;
        }

        private void CheckGameOver()
        {
            if (model.IsDead)
                OpenGameOver();
        }

        public void OpenGameOver()
        {
            StartCoroutine(EndGameSequence(gameOverPanel, gameOverSound));
        }

        public void OpenWinMenu()
        {
            StartCoroutine(EndGameSequence(gameWonPanel, gameWonSound));
        }

        private IEnumerator EndGameSequence(FadePanel panel, AudioClip clip)
        {
            if (!panel)
                yield break;
    
            endGameSequenceStarted = true;
            UIBlocker.BlockAll();
    
            yield return new WaitForSeconds(startDelay);
    
            if (clip && audioSource)
            {
                AudioManager.Instance.StopMusic();
                audioSource.PlayOneShot(clip, volume);
            }

            Time.timeScale = 0f;

            if (menuBackground)
                menuBackground.Show(fadeDuration);
            
            panel.Show();
            endGameSequenceStarted = false; 
        }

        public void RestartGame()
        {
            SessionSaveManager.DeleteSession();
            SceneTransitions.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadMainMenu()
        {
            SceneTransitions.LoadScene("MainMenu");
        }

        public void ContinueInfinite()
        {
            Time.timeScale = 1f;
            endGameSequenceStarted = false; 
            UIBlocker.UnblockAll();

            gameWonPanel.Hide();
            if (menuBackground)
                menuBackground.Hide(fadeDuration);

            waveManager.StartInfiniteMode(infiniteSettings);
        }
    }
}