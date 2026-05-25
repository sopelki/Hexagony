using UnityEngine;
using Audio;
using Core;

namespace MenuScripts
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        private MenuAudioData menuAudioData;
        [SerializeField]
        private SoundData gameplaySoundData;
        [SerializeField]
        private SettingsMenu settingsMenu;

        private void Start()
        {
            if (menuAudioData != null && menuAudioData.mainMenuMusic != null)
                AudioManager.Instance.PlayMusic(menuAudioData.mainMenuMusic);
        }

        public void PlayGame() => SceneTransitions.LoadScene("GameScene");

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OpenSettings()
        {
            if (settingsMenu != null)
                settingsMenu.OpenSettings();
            else
                Debug.LogError("SettingsMenu reference is missing in MainMenu script!");
        }
    }
}