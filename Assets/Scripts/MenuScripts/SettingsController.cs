using Logic.Castle;
using Misc;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace MenuScripts
{
    public class SettingsController : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField]
        private AudioMixer audioMixer;
        [SerializeField]
        private Slider masterSlider;
        [SerializeField]
        private Slider musicSlider;
        [SerializeField]
        private Slider sfxSlider;
        [SerializeField]
        private Slider uiSlider;

        [Header("Graphics")]
        [SerializeField]
        private Toggle fullscreenToggle;
        
        [Header("Tutorial")]
        [SerializeField]
        private Toggle tutorialToggle;

        private void OnEnable()
        {
            LoadUIValues();
        }

        private void LoadUIValues()
        {
            var savedValue = PlayerPrefs.GetInt("ShowTutorial", 1);
            if (tutorialToggle != null)
            {
                tutorialToggle.SetIsOnWithoutNotify(savedValue == 1);
                Debug.Log($"UI Synced: Toggle is now {savedValue == 1} based on PlayerPrefs");
            }
            
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 0.75f);
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.75f);
            sfxSlider.value = PlayerPrefs.GetFloat("SfxVol", 0.75f);
            uiSlider.value = PlayerPrefs.GetFloat("UiVol", 0.75f);
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            // tutorialToggle.isOn = PlayerPrefs.GetInt("ShowTutorial", 1) == 1;
            // tutorialToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("ShowTutorial", 1) == 1);
        }

        public void SetMasterVolume(float volume)
        {
            ApplyVolume("MasterVol", volume);
        }

        public void SetMusicVolume(float volume)
        {
            ApplyVolume("MusicVol", volume);
        }

        public void SetSfxVolume(float volume)
        {
            ApplyVolume("SfxVol", volume);
        }

        public void SetUiVolume(float volume)
        {
            ApplyVolume("UiVol", volume);
        }

        private void ApplyVolume(string parameterName, float volume)
        {
            if (audioMixer == null) return;
            var clampedVol = Mathf.Clamp(volume, 0.0001f, 1f);
            audioMixer.SetFloat(parameterName, Mathf.Log10(clampedVol) * 20);
            PlayerPrefs.SetFloat(parameterName, volume);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        }
        
        public void SetTutorial(bool value)
        {
            Debug.Log("SetTutorial called: " + value);
            PlayerPrefs.SetInt("ShowTutorial", value ? 1 : 0);
            PlayerPrefs.Save();

            var tutorial = FindAnyObjectByType<TutorialManager>(FindObjectsInactive.Include);
            
            if (tutorial == null) 
                return;

            if (value)
                tutorial.TryStartTutorialFromScratch();
            else
                tutorial.ForceStopTutorial();
        }
    }
}