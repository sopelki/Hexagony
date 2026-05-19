using Audio;
using UnityEngine;

namespace UI
{
    public class UIAudioManager : MonoBehaviour
    {
        public static UIAudioManager Instance { get; private set; }

        [SerializeField]
        private UIAudioData uiAudioData;
        [SerializeField]
        private AudioSource audioSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void PlayButtonClick()
        {
            if (uiAudioData != null && uiAudioData.buttonClickSound != null)
                audioSource.PlayOneShot(uiAudioData.buttonClickSound, uiAudioData.buttonClickVolume);
        }

        public void PlayButtonHover()
        {
            if (uiAudioData != null && uiAudioData.buttonHoverSound != null)
                audioSource.PlayOneShot(uiAudioData.buttonHoverSound, uiAudioData.buttonHoverVolume);
        }

        public void SetUIAudioData(UIAudioData data)
        {
            uiAudioData = data;
        }
    }
}