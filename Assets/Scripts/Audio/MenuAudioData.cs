using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "MenuAudioData", menuName = "Audio/Menu Audio Data")]
    public class MenuAudioData : ScriptableObject
    {
        [Header("Main Menu Music")]
        public AudioClip mainMenuMusic;
    }
}