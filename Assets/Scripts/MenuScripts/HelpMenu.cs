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
        [Header("Панель справки")] [SerializeField]
        private FadePanel helpPanel;

        [SerializeField] private FadePanel menuBackground;
        [SerializeField] private FadePanel textBackground;

        [Header("Текстовые поля (TextMeshPro)")] [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Кнопки вкладок (Ярлычки)")] [SerializeField]
        private Button tabMechanicsButton;

        [SerializeField] private Button tabCastleButton;
        [SerializeField] private Button tabFieldButton;
        [SerializeField] private Button tabMonsterButton;
        
        [Header("Монстры")]
        [SerializeField] private List<MonsterData> monstersList;

        public bool IsOpen => helpPanel != null && helpPanel.GetComponent<CanvasGroup>().alpha > 0.5f;

        private void Start()
        {
            if (tabMechanicsButton != null)
                tabMechanicsButton.onClick.AddListener(ShowMechanics);

            if (tabCastleButton != null)
                tabCastleButton.onClick.AddListener(ShowCastle);

            if (tabFieldButton != null)
                tabFieldButton.onClick.AddListener(ShowField);

            if (tabMonsterButton != null)
                tabMonsterButton.onClick.AddListener(ShowMonsters);
        }

        public void OpenHelp()
        {
            UIBlocker.BlockAll();

            if (helpPanel == null)
                helpPanel = GetComponent<FadePanel>();

            if (menuBackground != null)
                menuBackground.Show();

            helpPanel.Show();
            ShowMechanics();

            Time.timeScale = 0f;
        }

        public void CloseHelp()
        {
            UIBlocker.UnblockAll();

            if (menuBackground)
                menuBackground.Hide();

            if (helpPanel)
                helpPanel.Hide();

            if (textBackground)
                textBackground.Hide();

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
            titleText.text = "<color=#FFD54F>ОСНОВЫ ИГРЫ</color>";
            descriptionText.text =
                "<b>Ваша миссия:</b> Не дать монстрам прорваться к воротам замка.\n" +
                "▪ <color=#dfe88b><b>Подготовка:</b></color> Время застыло, пока вы <nobr>не разместите</nobr> свою первую постройку.\n" +
                "▪ <color=#dfe88b><b>Ресурсы:</b></color> <b>Золото</b> добывается <nobr>за уничтожение</nobr> врагов.\n" +
                "▪ <color=#dfe88b><b>Информация:</b></color> <nobr>Наведите курсор</nobr> <nobr>на объект, чтобы</nobr> увидеть его <color=#EF5350>урон </color><nobr>и <color=#FF7733>скорость</color></nobr>.\n" +
                "▪ Если захотите  <color=#dfe88b>ускорить игру</color> — нажмите <color=#FFEE58>Пробел</color>. Чтобы вернуть, как было нажмите снова.";
        }

        public void ShowCastle()
        {
            UpdateTabs(tabCastleButton);
            textBackground.Show();
            titleText.text = "<color=#66BB6A>ЭКОНОМИКА ЗАМКА</color>";
            descriptionText.text =
                "Здания во внутреннем дворе (сетка 3х3) определяют мощь вашей армии:\n" +
                "▪ <color=#dfe88b><b>Казарма:</b></color> Автоматически нанимает рыцарей для защиты стен.\n" +
                "▪ <color=#dfe88b><b>Ферма:</b></color> Увеличивает <b>лимит населения</b> для содержания армии.\n" +
                "▪ <color=#dfe88b><b>Кузница:</b></color> Повышает <color=#EF5350>урон</color> ваших войск через улучшение стали.\n" +
                "▪ <color=#dfe88b><b>Алхимик:</b></color> Увеличивает <color=#ade6a3>максимальное здоровье</color> всех защитников.\n";
        }

        public void ShowField()
        {
            UpdateTabs(tabFieldButton);
            textBackground.Show();
            titleText.text = "<color=#EF5350>ОБОРОНА ПОЛЯ</color>";
            descriptionText.text =
                "<color=#90CAF9><b>БАШНИ</b></color>:\n" +
                "▪ <color=#dfe88b><b>Маг:</b></color> Атакует магическими сферами <color=#AB47BC><nobr>по области (AoE)</nobr></color>.\n" +
                "▪ <color=#dfe88b><b>Лучник:</b></color> Высокая <color=#FF7733>скорострельность </color><nobr>по одиночным</nobr> целям.\n" +
                "<color=#90CAF9><b>ЛОВУШКИ</b></color>:\n" +
                "▪ <color=#dfe88b><b>Лоза:</b></color> Оплетает монстров, значительно замедляя их ход.\n" +
                "▪ <color=#dfe88b><b>Колья:</b></color> Наносят стабильный <b>урон </b><nobr>всем, кто</nobr> стоит на них.\n" +
                "▪ <color=#dfe88b><b>Капкан:</b></color> Наносит <b>критический удар </b><nobr>и исчезает.</nobr>\n";
        }
        
        public void ShowMonsters()
        {
            UpdateTabs(tabMonsterButton);
            textBackground.Show();
            titleText.text = "<color=#EF5350>МОНСТРЫ</color>";
            
            var (hMult, dMult, gMult) = DifficultyManager.GetCurrentMultipliers();
    
            var resultText = "";

            foreach (var monster in monstersList)
            {
                var hp = Mathf.RoundToInt(monster.maxHealth * hMult);
                var dmg = Mathf.RoundToInt(monster.damage * dMult);
                var reward = Mathf.RoundToInt(monster.goldReward * gMult);

                resultText += $"<color=#90CAF9><b>{monster.monsterName.ToUpper()}</b></color>\n" +
                              $"<b>{monster.monsterDescription}</b>\n" +
                              "<line-height=60%>" +
                              $"▪️ <color=#dfe88b><b>Здоровье:</b></color> {hp}\n" +
                              $"▪️ <color=#dfe88b><b>Урон:</b></color> {dmg}\n" +
                              $"▪️ <color=#dfe88b><b>Награда:</b></color> {reward}\n" +
                              "</line-height>" +
                              "<line-height=-15px>\n</line-height>";
            }

            descriptionText.text = resultText;
            Canvas.ForceUpdateCanvases();
        }
    }
}