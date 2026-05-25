using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DialogueAnimator : MonoBehaviour
    {
        [Header("Компоненты UI")]
        [SerializeField] private Image characterAvatar;
        [SerializeField] private TextMeshProUGUI dialogueText;

        [Header("Настройки скорости")]
        [SerializeField] private float textSpeed = 0.05f;
        
        public void PrintPhrase(string phrase)
        {
            StopAllCoroutines();
            StartCoroutine(TypeText(phrase));
        }
        
        private IEnumerator TypeText(string phrase)
        {
            dialogueText.text = "";

            foreach (char letter in phrase.ToCharArray())
            {
                dialogueText.text += letter;
                // Тут можно добавить воспроизведение короткого пиксельного звука "блып-блып"
                yield return new WaitForSecondsRealtime(textSpeed); 
            }
        }
    }
}