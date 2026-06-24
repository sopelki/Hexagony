using TMPro;
using UnityEngine;

namespace Misc
{
    public class VersionDisplay : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI versionText;

        [SerializeField]
        private string prefix = "v";

        private void Start()
        {
            if (versionText != null)
                versionText.text = prefix + Application.version;
        }
    }
}