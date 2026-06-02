using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Logic.Castle;
using Logic.Tower;
using Logic.Trap;
using Logic.Unit;
using MenuScripts;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using View;

namespace Misc
{
    public class TutorialManager : MonoBehaviour
    {
        [Header("Ссылки на элементы")]
        [SerializeField]
        private FadePanel tutorialFadePanel;
        [SerializeField]
        private DialogueAnimator dialogueAnimator;
        [SerializeField]
        private Button actionButton;
        [SerializeField]
        private TextMeshProUGUI actionButtonText;
        [SerializeField]
        private GameObject barrackSlot, towerSlot, trapSlot, helpButton, pauseButton, castleGrid;
        [SerializeField]
        private GameObject highlightEffect, highlightEffectCastle, highlightEffectHex;

        [Header("Настройки задержки")]
        [SerializeField]
        private float startDelay = 1.5f;

        [SerializeField]
        private float clickCooldown = 0.3f;

        [Header("Highlight Animation")]
        [SerializeField]
        private float highlightPulseSpeed = 1.8f;

        private bool barrackTracked, towerTracked, trapTracked;
        private TutorialStep currentStep = TutorialStep.Greeting;
        private GameFlowManager gameFlowManager;
        private Image highlightImage;
        private float lastClickTime;
        private List<GameObject> towerSlots;
        private bool IsRunning { get; set; }

        private void Start()
        {
            var foundSlots = GameObject.FindGameObjectsWithTag("hexHighlight");
            towerSlots = foundSlots.ToList();
            highlightImage = highlightEffect.GetComponent<Image>();

            if (actionButton)
                actionButton.onClick.AddListener(OnActionButtonClick);

            if (PlayerPrefs.GetInt("ShowTutorial", 1) == 1)
                TryStartTutorialFromScratch();
            else
                ForceStopTutorial();
        }

        private void Update()
        {
            if (PlayerPrefs.GetInt("ShowTutorial", 1) == 0)
                return;

            AnimateHighlight();

            switch (currentStep)
            {
                case TutorialStep.BuildBarrack:
                    if (CastleSystem.Instance != null &&
                        CastleSystem.Instance.Model.Buildings.Count > 0 &&
                        CastleSystem.Instance.Model.Buildings.Any(b => b.Data.type == BuildingType.Barracks))
                    {
                        currentStep = TutorialStep.BarrackSuccess;
                        UpdateTutorialState();
                    }
                    break;

                case TutorialStep.BuildTower:
                    if (TowerSystem.Instance != null &&
                        TowerSystem.Instance.GetTowers().Count > 0 &&
                        TowerSystem.Instance.GetTowers().Any(t => t.Data.type == TowerType.Archer))
                    {
                        currentStep = TutorialStep.TowerSuccess;
                        UpdateTutorialState();
                    }
                    break;

                case TutorialStep.BuildTrap:
                    if (TrapSystem.Instance != null &&
                        TrapSystem.Instance.GetTraps().Count > 0 &&
                        TrapSystem.Instance.GetTraps().Any(t => t.Data.trapType == TrapType.SlowZone))
                    {
                        currentStep = TutorialStep.HelpExplanation;
                        UpdateTutorialState();
                    }
                    break;
            }
        }

        public void Setup(GameFlowManager flowManager)
        {
            gameFlowManager = flowManager;
        }

        public static bool IsTutorialActive()
        {
            var tutorial = FindAnyObjectByType<TutorialManager>();
            return tutorial != null && tutorial.IsRunning;
        }

