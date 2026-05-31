using Misc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuScripts
{
    public class HelpMenu : MonoBehaviour
    {
        [Header("Панель справки")]
        [SerializeField]
        private FadePanel helpPanel;
        [SerializeField]
        private FadePanel menuBackground;
        [SerializeField]
        private FadePanel textBackground;

        [Header("Текстовые поля (TextMeshPro)")]
        [SerializeField]
        private TextMeshProUGUI titleText;
        [SerializeField]
        private TextMeshProUGUI descriptionText;

        [Header("Кнопки вкладок (Ярлычки)")]
        [SerializeField]
        private Button tabMechanicsButton;
        [SerializeField]
        private Button tabCastleButton;
        [SerializeField]
        private Button tabFieldButton;

        public bool IsOpen => helpPanel != null && helpPanel.GetComponent<CanvasGroup>().alpha > 0.5f;

        private void Start()
        {
            if (tabMechanicsButton != null)
                tabMechanicsButton.onClick.AddListener(ShowMechanics);

            if (tabCastleButton != null)
                tabCastleButton.onClick.AddListener(ShowCastle);

            if (tabFieldButton != null)
                tabFieldButton.onClick.AddListener(ShowField);
        }

        public void OpenHelp()
        {
            UIBlocker.BlockAll();

            if (helpPanel == null)
                helpPanel = GetComponent<FadePanel>();

            if (menuBackground != null)
                menuBackground.Show();

            titleText.text = "";
            descriptionText.text = "";

            helpPanel.Show();

            Time.timeScale = 0f;
        }

        public void CloseHelp()
        {
            UIBlocker.UnblockAll();

            if (menuBackground != null)
                menuBackground.Hide();

            if (helpPanel != null)
                helpPanel.Hide();

            if (textBackground != null)
                textBackground.Hide();

            UpdateTabs();
            Time.timeScale = 1f;
        }

        private void UpdateTabs(Button activeButton = null)
        {
            tabMechanicsButton.interactable = true;
            tabCastleButton.interactable = true;
            tabFieldButton.interactable = true;

            if (activeButton != null)
                activeButton.interactable = false;
        }

        public void ShowMechanics()
        {
            UpdateTabs(tabMechanicsButton);
            textBackground.Show();
            titleText.text = "<color=#FFD54F>ОСНОВЫ ИГРЫ</color>";
            descriptionText.text =
                "<b>Ваша миссия:</b> Не дать монстрам прорваться к воротам замка.\n" +
                "▪ <color=#dfe88b><b>Подготовка:</b></color> Время застыло, пока вы не разместите свою первую постройку.\n" +
                "▪ <color=#dfe88b><b>Ресурсы:</b></color> <b>Золото</b> добывается <nobr>за уничтожение</nobr> врагов.\n" +
                "▪ <color=#dfe88b><b>Информация:</b></color> Наведите <nobr>курсор на</nobr> объект, чтобы увидеть его <nobr><color=#EF5350>урон </color></nobr>и <color=#FF7733>скорость</color>.\n" +
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
                "▪ <color=#dfe88b><b>Маг:</b></color> Атакует магическими <nobr>сферами <color=#AB47BC>по</nobr> области (AoE)</color>.\n" +
                "▪ <color=#dfe88b><b>Лучник:</b></color> Высокая <color=#FF7733>скорострельность </color><nobr>по одиночным</nobr> целям.\n" +
                "<color=#90CAF9><b>ЛОВУШКИ</b></color>:\n" +
                "▪ <color=#dfe88b><b>Лоза:</b></color> Оплетает монстров, значительно замедляя их ход.\n" +
                "▪ <color=#dfe88b><b>Колья:</b></color> Наносят стабильный <b>урон </b><nobr>всем, кто</nobr> стоит на них.\n" +
                "▪ <color=#dfe88b><b>Капкан:</b></color> Наносит <b>критический удар </b><nobr>и исчезает.</nobr>\n";
        }
    }
}