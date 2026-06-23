using Audio;
using Core;
using SaveSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField]
        private Button continueButton;

        private void Start()
        {
            Debug.Log("Save path: " + Application.persistentDataPath);
            if (menuAudioData != null && menuAudioData.mainMenuMusic != null)
                AudioManager.Instance.PlayMusic(menuAudioData.mainMenuMusic);

            if (continueButton != null)
            {
                var hasSession = SessionSaveManager.HasSavedSession();
                continueButton.gameObject.SetActive(hasSession);
            }
        }

        public void PlayGame()
        {
            SessionSaveManager.IsSaveLoaded = false;
            SessionSaveManager.DeleteSession();
            SceneTransitions.LoadScene("GameScene");
        }

        public void ContinueGame()
        {
            if (SessionSaveManager.HasSavedSession())
            {
                SessionSaveManager.IsSaveLoaded = true;
                SceneTransitions.LoadScene("GameScene");
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
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