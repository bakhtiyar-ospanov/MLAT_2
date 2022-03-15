using System;
using System.Collections.Generic;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Modules.Film
{
    public class FilmView : MonoBehaviour
    {
        public GameObject canvas;
        public GameObject root;
        [SerializeField] private List<TxtButton> txtButtons;
        [SerializeField] private GameObject emptySign;
        public Color inactiveColor;
        public Color activeColor;

        [Header("Player")] 
        public GameObject playerCanvas;
        public Button backButton;
        public CustomPlayer customPlayer;
        public RawImage screen;
        public GameObject playIcon;
        public GameObject pauseIcon;
        public Slider slider;
        public TextMeshProUGUI currentTime;
        public TextMeshProUGUI totalTime;
        public Button volumeButton;
        public GameObject[] volumeIcons;
        public Color transparentColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        public GameObject loading;
        public TextMeshProUGUI watermark;
        
        private void Awake()
        {
            canvas.SetActive(false);
            emptySign.SetActive(true);
            ShowScreen(false);
            customPlayer.OnPlay += () => VideoPlay(true);
            customPlayer.OnPause += () => VideoPlay(false);
        }

        public void SetValues(List<string> texts, List<UnityAction> calls, List<bool> availability)
        {
            CheckButtons(texts.Count);
            emptySign.SetActive(texts.Count == 0);
        
            for (var i = 0; i < texts.Count; ++i)
            {
                txtButtons[i].tmpText.text = texts[i];
                txtButtons[i].button.image.color = availability[i] ? activeColor : inactiveColor;
                txtButtons[i].button.onClick.RemoveAllListeners();
                txtButtons[i].button.onClick.AddListener(calls[i]);
                txtButtons[i].gameObject.SetActive(true);
            }
        }

        private void CheckButtons(int requiredSize)
        {
            var currentSize = txtButtons.Count;
            if (requiredSize > currentSize)
            {
                var parent = txtButtons[0].transform.parent;
                var obj = txtButtons[0].gameObject;
            
                for (var i = 0; i < requiredSize - currentSize; i++)
                {
                    txtButtons.Add(Instantiate(obj, parent).GetComponent<TxtButton>());
                }
            }
        
            foreach (var txtButton in txtButtons)
            {
                txtButton.gameObject.SetActive(false);
            }
        }

        public void VideoPlay(bool isPlay)
        {
            playIcon.SetActive(!isPlay);
            pauseIcon.SetActive(isPlay);
        }
        
        public void SetVolumeIcon(bool isMuted)
        {
            volumeIcons[0].SetActive(!isMuted);
            volumeIcons[1].SetActive(isMuted);
        }
        
        public void SetTotalTime(float seconds)
        {
            totalTime.text = TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");
        }
        
        public void SetCurrentTime(float seconds)
        {
            currentTime.text = TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");;
        }

        public void ShowScreen(bool val)
        {
            screen.color = val ? Color.white : transparentColor;
        }
    }
}
