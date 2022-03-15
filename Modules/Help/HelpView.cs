using System;
using UnityEngine;

namespace Modules.Help
{
    public class HelpView : MonoBehaviour
    {
        public GameObject canvas;
        public GameObject desktopPanel;
        public GameObject touchPanel;

        private void Awake()
        {
            canvas.SetActive(false);
            desktopPanel.SetActive(!Input.touchSupported);
            touchPanel.SetActive(Input.touchSupported);
        }
        
        
    }
}
