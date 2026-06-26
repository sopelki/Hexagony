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
        [SerializeField]
        private DifficultyMenu difficultyMenu;
        
        public static bool ShouldOpenDifficultyOnStart;

        private void Start()
        {
            Debug.Log("Save path: " + Application.persistentDataPath);
            if (menuAudioData != null && menuAudioData.mainMenuMusic != null)
                AudioManager.Instance.PlayMusic(menuAudioData.mainMenuMusic);

            if (continueButton != null)
            {
                var hasSession = SessionSaveManager.HasSavedSession();
                continueButton.interactable = hasSession;
            }
            
            if (difficultyMenu != null)
                difficultyMenu.gameObject.SetActive(false);
            
            if (ShouldOpenDifficultyOnStart)
            {
                ShouldOpenDifficultyOnStart = false;
                PlayGame();
            }
        }
        
        public void PlayGame()
        {
            if (difficultyMenu != null)
                difficultyMenu.OpenMenu();
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
            if (settingsMenu != null)
                settingsMenu.OpenSettings();
            else
                Debug.LogError("SettingsMenu reference is missing in MainMenu script!");
        }
    }
}