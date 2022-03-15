using TMPro;
using UnityEngine;

namespace Modules.Scenario
{
    public class WallHint : MonoBehaviour
    {
        private TextMeshProUGUI _txtHint;

        private void Awake()
        {
            _txtHint = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void ShowHint(string val)
        {
            return;
            var isNull = string.IsNullOrEmpty(val);
            if(!isNull)
                _txtHint.text = val.Replace("+", "");
            
            gameObject.SetActive(!isNull);
        }
        
    }
}
