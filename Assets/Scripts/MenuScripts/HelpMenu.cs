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
        private const string CGold = "#FFD54F"; // Заголовки
        private const string CKey = "#dfe88b"; // Ключевые слова / Подзаголовки
        private const string CDmg = "#EF5350"; // Урон / Опасность
        private const string CSpd = "#FF7733"; // Скорость
        private const string CHlp = "#66BB6A"; // Здоровье / Фермы
        private const string CMag = "#AB47BC"; // Магия / Область
        private const string CUpgrade = "#90CAF9"; // Улучшения / Синее

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
            if (activeButton)
                activeButton.interactable = false;
        }

        public void ShowMechanics()
        {
            UpdateTabs(tabMechanicsButton);
            textBackground.Show();
            titleText.text = $"<color={CGold}>ОСНОВЫ ИГРЫ</color>";
            descriptionText.text =
                $"<b>Ваша миссия:</b> Защитить ворота замка. Каждый прорвавшийся монстр наносит урон стенам. Если прочность упадет до нуля — <color={CDmg}>игра будет окончена</color>.\n\n" +
                $"▪ <color={CKey}><b>Подготовка:</b></color> В начале игры время застыло. Это ваш шанс обдумать стратегию. Игра начнется автоматически, как только вы <color={CKey}><nobr>разместите первый объект</nobr></color>.\n\n" +
                $"▪ <color={CKey}><b>Улучшение башен:</b></color> Вы можете повышать уровень ваших башен (макс. 5 уровень). Для этого <color={CKey}>перетащите новую башню</color> из магазина прямо на уже <nobr>установленную того же типа</nobr>. Улучшенная башня получает значительный <nobr>бонус к <color={CDmg}>урону</color></nobr>, <color={CSpd}>скорострельности</color> и дальности.\n\n" +
                $"▪ <color={CKey}><b>Экономика:</b></color> <b>Золото</b> — ваш главный ресурс. Оно добывается <nobr>за уничтожение</nobr> врагов и необходимо для <nobr>строительства и улучшений</nobr>.\n\n" +
                $"▪ <color={CKey}><b>Ускорение:</b></color> Если оборона надежна и вы не хотите ждать, нажмите <color={CKey}>Пробел</color>, чтобы <nobr>ускорить течение времени</nobr>. Повторное нажатие вернет обычную скорость.";
        }

        public void ShowCastle()
        {
            UpdateTabs(tabCastleButton);
            textBackground.Show();
            titleText.text = $"<color={CHlp}>ЭКОНОМИКА ЗАМКА</color>";
            descriptionText.text =
                $"Внутренний двор замка (сетка 4х4) предназначен для тыловых зданий. Они не атакуют сами, но дают <color={CUpgrade}>усиления</color>:\n\n" +
                $"▪ <color={CKey}><b>Казарма:</b></color> Тренирует рыцарей, которые атакуют врагов. Каждая новая казарма <nobr>сокращает <color={CSpd}>время появления</color></nobr> новых бойцов.\n\n" +
                $"▪ <color={CKey}><b>Ферма:</b></color> Обеспечивает провизию. Увеличивает <b>лимит населения</b>, позволяя вам <nobr>содержать более многочисленную</nobr> армию.\n\n" +
                $"▪ <color={CKey}><b>Кузница:</b></color> Улучшает снаряжение. Дает <nobr>постоянную прибавку</nobr> к <color={CDmg}>силе атаки</color> для всех ваших воинов.\n\n" +
                $"▪ <color={CKey}><b>Алхимик:</b></color> Варит зелья, увеличивающие <color={CHlp}>максимальный запас здоровья</color> <nobr>всех живых защитников</nobr> на поле.";
        }

        public void ShowField()
        {
            UpdateTabs(tabFieldButton);
            textBackground.Show();
            titleText.text = $"<color={CDmg}>ОБОРОНА ПОЛЯ</color>";
            descriptionText.text =
                $"<color={CUpgrade}><b>БАШНИ</b></color> (Можно улучшать до 5 уровня):\n" +
                $"▪ <color={CKey}><b>Лучник:</b></color> Базовая оборона. Обладает отличной <color={CSpd}>скорострельностью</color> и эффективно <nobr>устраняет одиночные цели</nobr>.\n" +
                $"▪ <color={CKey}><b>Маг:</b></color> Обрушивает на врагов магические сферы. Наносит <color={CMag}>урон по области (AoE)</color>, что незаменимо против толп мелких монстров.\n\n" +
                $"<color={CUpgrade}><b>ЛОВУШКИ</b></color> (Размещаются на пути следования):\n" +
                $"▪ <color={CKey}><b>Лоза:</b></color> Магические растения, которые <nobr>значительно <color={CUpgrade}>замедляют</color></nobr> монстров, подставляя их под стрелы башен.\n" +
                $"▪ <color={CKey}><b>Колья:</b></color> Преграда, которая наносит урон <nobr>всем противникам</nobr>, пока они находятся на этой клетке.\n" +
                $"▪ <color={CKey}><b>Капкан:</b></color> Механическая ловушка. Срабатывает один раз, нанося <color={CDmg}>огромный критический урон</color> одиночной цели, после чего исчезает.";
        }

        public void ShowMonsters()
        {
            UpdateTabs(tabMonsterButton);
            textBackground.Show();
            titleText.text = $"<color={CDmg}>БЕСТИАРИЙ</color>";

            var (hMult, dMult, gMult) = DifficultyManager.GetCurrentMultipliers();
            var resultText = "<size=80%><i>Показатели монстров адаптированы под текущую сложность:</i></size>\n\n";

            foreach (var monster in monstersList)
            {
                var hp = Mathf.RoundToInt(monster.maxHealth * hMult);
                var dmg = Mathf.RoundToInt(monster.damage * dMult);
                var reward = Mathf.RoundToInt(monster.goldReward * gMult);

                resultText += $"<color={CUpgrade}><b>{monster.monsterName.ToUpper()}</b></color>\n" +
                              $"<size=90%>{monster.monsterDescription}</size>\n" +
                              $"▪️ <color={CKey}>Здоровье:</color> {hp} | <color={CKey}>Урон:</color> {dmg} | <color={CKey}>Награда:</color> {reward}\n\n";
            }

            descriptionText.text = resultText;
            Canvas.ForceUpdateCanvases();
        }
    }
}