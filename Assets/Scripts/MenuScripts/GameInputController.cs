using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using System.Linq;
using Retro.PSXEffects.Retro_Shaders.Runtime.Scripts;

namespace MenuScripts
{
    public class GameInputController : MonoBehaviour
    {
        [Header("Ссылки на меню")]
        [SerializeField]
        private PauseMenu pauseMenu;
        [SerializeField]
        private SettingsMenu settingsMenu;
        [SerializeField]
        private HelpMenu helpMenu;
        [SerializeField]
        private EndGameMenu endGameMenu;

        [Header("Настройки скорости")]
        [SerializeField]
        private float speedUpMultiplier = 3.0f;
        [SerializeField]
        private FadePanel speedUpIcon;

        [Header("Эффекты (URP)")]
        [SerializeField]
        private Renderer2DData rendererData;
        [SerializeField]
        private float vhsTrackingTarget = 0.02f;

        private bool isSpedUp;
        private PsxRendererFeature psxFeature;

        private void Start()
        {
            if (speedUpIcon)
                speedUpIcon.Hide(0);

            if (rendererData != null)
                psxFeature = rendererData.rendererFeatures.OfType<PsxRendererFeature>().FirstOrDefault();

            UpdateVfx(0);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                HandleEscape();

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                HandleSpeedToggle();

            var isAnyMenuOpen = IsAnyMenuOpen();

            if (isSpedUp && !isAnyMenuOpen && Mathf.Approximately(Time.timeScale, 1f))
            {
                Time.timeScale = speedUpMultiplier;
                UpdateVfx(vhsTrackingTarget);

                if (speedUpIcon && speedUpIcon.CurrentAlpha < 0.1f)
                    speedUpIcon.Show();
            }

            if (isAnyMenuOpen)
                UpdateVfx(0f);
        }

        private void HandleSpeedToggle()
        {
            if (IsAnyMenuOpen()) return;

            isSpedUp = !isSpedUp;
            Time.timeScale = isSpedUp ? speedUpMultiplier : 1f;

            UpdateVfx(isSpedUp ? vhsTrackingTarget : 0f);

            if (speedUpIcon != null)
            {
                if (isSpedUp) speedUpIcon.Show();
                else speedUpIcon.Hide();
            }
        }

        private void UpdateVfx(float value)
        {
            if (psxFeature != null && psxFeature.settings != null)
            {
                psxFeature.settings.vhsTracking = value;

                rendererData.SetDirty();
            }
        }

        private void OnDisable() => UpdateVfx(0);

        private void HandleEscape()
        {
            if (helpMenu && helpMenu.IsOpen)
            {
                helpMenu.CloseHelp();
                return;
            }

            if (settingsMenu && settingsMenu.IsOpen)
            {
                settingsMenu.CloseSettings();
                return;
            }

            if (pauseMenu && pauseMenu.IsOpen)
            {
                pauseMenu.ResumeGame();
                return;
            }

            if (endGameMenu && endGameMenu.IsAnyEndGameOpen)
                return;

            if (pauseMenu)
                pauseMenu.OpenPause();
        }

        private bool IsAnyMenuOpen()
        {
            var help = helpMenu && helpMenu.IsOpen;
            var settings = settingsMenu && settingsMenu.IsOpen;
            var pause = pauseMenu && pauseMenu.IsOpen;
            var end = endGameMenu && endGameMenu.IsAnyEndGameOpen;
            return help || settings || pause || end;
        }
    }
}