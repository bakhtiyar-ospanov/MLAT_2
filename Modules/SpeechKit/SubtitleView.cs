using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.SpeechKit
{
    public class SubtitleView : MonoBehaviour
    {
        public GameObject canvas;
        public TextMeshProUGUI textTMP;
        public Button skipButton;

        private void Awake()
        {
            canvas.SetActive(false);
        }
    }
}

