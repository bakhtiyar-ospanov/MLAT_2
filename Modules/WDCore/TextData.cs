using System;
using System.Collections.Generic;
using Modules.Books;

namespace Modules.WDCore
{
    public static class TextData
    {
        public static Action RefreshTranslation;
        private static Dictionary<string, MedicalBase.Interface> _namesById;

        public static void Set(Dictionary<string, MedicalBase.Interface> data)
        {
            _namesById = data;
            RefreshTranslation?.Invoke();
        }

        public static string Get(string key)
        {
            MedicalBase.Interface value = null;
            _namesById?.TryGetValue(key, out value);
            return value == null ? "" : value.name;
        }
        public static string Get(int key)
        {
            MedicalBase.Interface value = null;
            _namesById?.TryGetValue($"{key}", out value);
            return value == null ? "" : value.name;
        }
    }
}
