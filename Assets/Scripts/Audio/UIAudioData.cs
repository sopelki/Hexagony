using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "UIAudioData", menuName = "Audio/UI Audio Data")]
    public class UIAudioData : ScriptableObject
    {
        [Header("Button Sounds")]
        public AudioClip buttonClickSound;
        public AudioClip buttonHoverSound;

        [Header("Volume Settings")]
        [Range(0f, 1f)]
        public float buttonClickVolume = 0.8f;
        [Range(0f, 1f)]
        public float buttonHoverVolume = 0.6f;
    }
}