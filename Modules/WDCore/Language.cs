using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.WDCore
{
    public static class Language
    {
        public static string Code;
        public static Dictionary<string, string[]> LangNames;
        public static Action<string> onLanguageChange;
        
        public static void SetLanguage(string strLang)
        {
            Code = strLang;
            PlayerPrefs.SetString("APP_LANG", Code);
            PlayerPrefs.Save();
            onLanguageChange?.Invoke(strLang);
        }
    }
}
