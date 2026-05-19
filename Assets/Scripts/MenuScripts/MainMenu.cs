using UnityEngine;
using UnityEngine.SceneManagement;
using Audio;

namespace MenuScripts
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        private MenuAudioData menuAudioData;
        [SerializeField]
        private SoundData gameplaySoundData;

        private void Start()
        {
            if (menuAudioData != null && menuAudioData.mainMenuMusic != null)
                AudioManager.Instance.PlayMusic(menuAudioData.mainMenuMusic);
        }

        public void PlayGame()
        {
            if (gameplaySoundData != null && gameplaySoundData.backgroundMusic != null)
                AudioManager.Instance.PlayMusic(gameplaySoundData.backgroundMusic);

            SceneManager.LoadScene("GameScene");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            Debug.Log("Game is exited.");
        }
        
        public void Settings()
        {
            Debug.Log("Settings opened");
        }
    }
}