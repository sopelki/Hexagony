using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DialogueAnimator : MonoBehaviour
    {
        [Header("Компоненты UI")]
        [SerializeField]
        private Image characterAvatar;
        [SerializeField]
        private TextMeshProUGUI dialogueText;

        [Header("Настройки скорости")]
        [SerializeField]
        private float textSpeed = 0.05f;

        public void PrintPhrase(string phrase)
        {
            StopAllCoroutines();
            StartCoroutine(TypeText(phrase));
        }

        private IEnumerator TypeText(string phrase)
        {
            dialogueText.text = "";

            foreach (var letter in phrase.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(textSpeed);
            }
        }
    }
}