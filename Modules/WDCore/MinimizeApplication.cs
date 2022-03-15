using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.WDCore
{
    public class MinimizeApplication : MonoBehaviour
    {
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        private void Awake()
        {
            var btn = GetComponent<Button>();
            if(btn == null) return;
            btn.onClick.AddListener(OnMinimizeButtonClick);
        }

        private void OnMinimizeButtonClick()
        {
            ShowWindow(GetActiveWindow(), 2);
        }
        #else
        private void Awake()
        {
            var btn = GetComponent<Button>();
            btn.gameObject.SetActive(false);
        }
        #endif

    }
}
