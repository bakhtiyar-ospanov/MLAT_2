using Modules.WDCore;
using TMPro;
using UnityEngine;

namespace Modules.LanguageInit
{
    public class LanguageInitView : MonoBehaviour
    {
        public GameObject canvas;
        public TMP_Dropdown langDropdown;
        public TxtButton confirmButton;
        public TextMeshProUGUI title;

        private void Awake()
        {
            canvas.SetActive(false);
        }
    }
}
