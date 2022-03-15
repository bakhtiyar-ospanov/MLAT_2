using Modules.WDCore;
using TMPro;
using UnityEngine;

namespace Modules
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [DisallowMultipleComponent]
    public class LocalizeTMP : MonoBehaviour
    {
        public string key;
        private TextMeshProUGUI _txtTmp;

        private void Awake()
        {
            _txtTmp = GetComponent<TextMeshProUGUI>();
            TextData.RefreshTranslation += SetTxt;
            SetTxt();
        }

        private void SetTxt()
        {
            _txtTmp.text = TextData.Get(key);
        }
    }
}