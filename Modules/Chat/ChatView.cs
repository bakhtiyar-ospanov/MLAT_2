using System;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Chat
{
    public class ChatView : MonoBehaviour
    {
        public GameObject canvas;
        public Button micButton;
        public GameObject[] micIcons;
        public Slider volumeSlider;

        private void Awake()
        {
            canvas.SetActive(false);
        }
    }
}
