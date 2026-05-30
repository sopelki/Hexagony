using System;
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
        private GameObject barrackSlot, towerSlot, trapSlot;
        [SerializeField]
        private GameObject highlightEffect;

        [Header("Настройки задержки")]
        [SerializeField]
        private float startDelay = 1.5f;

        private bool barrackTracked, towerTracked, trapTracked;
        private TutorialStep currentStep = TutorialStep.Greeting;
        private GameFlowManager gameFlowManager;

        private void Start()
        {
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
                        currentStep = TutorialStep.Finish;
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
            return tutorial && tutorial.isActiveAndEnabled;
        }

        public void OnActionButtonClick()
        {
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

            switch (currentStep)
            {
                case TutorialStep.Greeting:
                    ConfigureButton(true, "Далее");
                    PrintPhrase("Здравствуйте, властитель Гексагонии! Ваш замок атакуют монстры.");
                    break;

                case TutorialStep.BuildBarrack:
                    ConfigureButton(false);
                    PrintPhrase("Давайте начнём строить! Перетяните казарму в поле замка.");
                    ApplyHighlight(barrackSlot);
                    break;

                case TutorialStep.BarrackSuccess:
                    ConfigureButton(true, "Далее");
                    PrintPhrase("Отлично, казарма готова! Теперь у вас есть верные рыцари.");
                    break;

                case TutorialStep.BuildTower:
                    ConfigureButton(false);
                    PrintPhrase("Защита периметра превыше всего! Давайте выберем\nи поставим Башню.");
                    ApplyHighlight(towerSlot);
                    break;

                case TutorialStep.TowerSuccess:
                    ConfigureButton(true, "Далее");
                    PrintPhrase("Защита установлена!\nМилорд, вы отлично справляетесь.");
                    break;

                case TutorialStep.BuildTrap:
                    ConfigureButton(false);
                    PrintPhrase("Остался последний штрих. Установите Ловушку, чтобы замедлить врагов.");
                    ApplyHighlight(trapSlot);
                    break;

                case TutorialStep.Finish:
                    ConfigureButton(true, "К игре!");
                    PrintPhrase("Теперь вы знаете как защитить замок.\nНачнём настоящий бой!");
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
        }

        public void ForceStopTutorial()
        {
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
            Finish
        }
    }
}