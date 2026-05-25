using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Misc;

namespace MenuScripts
{
    public class HelpMenu : MonoBehaviour
    {
        [Header("Панель справки")]
        [SerializeField]
        private FadePanel helpPanel;
        [SerializeField]
        private FadePanel menuBackground;

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

        [Header("Настройки увеличения")]
        [SerializeField]
        private Vector3 activeScale = new(1.15f, 1.15f, 1f);
        [SerializeField]
        private Vector3 normalScale = new(1f, 1f, 1f);

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

            helpPanel.Show();

            Time.timeScale = 0f;

            ShowMechanics();
        }

        public void CloseHelp()
        {
            UIBlocker.UnblockAll();

            if (menuBackground != null)
                menuBackground.Hide();

            if (helpPanel != null)
                helpPanel.Hide();

            Time.timeScale = 1f;
        }

        public void ShowMechanics()
        {
            titleText.text = "<color=#FFD54F>ОСНОВЫ ИГРЫ</color>";
            descriptionText.text =
                "<b>Главная цель:</b> Защитить замок от наступающих волн монстров.\n\n" +
                "• Игра начнётся, как только вы разместите свою <b>первую постройку</b>.\n" +
                "• Уничтожайте врагов, получайте за них <b>деньги</b> и тратьте их на оборону.\n" +
                "• Характеристики объектов можно увидеть при <i>наведении курсора</i> в магазинах.";
        }

        public void ShowCastle()
        {
            titleText.text = "<color=#66BB6A>ЭКОНОМИКА ЗАМКА (Сетка 3х3)</color>";
            descriptionText.text =
                "Здания внутри замка развивают ваше поселение и усиливают армию:\n\n" +
                "• <b>Ферма:</b> Добавляет +Х к макс. количеству жителей.\n" +
                "• <b>Бараки:</b> Производят нового жителя автоматически за Х секунд.\n" +
                "• <b>Кузнец:</b> Даёт пассивный бафф к урону вашей армии.\n" +
                "• <b>Лекарь:</b> Увеличивает максимальный запас HP юнитов.\n\n" +
                "<i>Показатели строений всегда видны в поле <color=#FFD54F>«Статистика»</color>.</i>";
        }

        public void ShowField()
        {
            titleText.text = "<color=#EF5350>ОБОРОНА ПОЛЯ (Башни и Ловушки)</color>";
            descriptionText.text =
                "<b>БАШНИ</b> (Строятся в специальные слоты):\n" +
                "• <b>Маг:</b> Медленная АОЕ-атака магическими сферами.\n" +
                "• <b>Лучник:</b> Быстрый обстрел одиночных целей стрелами.\n\n" +
                "<b>ЛОВУШКИ</b> (Размещаются на дороге):\n" +
                "• <b>Лоза:</b> Оплетает чудовищ, замедляя их скорость.\n" +
                "• <b>Колья:</b> Стабильно ранят врагов, пока они стоят на них.\n" +
                "• <b>Капкан:</b> Наносит огромный разовый урон и исчезает.\n\n" +
                "<i><color=#66BB6A>Зелёный цвет подсветки</color> — строить можно. <color=#EF5350>Красный</color> — нельзя.</i>";
        }
    }
}