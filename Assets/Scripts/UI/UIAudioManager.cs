using Audio;
using UnityEngine;

namespace UI
{
    public class UIAudioManager : MonoBehaviour
    {
        [SerializeField]
        private UIAudioData uiAudioData;
        [SerializeField]
        private AudioSource audioSource;
        public static UIAudioManager Instance { get; private set; }

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
    }
}