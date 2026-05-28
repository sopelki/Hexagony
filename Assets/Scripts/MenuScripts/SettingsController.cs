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

        private void OnEnable()
        {
            LoadUIValues();
        }

        private void LoadUIValues()
        {
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 0.75f);
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.75f);
            sfxSlider.value = PlayerPrefs.GetFloat("SfxVol", 0.75f);
            uiSlider.value = PlayerPrefs.GetFloat("UiVol", 0.75f);
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
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
    }
}