using System;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.WDCore
{
    public class AppControlsView : MonoBehaviour
    {
        public GameObject canvas;

        public Button minimizeApplication;
        public Button resizeApplication;
        public Button quitApplication;
        public Button newsButton;

        private void Awake()
        {
            canvas.SetActive(false);
        }
    }
}
