using System.Linq;
using Audio;
using Misc;
using UnityEngine;
using UnityEngine.UI;
using Retro.PSXEffects.Retro_Shaders.Runtime.Scripts;
using UnityEngine.Rendering.Universal;

namespace MenuScripts
{
    public class SettingsController : MonoBehaviour
    {
        [Header("UI Sliders")]
        [SerializeField]
        private Slider masterSlider;
        [SerializeField]
        private Slider musicSlider;
        [SerializeField]
        private Slider sfxSlider;
        [SerializeField]
        private Slider uiSlider;

        [Header("UI Toggles")]
        [SerializeField]
        private Toggle fullscreenToggle;
        [SerializeField]
        private Toggle tutorialToggle;
        [SerializeField]
        private Renderer2DData rendererData;
        [SerializeField]
        private Toggle crtToggle;

        private CrtRendererFeature crtFeature;

        private void Start()
        {
            if (rendererData != null)
            {
                crtFeature = rendererData.rendererFeatures
                    .OfType<CrtRendererFeature>()
                    .FirstOrDefault();
            }

            LoadUIValues();
        }

        private void LoadUIValues()
        {
            masterSlider.SetValueWithoutNotify(LoadAndApply("MasterVol", 0.75f));
            musicSlider.SetValueWithoutNotify(LoadAndApply("MusicVol", 0.75f));
            sfxSlider.SetValueWithoutNotify(LoadAndApply("SfxVol", 0.75f));
            uiSlider.SetValueWithoutNotify(LoadAndApply("UiVol", 0.75f));

            if (tutorialToggle)
                tutorialToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("ShowTutorial", 1) == 1);

            if (fullscreenToggle)
                fullscreenToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Fullscreen", 1) == 1);

            if (crtToggle)
            {
                var savedState = PlayerPrefs.GetInt("CRTEnabled", 1) == 1;
                crtToggle.SetIsOnWithoutNotify(savedState);
                SetCrtEffect(savedState);
            }
        }

        private static float LoadAndApply(string key, float defaultValue)
        {
            var val = PlayerPrefs.GetFloat(key, defaultValue);

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMixerVolume(key, val);

            return val;
        }

        public void SetMasterVolume(float val)
        {
            UpdateVolume("MasterVol", val);
        }

        public void SetMusicVolume(float val)
        {
            UpdateVolume("MusicVol", val);
        }

        public void SetSfxVolume(float val)
        {
            UpdateVolume("SfxVol", val);
        }

        public void SetUiVolume(float val)
        {
            UpdateVolume("UiVol", val);
        }

        private static void UpdateVolume(string key, float val)
        {
            if (!AudioManager.Instance)
                return;

            AudioManager.Instance.SetMixerVolume(key, val);
            PlayerPrefs.SetFloat(key, val);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        }

        public void SetTutorial(bool value)
        {
            PlayerPrefs.SetInt("ShowTutorial", value ? 1 : 0);
            PlayerPrefs.Save();

            var tutorial = FindAnyObjectByType<TutorialManager>(FindObjectsInactive.Include);
            if (tutorial == null)
                return;

            if (value) tutorial.TryStartTutorialFromScratch();
            else tutorial.ForceStopTutorial();
        }

        public void SetCrtEffect(bool isEnabled)
        {
            if (crtFeature)
            {
                crtFeature.SetActive(isEnabled);
                rendererData.SetDirty();
            }
            PlayerPrefs.SetInt("CRTEnabled", isEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}