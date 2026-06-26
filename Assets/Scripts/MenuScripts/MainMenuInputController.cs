using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuScripts
{
    public class MainMenuInputController : MonoBehaviour
    {
        [SerializeField]
        private SettingsMenu settingsMenu;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) HandleEscape();
        }

        private void HandleEscape()
        {
            if (settingsMenu == null) return;

            if (settingsMenu.IsOpen) settingsMenu.CloseSettings();
            else
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}