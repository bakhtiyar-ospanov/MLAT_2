using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace Modules.LanguageInit
{
    public class LanguageInitController : MonoBehaviour
    {
        private LanguageInitView _view;
        private bool _isSelected;

        private void Awake()
        {
            _view = GetComponent<LanguageInitView>();
            _view.confirmButton.button.onClick.AddListener(() =>
            {
                var langCode = Language.LangNames.Keys.ToArray()[_view.langDropdown.value];
                Language.SetLanguage(langCode);
                _isSelected = true;
            });
        }

        public IEnumerator Init()
        {
            Language.LangNames = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(
                BookDatabase.Instance.Configurations["Langs"]);

            if (PlayerPrefs.HasKey("APP_LANG"))
            {
                var langCode = PlayerPrefs.GetString("APP_LANG");
                Language.SetLanguage(langCode);
                yield break;
            }
            else
            {
#if UNITY_STANDALONE_WIN
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey($"Software\\{Application.companyName}\\{Application.productName}");
                var innoLang = key?.GetValue("InnoLanguage");

                switch (innoLang)
                {
                    case "russian":
                        Language.SetLanguage("ru");
                        break;
                    case "english":
                        Language.SetLanguage("en");
                        break;
                    case "german":
                        Language.SetLanguage("de");
                        break;
                    case "ukrainian":
                        Language.SetLanguage("uk");
                        break;
                }
                
                if(!string.IsNullOrEmpty(Language.Code)) yield break;
#endif
            }

            _view.langDropdown.options = new List<TMP_Dropdown.OptionData>();
            foreach (var langCode in Language.LangNames)
                _view.langDropdown.options.Add(new TMP_Dropdown.OptionData(langCode.Value[0]));

            _view.langDropdown.onValueChanged.RemoveAllListeners();
            _view.langDropdown.onValueChanged.AddListener(ChangeLanguage);

            ChangeLanguage(0);
            
            _view.canvas.SetActive(true);
            yield return new WaitUntil(() => _isSelected);
            _view.canvas.SetActive(false);
        }

        private void ChangeLanguage(int val)
        {
            var langCode = Language.LangNames.Keys.ToArray()[val];
            _view.title.text = Language.LangNames[langCode][1];
            _view.confirmButton.tmpText.text = Language.LangNames[langCode][2];
        }
    }
}
