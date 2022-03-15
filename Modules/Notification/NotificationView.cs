using System;
using iTextSharp.text.pdf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Notification
{
    public class NotificationView : MonoBehaviour
    {
        public GameObject canvas;
        public Button closeButton;
        public TextMeshProUGUI headerTxt;
        public TextMeshProUGUI bodyTxt;

        private void Awake()
        {
            canvas.SetActive(false);
        }

        public void SetInfo(string header, string body)
        {
            headerTxt.text = header;
            bodyTxt.text = body;
        }

        public void SetActivePanel(bool val)
        {
            canvas.SetActive(val);
        }
    }
}
