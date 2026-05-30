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

            // ShowMechanics();
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
                "<b>Ваша миссия:</b> Не дать монстрам прорваться к воротам замка.\n\n" +
                "• <color=#FFD54F><b>Подготовка:</b></color> Время застыло, пока вы не разместите свою <b>первую постройку</b>. Используйте это, чтобы спланировать оборону.\n" +
                "• <color=#FFEE58><b>Ресурсы:</b></color> Золото добывается за уничтожение врагов. Тратьте его мудро — на развитие замка и на укрепление рубежей.\n" +
                "• <color=#FFA726><b>Информация:</b></color> Наведите курсор на любой объект в магазине, чтобы увидеть его <color=#FF7733>параметры и описание</color>.";
        }

        public void ShowCastle()
        {
            UpdateTabs(tabCastleButton);
            textBackground.Show();
            titleText.text = "<color=#66BB6A>ЗАМОК</color>";
            descriptionText.text =
                "Правильная комбинация зданий на внутреннем дворе замка (сетка 3х3) определяет исход войны:\n\n" +
                "• <color=#666fd9><b>Казарма:</b></color> Вербует новых рыцарей, которые будут защищать ваш замок.\n" +
                "• <color=#dfe665><b>Ферма:</b></color> Увеличивает максимальное количество рыцарей, которые могут выходить на поле бое.\n" +
                "• <color=#EF5350><b>Кузница:</b></color> Улучшает оружие, повышая <color=#EF5350>урон</color> ваших войск.\n" +
                "• <color=#42A5F5><b>Алхимик:</b></color> Повышает выживаемость армии, увеличивая их <color=#42A5F5>максимальное здоровье</color>.\n\n" +
                "<i>Совет: Место в замке ограничено, а убрать построенное здание уже нельзя. Стройте только то, что необходимо прямо сейчас.</i>";
        }

        public void ShowField()
        {
            UpdateTabs(tabFieldButton);
            textBackground.Show();
            titleText.text = "<color=#EF5350>ПОЛЕ БОЯ</color>";
            descriptionText.text =
                "<color=#FFA726><b>БАШНИ</b></color> (Размещаются в бирюзовые слоты):\n" +
                "• <b>Маг:</b> Обрушивает магические сферы. Бьёт медленно, но <color=#AB47BC>по области (AoE)</color>.\n" +
                "• <b>Лучник:</b> Высокая скорострельность. Идеален против <color=#EF5350>быстрых одиночных целей</color>.\n\n" +
                "<color=#FF7733><b>ЛОВУШКИ</b></color> (Устанавливаются прямо на дорогу):\n" +
                "• <color=#66BB6A><b>Лоза:</b></color> Сковывает движения монстров, значительно <color=#66BB6A>замедляя</color> их.\n" +
                "• <color=#BDBDBD><b>Колья:</b></color> Наносят периодический урон всем, кто по ним проходит.\n" +
                "• <color=#EF5350><b>Капкан:</b></color> Смертоносная ловушка для мощных врагов. <color=#EF5350>Огромный урон</color>, но ломается после активации.\n\n" +
                "<i>Внимательно следите за цветом проекции при строительстве!</i>";
        }
    }
}