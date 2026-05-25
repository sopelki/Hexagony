using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Audio;
using Core;
using Logic.Castle;
using Misc;

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

        [Header("Audio")]
        [SerializeField]
        private AudioClip gameOverSound;
        [SerializeField]
        private AudioClip gameWonSound;

        [Header("References")]
        [SerializeField]
        private Core.GameInitializer gameInitializer;
        [SerializeField]
        private FadePanel menuBackground;

        private CastleModel model;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.ignoreListenerPause = true;
        }

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

        public void OpenGameOver() => StartCoroutine(EndGameSequence(gameOverPanel, gameOverSound));

        public void OpenWinMenu() => StartCoroutine(EndGameSequence(gameWonPanel, gameWonSound));

        private IEnumerator EndGameSequence(FadePanel panel, AudioClip clip)
        {
            if (!panel)
                yield break;

            if (clip && audioSource)
            {
                AudioManager.Instance.StopMusic();
                audioSource.PlayOneShot(clip);
            }

            UIBlocker.BlockAll();
            Time.timeScale = 0f;

            if (menuBackground)
                menuBackground.Show(fadeDuration);

            panel.Show();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneTransitions.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneTransitions.LoadScene("MainMenu");
        }
    }
}