        public void OnActionButtonClick()
        {
            if (Time.time - lastClickTime < clickCooldown)
                return;

            lastClickTime = Time.time;

            switch (currentStep)
            {
                case TutorialStep.Greeting:
                    currentStep = TutorialStep.BuildBarrack;
                    break;

                case TutorialStep.BarrackSuccess:
                    currentStep = TutorialStep.BuildTower;
                    break;

                case TutorialStep.TowerSuccess:
                    currentStep = TutorialStep.BuildTrap;
                    break;

                case TutorialStep.HelpExplanation:
                    currentStep = TutorialStep.ClickExplanation;
                    break;
                
                case TutorialStep.ClickExplanation:
                    currentStep = TutorialStep.PauseExplanation;
                    break;

                case TutorialStep.PauseExplanation:
                    currentStep = TutorialStep.SpeedExplanation;
                    break;

                case TutorialStep.SpeedExplanation:
                    currentStep = TutorialStep.Finish;
                    break;

                case TutorialStep.Finish:
                    FinishTutorialAndStartRealGame();
                    return;

                case TutorialStep.BuildBarrack:
                case TutorialStep.BuildTower:
                case TutorialStep.BuildTrap:
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateTutorialState();
        }

        private void UpdateTutorialState()
        {
            if (highlightEffect)
                highlightEffect.SetActive(false);
            if (highlightEffectCastle)
                highlightEffectCastle.SetActive(false);
            ClearHexHighlights();

            switch (currentStep)
            {
                case TutorialStep.Greeting:
                    ConfigureButton(true, "Далее");
                    PrintPhrase("Здравствуйте, правитель Гексагонии! Ваш замок атакуют монстры.");
                    break;

                case TutorialStep.BuildBarrack:
                    ConfigureButton(false);
                    PrintPhrase(
                        "Давайте начнём строить! Перетяните <color=#FFEE58>Казарму</color> в\u00A0замок <size=65%>(сетка 3x3)</size>.");
                    ApplyHighlight(barrackSlot);
                    ApplyCastleHighlight(castleGrid);
                    break;

                case TutorialStep.BarrackSuccess:
                    ConfigureButton(true, "Далее");
                    PrintPhrase(
                        "Отлично, <color=#FFEE58>Казарма</color> готова! Теперь у\u00A0вас есть верные рыцари.");
                    break;

                case TutorialStep.BuildTower:
                    ConfigureButton(false);
                    PrintPhrase(
                        "Защита периметра превыше всего! Перетяние <color=#FFEE58>Башню</color> в\u00A0слот на\u00A0поле боя.");
                    ApplyHighlight(towerSlot);
                    ApplyHexHighlight(towerSlots);
                    break;

                case TutorialStep.TowerSuccess:
                    ConfigureButton(true, "Далее");
                    PrintPhrase("Защита установлена! Милорд,\u00A0вы отлично справляетесь.");
                    break;

                case TutorialStep.BuildTrap:
                    ConfigureButton(false);
                    PrintPhrase("Остался последний штрих. Перетяните <color=#FFEE58>Ловушку</color> на\u00A0дорогу.");
                    ApplyHighlight(trapSlot);
                    break;

                case TutorialStep.HelpExplanation:
                    ConfigureButton(true, "Далее");
                    ApplyHighlight(helpButton);
                    PrintPhrase(
                        "Изучите другие \u00A0постройки в магазине или прочтите <color=#FFEE58>Справку</color>.");
                    break;
                
                case TutorialStep.ClickExplanation:
                    ConfigureButton(true, "Далее");
                    PrintPhrase(
                        "Вы также можете наносить урон монстрам, кликая <nobr>по ним.</nobr>");
                    break;

                case TutorialStep.PauseExplanation:
                    ConfigureButton(true, "Далее");
                    ApplyHighlight(pauseButton);
                    PrintPhrase(
                        "Для настройки или выхода из игры нажмите <nobr><color=#FFEE58>Паузу</color> или <color=#FFEE58>Esc</color>.</nobr>");
                    break;

                case TutorialStep.SpeedExplanation:
                    ConfigureButton(true, "Далее");
                    PrintPhrase(
                        "Кстати, вы можете ускорить время,\u00A0нажав клавишу <color=#FFEE58>Пробел</color>.");
                    break;

                case TutorialStep.Finish:
                    ConfigureButton(true, "К игре!");
                    PrintPhrase("Теперь вы готовы защищать замок. Начнём\u00A0настоящий бой!");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ConfigureButton(bool isVisible, string text = "")
        {
            if (actionButton)
                actionButton.gameObject.SetActive(isVisible);
            if (actionButtonText && isVisible)
                actionButtonText.text = text;
        }

        private void PrintPhrase(string text)
        {
            if (dialogueAnimator)
                dialogueAnimator.PrintPhrase(text);
        }

        private void ApplyHighlight(GameObject slot)
        {
            if (highlightEffect && slot != null)
            {
                highlightEffect.transform.position = slot.transform.position;
                highlightEffect.SetActive(true);
            }
        }

        private void AnimateHighlight()
        {
            if (highlightEffect == null || !highlightEffect.activeSelf || highlightImage == null)
                return;

            var color = highlightImage.color;
            var t = Mathf.PingPong(Time.time * highlightPulseSpeed, 1f);

            var smoothT = Mathf.SmoothStep(0f, 1f, t);
            smoothT = Mathf.SmoothStep(0f, 1f, smoothT);

            color.a = smoothT * (96f / 255f);
            highlightImage.color = color;
        }

        private void ApplyCastleHighlight(GameObject slot)
        {
            if (highlightEffectCastle && slot != null)
            {
                var slotRect = slot.GetComponent<RectTransform>();
                var highlightRect = highlightEffectCastle.GetComponent<RectTransform>();

                if (slotRect != null && highlightRect != null)
                {
                    highlightEffectCastle.transform.position = slot.transform.position;
                    highlightRect.sizeDelta = slotRect.sizeDelta;
                    highlightEffectCastle.SetActive(true);
                }
            }
        }

        private static void ApplyHexHighlight(List<GameObject> slots)
        {
            if (slots == null || slots.Count == 0)
                return;

            foreach (var slot in slots)
            {
                if (slot == null)
                    continue;

                var highlight = slot.transform.Find("Highlight");

                if (highlight != null)
                    highlight.gameObject.SetActive(true);
            }
        }

        private void ClearHexHighlights()
        {
            if (towerSlots == null) return;

            foreach (var slot in towerSlots)
            {
                if (slot == null) continue;
                var highlight = slot.transform.Find("Highlight");
                if (highlight != null)
                    highlight.gameObject.SetActive(false);
            }
        }

        private static void ClearAllTutorialBuildings()
        {
            if (TowerSystem.Instance != null)
            {
                FindAnyObjectByType<TowerViewManager>()?.DestroyAllTowers();
                TowerSystem.Instance.Clear();
            }
            if (TrapSystem.Instance != null)
            {
                FindAnyObjectByType<TrapViewManager>()?.DestroyAllTraps();
                TrapSystem.Instance.Clear();
            }
            if (UnitSystem.Instance != null)
            {
                FindAnyObjectByType<UnitViewManager>()?.DestroyAllUnits();
                UnitSystem.Instance.Clear();
            }
            if (CastleSystem.Instance != null)
            {
                CastleSystem.Instance.Clear();
                foreach (var b in FindObjectsByType<InventoryItem>(FindObjectsInactive.Exclude))
                    Destroy(b.gameObject);
            }
        }

        private void FinishTutorialAndStartRealGame()
        {
            ClearAllTutorialBuildings();
            gameFlowManager?.ResetToStandardMode();
            ForceStopTutorial();
            PlayerPrefs.SetInt("ShowTutorial", 0);
            PlayerPrefs.Save();
        }

        public void ForceStopTutorial()
        {
            IsRunning = false;

            CancelInvoke(nameof(BeginTutorialDisplay));

            if (gameFlowManager is { IsTutorialActive: true })
            {
                ClearAllTutorialBuildings();
                gameFlowManager.IsTutorialActive = false;
                gameFlowManager?.ResetToStandardMode();
            }

            if (dialogueAnimator)
                dialogueAnimator.StopDialogue();

            if (tutorialFadePanel)
                tutorialFadePanel.Hide();

            if (highlightEffect)
                highlightEffect.SetActive(false);
        }

        public void TryStartTutorialFromScratch()
        {
            if (CastleSystem.Instance == null)
                return;

            var hasBuildings = CastleSystem.Instance != null && CastleSystem.Instance.Model.Buildings.Count > 0 ||
                               TowerSystem.Instance != null && TowerSystem.Instance.GetTowers().Count > 0 ||
                               TrapSystem.Instance != null && TrapSystem.Instance.GetTraps().Count > 0;

            if (hasBuildings)
            {
                Debug.Log("Нельзя запустить туториал: на поле уже есть постройки!");
                return;
            }

            IsRunning = true;

            if (gameFlowManager != null)
            {
                gameFlowManager.ResetToStandardMode();
                gameFlowManager.IsTutorialActive = true;

                currentStep = TutorialStep.Greeting;

                if (transform.parent != null)
                    transform.parent.gameObject.SetActive(true);

                gameObject.SetActive(true);

                CancelInvoke(nameof(BeginTutorialDisplay));
                Invoke(nameof(BeginTutorialDisplay), startDelay);
            }
        }

        private void BeginTutorialDisplay()
        {
            if (tutorialFadePanel)
                tutorialFadePanel.Show();

            UpdateTutorialState();
        }

        private enum TutorialStep
        {
            Greeting,
            BuildBarrack,
            BarrackSuccess,
            BuildTower,
            TowerSuccess,
            BuildTrap,
            HelpExplanation,
            PauseExplanation,
            SpeedExplanation,
            ClickExplanation,
            Finish
        }
    }
}