using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Audio;

namespace UI
{
    public class DialogueAnimator : MonoBehaviour
    {
        [Header("Компоненты UI")]
        [SerializeField]
        private Image characterAvatar;
        [SerializeField]
        private TextMeshProUGUI dialogueText;
        [SerializeField]
        private AudioSource audioSource;

        [Header("Настройки текста")]
        [SerializeField]
        private float textSpeed = 0.05f;

        [Header("Настройки звука")]
        [SerializeField]
        private SoundData soundData;
        [SerializeField]
        private float minTimeBetweenSounds = 0.05f;

        private float lastSoundTime;

        public void PrintPhrase(string phrase)
        {
            StopDialogue();
            StartCoroutine(TypeText(phrase));
        }

        public void StopDialogue()
        {
            StopAllCoroutines();
            if (audioSource != null)
                audioSource.Stop();
        }

        private IEnumerator TypeText(string phrase)
        {
            dialogueText.text = phrase;
            dialogueText.maxVisibleCharacters = 0;

            dialogueText.ForceMeshUpdate();

            var textInfo = dialogueText.textInfo;
            var totalVisibleCharacters = textInfo.characterCount;

            for (var i = 0; i <= totalVisibleCharacters; i++)
            {
                dialogueText.maxVisibleCharacters = i;

                if (i > 0 && i <= totalVisibleCharacters)
                {
                    var character = textInfo.characterInfo[i - 1].character;
                    TryPlayTypingSound(character);
                }

                float timer = 0;
                while (timer < textSpeed)
                {
                    if (Time.timeScale > 0)
                        timer += Time.unscaledDeltaTime;

                    yield return null;
                }
            }
        }

        private void TryPlayTypingSound(char currentLetter)
        {
            if (char.IsWhiteSpace(currentLetter) || currentLetter == '\0')
                return;

            if (Time.unscaledTime - lastSoundTime >= minTimeBetweenSounds)
            {
                if (soundData != null && soundData.typingSounds is { Length: > 0 })
                {
                    var randomIndex = Random.Range(0, soundData.typingSounds.Length);
                    var clip = soundData.typingSounds[randomIndex];

                    if (clip != null)
                    {
                        audioSource.PlayOneShot(clip, soundData.typingVolume);
                        lastSoundTime = Time.unscaledTime;
                    }
                }
            }
        }
    }
}