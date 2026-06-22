using TMPro;
using UnityEngine;

namespace View
{
    public class TowerView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;
        [SerializeField]
        private TextMeshPro levelText;
        [SerializeField]
        private TextMeshPro shadowLevelText;

        public float debugRange;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, debugRange);
        }

        public void Initialize(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
            SetLevel(1);
        }

        public void SetLevel(int level)
        {
            if (levelText == null)
                return;

            if (shadowLevelText == null)
                return;

            if (level <= 1)
            {
                levelText.text = "";
                shadowLevelText.text = levelText.text;
                return;
            }

            levelText.text = IntToRoman(level);
            shadowLevelText.text = levelText.text;
        }

        private static string IntToRoman(int number)
        {
            return number switch
            {
                1 => "I",
                2 => "II",
                3 => "III",
                4 => "IV",
                5 => "V",
                6 => "VI",
                7 => "VII",
                8 => "VIII",
                9 => "IX",
                10 => "X",
                _ => number.ToString()
            };
        }
    }
}