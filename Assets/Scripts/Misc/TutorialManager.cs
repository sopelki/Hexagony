using System;
using Core;
using Logic.Castle;
using Logic.Tower;
using Logic.Trap;
using Logic.Unit;
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
        private GameObject tutorialWindow;
        [SerializeField]
        private DialogueAnimator dialogueAnimator;
        [SerializeField]
        private Button actionButton;
        [SerializeField]
        private TMPro.TextMeshProUGUI actionButtonText;
        [SerializeField]
        private GameObject barrackSlot, towerSlot, trapSlot;
        [SerializeField]
        private GameObject highlightEffect;
        [SerializeField]
        private float startDelay = 10f;

        private enum TutorialStep
        {
            Greeting,
            BuildBarrack,
            BuildTower,
            BuildTrap,
            Finish
        }

        private TutorialStep currentStep = TutorialStep.Greeting;

        private GameFlowManager gameFlowManager;
        private bool barrackTracked, towerTracked, trapTracked;

        public void Setup(GameFlowManager flowManager) => gameFlowManager = flowManager;

        private void Start()
        {
            if (actionButton)
                actionButton.onClick.AddListener(OnActionButtonClick);

            UpdateTutorialState();
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
                    UpdateTutorialState();
                    break;

                case TutorialStep.Finish:
                    FinishTutorialAndStartRealGame();
                    break;

                case TutorialStep.BuildBarrack:
                case TutorialStep.BuildTower:
                case TutorialStep.BuildTrap:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            if (currentStep == TutorialStep.BuildBarrack && !barrackTracked)
            {
                if (CastleSystem.Instance != null && CastleSystem.Instance.Model.Buildings.Count > 0)
                {
                    barrackTracked = true;
                    NextStepAfterBuild("Отлично, барак готов! Теперь выберите Башню.", TutorialStep.BuildTower);
                }
            }

            if (currentStep == TutorialStep.BuildTower && !towerTracked)
            {
                if (TowerSystem.Instance != null && TowerSystem.Instance.GetTowers().Count > 0)
                {
                    towerTracked = true;

                    // if (highlightEffect)
                    //     highlightEffect.SetActive(false);

                    NextStepAfterBuild("Защита установлена! Последний штрих — Ловушка.", TutorialStep.BuildTrap);
                }
            }

            if (currentStep == TutorialStep.BuildTrap && !trapTracked)
            {
                if (TrapSystem.Instance != null && TrapSystem.Instance.GetTraps().Count > 0)
                {
                    trapTracked = true;
                    currentStep = TutorialStep.Finish;
                    UpdateTutorialState();
                }
            }
        }

        private void NextStepAfterBuild(string text, TutorialStep nextStep)
        {
            if (tutorialWindow)
                tutorialWindow.SetActive(true);

            if (actionButton)
                actionButton.gameObject.SetActive(false);

            if (dialogueAnimator)
                dialogueAnimator.PrintPhrase(text);

            Invoke(nameof(ExecuteNextStep), 0f);
            currentStep = nextStep;
        }

        private void ExecuteNextStep() => UpdateTutorialState();

        private void UpdateTutorialState()
        {
            if (highlightEffect)
                highlightEffect.SetActive(false);

            if (tutorialWindow && currentStep is TutorialStep.Greeting or TutorialStep.Finish)
                tutorialWindow.SetActive(true);

            switch (currentStep)
            {
                case TutorialStep.Greeting:
                    if (actionButton)
                        actionButton.gameObject.SetActive(true);

                    if (actionButtonText)
                        actionButtonText.text = "Далее";

                    if (dialogueAnimator)
                    {
                        dialogueAnimator.PrintPhrase(
                            "Лорд, давайте начнём строительство! Перетяните казарму в поле замка");
                    }

                    break;

                case TutorialStep.BuildBarrack:
                    // if (tutorialWindow != null) tutorialWindow.SetActive(false);
                    if (highlightEffect && barrackSlot != null)
                    {
                        highlightEffect.transform.position = barrackSlot.transform.position;
                        highlightEffect.SetActive(true);
                    }
                    break;

                case TutorialStep.BuildTower:
                    // if (tutorialWindow != null) tutorialWindow.SetActive(false);
                    if (highlightEffect && towerSlot != null)
                    {
                        highlightEffect.transform.position = towerSlot.transform.position;
                        highlightEffect.SetActive(true);
                    }
                    break;

                case TutorialStep.BuildTrap:
                    // if (tutorialWindow != null) tutorialWindow.SetActive(false);
                    if (highlightEffect && trapSlot != null)
                    {
                        highlightEffect.transform.position = trapSlot.transform.position;
                        highlightEffect.SetActive(true);
                    }
                    break;

                case TutorialStep.Finish:
                    if (actionButton)
                        actionButton.gameObject.SetActive(true);

                    if (actionButtonText)
                        actionButtonText.text = "К игре!";

                    if (dialogueAnimator)
                        dialogueAnimator.PrintPhrase("Сборка завершена. Нажмите кнопку, чтобы начать настоящий бой!");

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FinishTutorialAndStartRealGame()
        {
            if (gameFlowManager != null)
                gameFlowManager.IsTutorialActive = false;

            if (CastleSystem.Instance != null)
            {
                CastleSystem.Instance.Clear();
                foreach (var b in FindObjectsByType<InventoryItem>(FindObjectsInactive.Exclude))
                    Destroy(b.gameObject);
            }
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


            gameFlowManager?.ResetToStandardMode();

            if (tutorialWindow)
                tutorialWindow.SetActive(false);

            transform.parent.gameObject.SetActive(false);
        }
    }
}