using System.Collections;
using System.Collections.Generic;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.WorldCourse
{
    public class WCourseView : MonoBehaviour
    {
        public GameObject canvas;
        public GameObject questionRoot;
        public Button closeButton;
        public GameObject eventSystem;
        public Button[] controlButtons;
        public Sprite[] sprites;
        public Image playImg;
        public TextMeshProUGUI actionTitle;
        public TextMeshProUGUI actionDescription;
        [SerializeField] private TxtButton[] answerButtons;
        [SerializeField] private TextMeshProUGUI questionTxt;
        private int _correctIndex;
        public int mistakeCount;
        public bool isAnswered;

        private void Awake()
        {
            canvas.SetActive(false);
            eventSystem.SetActive(false);
            questionRoot.SetActive(false);
            for (var i = 0; i < answerButtons.Length; i++)
            {
                var index = i;
                answerButtons[i].button.onClick.AddListener(() => StartCoroutine(RecordAnswer(index)));
            }
        }
        
        public void SetActivePanel(bool val)
        {
            canvas.SetActive(val);
            eventSystem.SetActive(val);
            Starter.Cursor.ActivateCursor(val);
        }

        public void ShowQuestionPanel(bool val)
        {
            questionRoot.SetActive(val);
        }
        
         public void SetPlaySprite(bool isPlay)
         {
             playImg.sprite = sprites[isPlay ? 0 : 1];
         }

         public void SetActionName(string val)
         {
             actionTitle.text = val;
         }
         
         public void SetActionDescription(string val)
         {
             actionDescription.text = val;
         }

         public void Init()
         {
             SetPlaySprite(true);
             SetActionName("");
             SetActionDescription("");
         }
         
        
        public void SetQuestion(string val)
        {
            questionTxt.text = val;
        }

        public void SetAnswers(List<string> answers, int correctIndex)
        {
            _correctIndex = correctIndex;
            mistakeCount = 0;
            for (var i = 0; i < answerButtons.Length; i++)
                answerButtons[i].tmpText.text = answers[i];
        }

        private IEnumerator RecordAnswer(int index)
        {
            if (_correctIndex == index)
            {
                GameManager.Instance.blackout.RedGreenBlackout(true);
                yield return new WaitForSeconds(1.0f);
                ShowQuestionPanel(false);
                isAnswered = true;
            }
            else
            {
                mistakeCount++;
                GameManager.Instance.blackout.RedGreenBlackout(false);
            }

        }
     }
    
}
