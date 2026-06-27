using System.Collections.Generic;
using Core;
using Logic.Monster;
using Misc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuScripts
{
    public class HelpMenu : MonoBehaviour
    {
        private const string CGold = "#FFD54F";
        private const string CKey = "#dfe88b";
        private const string CDmg = "#EF5350";
        private const string CSpd = "#FF7733";
        private const string CHlp = "#66BB6A";
        private const string CMag = "#AB47BC";
        private const string CUpgrade = "#90CAF9";

        private const string StartLh = "<line-height=115%>";
        private const string EndLh = "</line-height>";
        private const string ParaGap = "\n<line-height=10%>\n</line-height><line-height=115%>";
        private const string HeaderGap = "\n<size=110%>\n</size>";

        [Header("Panel")]
        [SerializeField]
        private FadePanel helpPanel;
        [SerializeField]
        private FadePanel menuBackground;
        [SerializeField]
        private FadePanel textBackground;

        [Header("Text")]
        [SerializeField]
        private TextMeshProUGUI titleText;
        [SerializeField]
        private TextMeshProUGUI descriptionText;

        [Header("Tab buttons")]
        [SerializeField]
        private Button tabMechanicsButton;
        [SerializeField]
        private Button tabCastleButton;
        [SerializeField]
        private Button tabFieldButton;
        [SerializeField]
        private Button tabMonsterButton;
        [SerializeField]
        private Button tabControlButton;

        [Header("Monsters")]
        [SerializeField]
        private List<MonsterData> monstersList;
        
        [Header("Scroll")]
        [SerializeField]
        private ScrollRect helpScrollRect;

        public bool IsOpen => helpPanel != null && helpPanel.GetComponent<CanvasGroup>().alpha > 0.5f;

        private void Start()
        {
            tabMechanicsButton?.onClick.AddListener(ShowMechanics);
            tabCastleButton?.onClick.AddListener(ShowCastle);
            tabFieldButton?.onClick.AddListener(ShowField);
            tabMonsterButton?.onClick.AddListener(ShowMonsters);
        }

        public void OpenHelp()
        {
            UIBlocker.BlockAll();
            helpPanel ??= GetComponent<FadePanel>();
            menuBackground?.Show();
            helpPanel?.Show();
            ShowMechanics();
            Time.timeScale = 0f;
        }

        public void CloseHelp()
        {
            UIBlocker.UnblockAll();
            menuBackground?.Hide();
            helpPanel?.Hide();
            textBackground?.Hide();
            UpdateTabs();
            Time.timeScale = 1f;
        }

        private void UpdateTabs(Button activeButton = null)
        {
            tabMechanicsButton.interactable = true;
            tabCastleButton.interactable = true;
            tabFieldButton.interactable = true;
            tabMonsterButton.interactable = true;
            tabControlButton.interactable = true;
            if (activeButton) activeButton.interactable = false;
        }
        
        private void ResetScroll()
        {
            if (helpScrollRect != null)
                helpScrollRect.verticalNormalizedPosition = 1f;
        }

        public void ShowMechanics()
        {
            UpdateTabs(tabMechanicsButton);
            textBackground.Show();
            titleText.text = $"<color={CGold}>ОСНОВЫ ИГРЫ</color>";
        
            descriptionText.text = StartLh +
                                   $"<b>Ваша миссия:\n</b>Не дать врагам прорваться к замку. Каждый монстр, достигший <nobr>цели, снижает</nobr> прочность врат. Если замок падет - <color={CDmg}>игра будет окончена</color>." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Подготовка:\n</b></color>В начале игры время остановлено. <nobr>Вы можете</nobr> спокойно изучить карту. <nobr>Как только</nobr> вы <color={CKey}>разместите первый <nobr>объект</color>, начнется</nobr> отсчет до первой волны." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Ресурсы:\n</b></color><b>Золото</b> - валюта для всех построек. <nobr>Оно выдается</nobr> мгновенно <nobr>за каждого</nobr> убитого монстра." +
                                   EndLh;
            ResetScroll();
        }
        
        public void ShowCastle()
        {
            UpdateTabs(tabCastleButton);
            textBackground.Show();
            titleText.text = $"<color={CHlp}>ЭКОНОМИКА ЗАМКА</color>";
        
            descriptionText.text = StartLh +
                                   $"Здания во внутреннем дворе <nobr>(сетка 4х4)</nobr> обеспечивают <nobr>вашу армию</nobr> пассивными <nobr>бонусами и подкреплением</nobr>" +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Казарма:\n</b></color>Автоматически призывает рыцарей. Каждая новая казарма значительно <color={CSpd}>ускоряет время появления</color> новых воинов." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Ферма:\n</b></color>Поставляет провизию. Каждая ферма <nobr>увеличивает <b>лимит населения. Это</b></nobr> позволет содержать больше рыцарей <nobr>на поле</nobr>." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Кузница:\n</b></color>Кует превосходное оружие. Дает <nobr>постоянную прибавку</nobr> к <color={CDmg}>силе атаки</color> <nobr>для всей вашей армии.</nobr>" +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Алхимик:\n</b></color>Готовит целебные отвары, повышая <color={CHlp}>максимальное здоровье</color> всех защитников." +
                                   EndLh;
            ResetScroll();
        }
        
        public void ShowField()
        {
            UpdateTabs(tabFieldButton);
            textBackground.Show();
            titleText.text = $"<color={CDmg}>ОБОРОНА ПОЛЯ</color>";
        
            descriptionText.text = $"<color={CUpgrade}><b>БАШНИ</b></color>" + 
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Лучник:\n</b></color>Башня с высокой <color={CSpd}>скорострельностью</color>. Идеальна против одиночных целей." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Маг:\n</b></color>Выпускает сферы, наносящие <color={CMag}>урон по области</color>. Эффективен против плотных скоплений врага." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Улучшение башен:\n</b></color>Мощь вашей обороны можно наращивать. Чтобы повысить уровень башни <nobr>(до 5-го)</nobr>, <color={CKey}>купите в магазине</color> такую же <nobr>и перетащите её сверху</nobr> на уже установленную. Это значительно <nobr>увеличит <color={CDmg}>урон</color></nobr> и дальность стрельбы." +
                                   HeaderGap +
                                   $"<color={CUpgrade}><b>ЛОВУШКИ</b></color>" + 
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Лоза:\n</b></color>Поле магических растений, которые <color={CUpgrade}>замедляют</color> всех монстров, проходящих сквозь них." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Колья:\n</b></color>Ряды острых шипов. Наносят <color={CDmg}>периодический урон</color> любому, кто наступит на клетку." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Капкан:\n</b></color>Мощное механическое устройство. Срабатывает один раз и наносит цели <color={CDmg}>критический урон</color>." +
                                   EndLh;
            ResetScroll();
        }
        
        public void ShowMonsters()
        {
            UpdateTabs(tabMonsterButton);
            textBackground.Show();
            titleText.text = $"<color={CDmg}>МОНСТРЫ</color>";
        
            var (hMult, dMult, gMult) = DifficultyManager.GetCurrentMultipliers();
            var resultText = StartLh + "Текущие характеристики врагов:" + ParaGap;
        
            foreach (var monster in monstersList)
            {
                var hp = Mathf.RoundToInt(monster.maxHealth * hMult);
                var dmg = Mathf.RoundToInt(monster.damage * dMult);
                var reward = Mathf.RoundToInt(monster.goldReward * gMult);
        
                resultText += $"<color={CUpgrade}><b>{monster.monsterName.ToUpper()}</b></color>\n" +
                              $"<size=90%>{monster.monsterDescription}</size>\n" +
                              $"▪️ <color={CKey}>HP:</color> {hp} | <color={CKey}>ATK:</color> {dmg} | <color={CKey}>GOLD:</color> {reward}" +
                              ParaGap;
            }
        
            descriptionText.text = resultText + EndLh;
            Canvas.ForceUpdateCanvases();
            ResetScroll();
        }
        
        public void ShowControl()
        {
            UpdateTabs(tabControlButton);
            textBackground.Show();
            titleText.text = $"<color={CDmg}>УПРАВЛЕНИЕ</color>";
        
            descriptionText.text = StartLh +
                                   $"▪ <color={CKey}><b>Управление временем:\n</b></color>Клавиша <color={CUpgrade}><b>Пробел</b></color> <nobr>позволяет ускорить</nobr> игру в 2 раза, если ситуация на поле <nobr>под контролем.</nobr>" +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Подсказки:\n</b></color>Наведитесь на любой предмет <nobr>в магазине</nobr> и прочитайте его характеристики и краткое описание. " +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Клики по монстрам:\n</b></color>Наведитесь на врага и кликайте, он будет получать урон." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Кнопки:\n</b></color>Чтобы остановить игру, начать заново или вернуться обратно <nobr>в главное</nobr> меню, нажмите кнопку <color={CUpgrade}><b>Пауза</b></color>.\nОткройте <color={CUpgrade}><b>Настройки</b></color>, чтобы включить или отключить проигрывание туториала, настроить звук и визуал игры." +
                                   EndLh;
            ResetScroll();
        }
    }
}