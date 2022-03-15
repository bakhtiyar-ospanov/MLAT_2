using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.MainMenu
{
    public class SettingsView : MonoBehaviour
    {
        public GameObject canvas;
        public GameObject root;
        public TMP_Dropdown langDropdown;
        public Toggle betaTestingTgl;
        public Toggle profModeTgl;
        public TextMeshProUGUI versionTxt;
        public Button dbUpdateButton;
        public Button downloadAllCasesButton;
        public GameObject[] developerItems;
        public TMP_Dropdown doctorVoiceDropdown;
        public TMP_Dropdown icdDropdown;
        public TMP_Dropdown timeLimitDropdown;
        public Button btnClearCache;
        public Button btnOpenLogs;
        
        [Header("Graphics")]
        public TMP_Dropdown resolutionDropdown;
        public TMP_Dropdown qualityDropdown;
        public Toggle fpsTgl;
        public TMP_Dropdown zoomDropdown;
        public Toggle postFxTgl;
        public Toggle vSyncTgl;

        [Header("Professor Mode")] 
        public GameObject pintModalRoot;
        public TextMeshProUGUI title;
        public TMP_InputField pinField;
        public GameObject wrongPinError;
        public Button applyPinButton;

        private void Awake()
        {
            canvas.SetActive(false);
        }
    }
}
