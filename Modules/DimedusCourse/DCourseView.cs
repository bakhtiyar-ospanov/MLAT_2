// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Modules.WDCore;
// using Modules.Scenario;
// using PolyAndCode.UI;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Modules.DimedusCourse
// {
//     public class DCourseView : MonoBehaviour, IRecyclableScrollRectDataSource
//     {
//         public GameObject canvas;
//         public GameObject questionRoot;
//         public Button closeButton;
//         public GameObject eventSystem;
//         public TextMeshProUGUI actionTitle;
//         public TextMeshProUGUI actionDescription;
//         [SerializeField] private TxtButton[] answerButtons;
//         [SerializeField] private TextMeshProUGUI questionTxt;
//         
//         [Header("Complex Actions")]
//         public RecyclableScrollRect _recyclableScrollRect;
//         private List<(int, string)> _activeActions;
//         private Dictionary<int, Transform> _listIndexByActionIndex = new Dictionary<int, Transform>();
//         
//         [Header("Timeline control")]
//         public Button[] controlButtons;
//         public GameObject[] playPauseIcons;
//         public TextMeshProUGUI currentTime;
//         public TextMeshProUGUI totalTime;
//         public Slider timelineSlider;
//         public Button volumeButton;
//         public GameObject[] volumeIcons;
//         private int _correctIndex;
//         public int mistakeCount;
//         public bool isAnswered;
//
//         private void Awake()
//         {
//             canvas.SetActive(false);
//             eventSystem.SetActive(false);
//             questionRoot.SetActive(false);
//             for (var i = 0; i < answerButtons.Length; i++)
//             {
//                 var index = i;
//                 answerButtons[i].button.onClick.AddListener(() => StartCoroutine(RecordAnswer(index)));
//             }
//             _recyclableScrollRect.DataSource = this;
//         }
//         
//         public void Init()
//         {
//             SetVolumeIcon(false);
//             SetActionName("");
//         }
//         
//         public void SetActivePanel(bool val)
//         {
//             canvas.SetActive(val);
//             eventSystem.SetActive(val);
//             ShowQuestionPanel(false);
//             if(!Input.touchSupported)
//                 Starter.Cursor.ActivateCursor(val);
//         }
//
//         public void ShowQuestionPanel(bool val)
//         {
//             questionRoot.SetActive(val);
//         }
//         
//          public void SetPlaySprite(bool isPlay)
//          {
//              playPauseIcons[0].SetActive(isPlay);
//              playPauseIcons[1].SetActive(!isPlay);
//          }
//         
//
//          public void SetActionName(string val)
//          {
//              actionTitle.text = val;
//          }
//          
//          public void SetActionDescription(string val)
//          {
//              actionDescription.text = val;
//          }
//
//          public void SetQuestion(string val)
//         {
//             questionTxt.text = val;
//         }
//
//         public void SetAnswers(List<string> answers, int correctIndex)
//         {
//             _correctIndex = correctIndex;
//             mistakeCount = 0;
//             for (var i = 0; i < answerButtons.Length; i++)
//                 answerButtons[i].tmpText.text = answers[i];
//         }
//
//         private IEnumerator RecordAnswer(int index)
//         {
//             if (_correctIndex == index)
//             {
//                 GameManager.Instance.blackout.RedGreenBlackout(true);
//                 yield return new WaitForSeconds(1.0f);
//                 ShowQuestionPanel(false);
//                 isAnswered = true;
//             }
//             else
//             {
//                 mistakeCount++;
//                 GameManager.Instance.blackout.RedGreenBlackout(false);
//             }
//
//         }
//
//         public void SetTotalTime(float seconds)
//         {
//             totalTime.text = TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");
//         }
//         
//         public void SetCurrentTime(double seconds)
//         {
//             currentTime.text = TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");
//         }
//         
//         public void SetValue(List<(int, string)> actions)
//         {
//             _activeActions = actions;
//             _recyclableScrollRect.ReloadData();
//         }
//
//         public void SetVolumeIcon(bool isMuted)
//         {
//             volumeIcons[0].SetActive(!isMuted);
//             volumeIcons[1].SetActive(isMuted);
//         }
//         
//         public int GetItemCount()
//         {
//             return _activeActions.Count;
//         }
//
//         public void SetCell(ICell cell, int index)
//         {
//             var actionIndex = _activeActions[index].Item1;
//
//             var item = cell as DCourseAction;
//             item.ConfigureCell(actionIndex, _activeActions[index].Item2);
//             
//             if (!_listIndexByActionIndex.ContainsKey(actionIndex))
//                 _listIndexByActionIndex.Add(actionIndex, item.transform);
//             else
//                 _listIndexByActionIndex[actionIndex] = item.transform;
//         }
//
//         public void SetSegment(int actionIndex)
//         {
//             _listIndexByActionIndex.TryGetValue(actionIndex, out var trans);
//             if(trans == null) return;
//             _recyclableScrollRect.ScrollToCenter((RectTransform) trans, RectTransform.Axis.Horizontal);
//         }
//     }
// }
