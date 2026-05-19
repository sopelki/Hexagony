using UnityEngine;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField]
        private AudioSource musicSource;
        [SerializeField]
        private AudioSource sfxSource;

        [Header("Volume Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float musicVolume = 0.25f;
        [SerializeField]
        [Range(0f, 1f)]
        private float sfxVolume = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();
            
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();

            ApplyVolumes();
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic() => musicSource.Stop();

        public void PlaySfx(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: AudioClip is null!");
                return;
            }

            sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
        }

        public void PlayRandomSfx(AudioClip[] clips, float volumeMultiplier = 1f)
        {
            if (clips == null || clips.Length == 0)
            {
                Debug.LogWarning("AudioManager: AudioClip array is empty or null!");
                return;
            }

            var randomClip = clips[Random.Range(0, clips.Length)];
            PlaySfx(randomClip, volumeMultiplier);
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }

        public void SetSfxVolume(float volume) => sfxVolume = Mathf.Clamp01(volume);

        public float GetMusicVolume() => musicVolume;
        
        public float GetSfxVolume() => sfxVolume;

        private void ApplyVolumes() => musicSource.volume = musicVolume;
    }
}