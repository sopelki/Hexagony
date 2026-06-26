using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private AudioMixer audioMixer;
        [SerializeField]
        private AudioSource musicSource;
        [SerializeField]
        private AudioSource sfxSource;

        [Header("Master Scales")]
        [Range(0f, 1f)]
        [SerializeField]
        private float musicMasterScale = 0.5f;
        [Range(0f, 1f)]
        [SerializeField]
        private float sfxMasterScale = 0.5f;

        public static AudioManager Instance { get; private set; }
        public bool MuteSfx { get; set; }

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSources();
        }

        private void Start()
        {
            SyncVolumesWithPrefs();
        }

        private void SyncVolumesWithPrefs()
        {
            SetMixerVolume("MasterVol", PlayerPrefs.GetFloat("MasterVol", 0.75f));
            SetMixerVolume("MusicVol", PlayerPrefs.GetFloat("MusicVol", 0.75f));
            SetMixerVolume("SfxVol", PlayerPrefs.GetFloat("SfxVol", 0.75f));
            SetMixerVolume("UiVol", PlayerPrefs.GetFloat("UiVol", 0.75f));
        }

        private void InitializeSources()
        {
            if (!musicSource)
                musicSource = gameObject.AddComponent<AudioSource>();
            if (!sfxSource)
                sfxSource = gameObject.AddComponent<AudioSource>();

            musicSource.volume = musicMasterScale;
            sfxSource.volume = sfxMasterScale;
        }

        public void SetMixerVolume(string parameterName, float sliderValue)
        {
            if (!audioMixer)
                return;

            var dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;
            audioMixer.SetFloat(parameterName, dB);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource.clip == clip && musicSource.isPlaying)
                StopMusic();

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void PlaySfx(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (MuteSfx || !clip)
                return;

            sfxSource.PlayOneShot(clip, volumeMultiplier);
        }

        public void PlayRandomSfx(AudioClip[] clips, float volumeMultiplier = 1f)
        {
            if (MuteSfx || clips == null || clips.Length == 0)
                return;

            PlaySfx(clips[Random.Range(0, clips.Length)], volumeMultiplier);
        }
    }
}