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

        [Header("Monsters")]
        [SerializeField]
        private List<MonsterData> monstersList;

        private const string CGold = "#FFD54F";
        private const string CKey = "#dfe88b";
        private const string CDmg = "#EF5350";
        private const string CSpd = "#FF7733";
        private const string CHlp = "#66BB6A";
        private const string CMag = "#AB47BC";
        private const string CUpgrade = "#90CAF9";

        private const string StartLh = "<line-height=115%>";
        private const string EndLh = "</line-height>";
        private const string ParaGap = "\n\n";

        public bool IsOpen => helpPanel != null && helpPanel.GetComponent<CanvasGroup>().alpha > 0.5f;

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

        public void ShowMechanics()
        {
            UpdateTabs(tabMechanicsButton);
            textBackground.Show();
            titleText.text = $"<color={CGold}>ОСНОВЫ ИГРЫ</color>";

            descriptionText.text = StartLh +
                                   $"<b>Ваша миссия:\n</b> Не дать врагам прорваться к замку. Каждый монстр, достигший цели, снижает прочность врат. Если замок падет - <color={CDmg}>игра будет окончена</color>." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Подготовка:\n</b></color> В начале игры время остановлено. Вы можете спокойно изучить карту. Как только вы <color={CKey}><nobr>разместите первый объект</nobr></color>, начнется отсчет до первой волны." +
                                   ParaGap +
                                   $"▪ <color={CUpgrade}><b>УЛУЧШЕНИЕ БАШЕН:\n</b></color> Мощь вашей обороны можно наращивать. Чтобы повысить уровень башни (до 5-го), <color={CKey}>купите в магазине</color> такую же и <nobr>перетащите её сверху</nobr> на уже установленную. Это значительно <nobr>увеличит <color={CDmg}>урон</color></nobr> и дальность стрельбы." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Ресурсы:\n</b></color> <b>Золото</b> - валюта для всех построек. Оно выдается мгновенно <nobr>за каждого</nobr> убитого монстра." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Управление временем:\n</b></color> Клавиша <color={CUpgrade}><b>Пробел</b></color> позволяет <nobr>ускорить игру в 2 раза</nobr>, если ситуация на поле под контролем." +
                                   EndLh;
        }

        public void ShowCastle()
        {
            UpdateTabs(tabCastleButton);
            textBackground.Show();
            titleText.text = $"<color={CHlp}>ЭКОНОМИКА ЗАМКА</color>";

            descriptionText.text = StartLh +
                                   "Здания во внутреннем дворе (сетка 4х4) обеспечивают вашу армию пассивными бонусами и подкреплением" +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Казарма:\n</b></color> Автоматически призывает рыцарей. Каждая новая казарма значительно <color={CSpd}>ускоряет время появления</color> новых воинов." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Ферма:\n</b></color> Поставляет провизию. Каждая ферма <nobr>увеличивает <b>лимит населения</b></nobr>, позволяя содержать больше рыцарей на поле." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Кузница:\n</b></color> Кует превосходное оружие. Дает <nobr>постоянную прибавку</nobr> к <color={CDmg}>силе атаки</color> для всей вашей армии." +
                                   ParaGap +
                                   $"▪ <color={CKey}><b>Алхимик:\n</b></color> Готовит целебные отвары, повышая <color={CHlp}>максимальное здоровье</color> <nobr>всех живых защитников</nobr>." +
                                   EndLh;
        }

        public void ShowField()
        {
            UpdateTabs(tabFieldButton);
            textBackground.Show();
            titleText.text = $"<color={CDmg}>ОБОРОНА ПОЛЯ</color>";

            descriptionText.text = StartLh + $"<color={CUpgrade}><b>БАШНИ</b></color>" + "\n" +
                                   $"▪ <color={CKey}><b>Лучник:\n</b></color> Недорогое здание с высокой <color={CSpd}>скорострельностью</color>. Идеально против одиночных целей." +
                                   "\n" +
                                   $"▪ <color={CKey}><b>Маг:\n</b></color> Выпускает сферы, наносящие <color={CMag}>урон по области</color>. Эффективен против плотных скоплений врага." +
                                   ParaGap + $"<color={CUpgrade}><b>ЛОВУШКИ</b></color>" + "\n" +
                                   $"▪ <color={CKey}><b>Лоза:\n</b></color> Поле магических растений, которые <color={CUpgrade}>замедляют</color> всех монстров, проходящих сквозь них." +
                                   "\n" +
                                   $"▪ <color={CKey}><b>Колья:\n</b></color> Ряды острых шипов. Наносят <color={CDmg}>периодический урон</color> любому, кто наступит на клетку." +
                                   "\n" +
                                   $"▪ <color={CKey}><b>Капкан:\n</b></color> Мощное механическое устройство. Наносит <color={CDmg}>критический урон</color> одной цели и требует времени на перезарядку." +
                                   EndLh;
        }

        public void ShowMonsters()
        {
            UpdateTabs(tabMonsterButton);
            textBackground.Show();
            titleText.text = $"<color={CDmg}>БЕСТИАРИЙ</color>";

            var (hMult, dMult, gMult) = DifficultyManager.GetCurrentMultipliers();
            var resultText = StartLh + "<size=80%><i>Текущие характеристики врагов:</i></size>" + ParaGap;

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
        }

        private void Start()
        {
            tabMechanicsButton?.onClick.AddListener(ShowMechanics);
            tabCastleButton?.onClick.AddListener(ShowCastle);
            tabFieldButton?.onClick.AddListener(ShowField);
            tabMonsterButton?.onClick.AddListener(ShowMonsters);
        }

        private void UpdateTabs(Button activeButton = null)
        {
            tabMechanicsButton.interactable = true;
            tabCastleButton.interactable = true;
            tabFieldButton.interactable = true;
            tabMonsterButton.interactable = true;
            if (activeButton) activeButton.interactable = false;
        }
    }
}