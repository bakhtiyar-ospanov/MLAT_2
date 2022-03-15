using UnityEngine;
using UnityEngine.UI;

namespace Modules.WDCore
{
    public class QuitApplication : MonoBehaviour
    {
        private void Awake()
        {
            #if UNITY_ANDROID
            transform.parent.parent.gameObject.SetActive(false);
            return;
            #endif
            
            var btn = GetComponent<Button>();
            if(btn == null) return;
            btn.onClick.AddListener(() => 
                GameManager.Instance.warningController.ShowExitWarning(TextData.Get(235), Application.Quit));
        }
    }
}
