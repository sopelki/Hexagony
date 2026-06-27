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
        public static bool ShouldOpenDifficultyOnStart;

        [SerializeField]
        private MenuAudioData menuAudioData;
        [SerializeField]
        private SoundData gameplaySoundData;
        [SerializeField]
        private SettingsMenu settingsMenu;
        [SerializeField]
        private Button continueButton;
        [SerializeField]
        private DifficultyMenu difficultyMenu;

        public void PlayGame()
        {
            if (difficultyMenu != null) difficultyMenu.OpenMenu();
        }

        public void ContinueGame()
        {
            if (SessionSaveManager.HasSavedSession())
            {
                if (gameplaySoundData?.gameStartSound)
                    AudioManager.Instance.PlaySfx(gameplaySoundData.gameStartSound, gameplaySoundData.gameStartVolume);

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
            if (settingsMenu != null) settingsMenu.OpenSettings();
            else Debug.LogError("SettingsMenu reference is missing in MainMenu script!");
        }

        private void Start()
        {
            Debug.Log("Save path: " + Application.persistentDataPath);

            if (continueButton != null)
            {
                var hasSession = SessionSaveManager.HasSavedSession();
                continueButton.interactable = hasSession;
            }

            if (ShouldOpenDifficultyOnStart)
            {
                ShouldOpenDifficultyOnStart = false;
                PlayGame();
            }
            
            if (menuAudioData != null && menuAudioData.mainMenuMusic != null)
                AudioManager.Instance.PlayMusic(menuAudioData.mainMenuMusic);
        }
    }
}