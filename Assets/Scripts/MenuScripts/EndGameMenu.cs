using System.Collections;
using Audio;
using Core;
using Logic.Castle;
using Logic.Monster;
using Misc;
using UnityEngine;

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

        [Header("Effects")]
        [SerializeField]
        private ParticleSystem leftWinConfetti;
        [SerializeField]
        private ParticleSystem rightWinConfetti;

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

        public bool endGameSequenceStarted;

        private AudioSource audioSource;

        private CastleModel model;
        private WaveManager waveManager;

        public bool IsGameOverOpen => gameOverPanel != null && gameOverPanel.GetComponent<CanvasGroup>().alpha > 0.5f;
        public bool IsGameWonOpen => gameWonPanel != null && gameWonPanel.GetComponent<CanvasGroup>().alpha > 0.5f;

        public bool IsAnyEndGameOpen => IsGameOverOpen || IsGameWonOpen;

        public void Initialize(CastleModel castleModel, WaveManager waveManager)
        {
            model = castleModel;
            model.OnChanged += CheckGameOver;
            this.waveManager = waveManager;
        }

        public void OpenGameOver()
        {
            StartCoroutine(EndGameSequence(gameOverPanel, gameOverSound, false));
        }

        public void OpenWinMenu()
        {
            StartCoroutine(EndGameSequence(gameWonPanel, gameWonSound, true));
        }

        public void RestartGame()
        {
            MainMenu.ShouldOpenDifficultyOnStart = true;
            Time.timeScale = 1f;
            SceneTransitions.LoadScene("MainMenu");
        }

        public void LoadMainMenu()
        {
            SceneTransitions.LoadScene("MainMenu");
        }

        public void ContinueInfinite()
        {
            Time.timeScale = 1f;
            UIBlocker.UnblockAll();

            if (leftWinConfetti && rightWinConfetti)
            {
                leftWinConfetti.Stop();
                rightWinConfetti.Stop();
            }

            gameWonPanel.Hide();
            if (menuBackground) menuBackground.Hide(fadeDuration);

            waveManager.StartInfiniteMode(infiniteSettings);
        }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.ignoreListenerPause = true;
        }

        private void CheckGameOver()
        {
            if (model.IsDead) OpenGameOver();
        }

        private IEnumerator EndGameSequence(FadePanel panel, AudioClip clip, bool isWin)
        {
            if (!panel) yield break;

            endGameSequenceStarted = true;
            UIBlocker.BlockAll();

            yield return new WaitForSeconds(startDelay);

            if (clip && audioSource)
            {
                AudioManager.Instance.StopMusic();
                audioSource.PlayOneShot(clip, volume);
            }

            Time.timeScale = 0f;

            if (menuBackground) menuBackground.Show(fadeDuration);

            panel.Show();

            if (isWin && leftWinConfetti && rightWinConfetti)
            {
                leftWinConfetti.Play();
                rightWinConfetti.Play();
            }

            endGameSequenceStarted = false;
        }
    }
}