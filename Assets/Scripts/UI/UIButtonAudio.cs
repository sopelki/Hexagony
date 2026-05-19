using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI
{
    public class UIButtonAudio : MonoBehaviour
    {
        [SerializeField]
        private Button button;
        
        private bool isInitialized;

        private void OnEnable()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (!isInitialized)
            {
                button.onClick.AddListener(PlayClickSound);
                
                var eventTrigger = gameObject.GetComponent<EventTrigger>();
                if (eventTrigger == null)
                    eventTrigger = gameObject.AddComponent<EventTrigger>();

                var pointerEnter = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };
                pointerEnter.callback.AddListener(_ => PlayHoverSound());
                eventTrigger.triggers.Add(pointerEnter);

                isInitialized = true;
            }
        }

        private static void PlayClickSound()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();
        }

        private static void PlayHoverSound()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonHover();
        }

        private void OnDisable()
        {
            if (button != null)
                button.onClick.RemoveListener(PlayClickSound);
        }
    }
}