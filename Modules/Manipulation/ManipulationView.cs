// using System.Collections.Generic;
// using System.Linq;
// using Modules.WDCore;
// using TMPro;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
//
// namespace Modules.Manipulation
// {
//     public class ManipulationView : MonoBehaviour
//     {
//         public GameObject canvas;
//         public Button closeButton;
//         public TextMeshProUGUI actionTitle;
//         public EventSystem eventSystem;
//         public GraphicRaycaster m_Raycaster;
//         public PointerEventData m_PointerEventData;
//         public Button btnPrev;
//         public Button btnNext;
//         public Dictionary<string, Transform> listByPointer;
//         
//         [SerializeField] private GameObject buttonListPrefab;
//         [SerializeField] private Transform container;
//         
//         private Dictionary<string, List<TxtButton>> _txtButtonsByPointer;
//
//         [Header("Multiselect question")] 
//         public TxtButton[] checkMultiselectButton;
//         public List<Toggle> checkboxes;
//         public Color[] colors;
//         [SerializeField] private GameObject questionRoot;
//         
//         private void Awake()
//         {
//             canvas.SetActive(false);
//             eventSystem.gameObject.SetActive(false);
//             questionRoot.SetActive(false);
//             
//             GameManager.Instance.settingsController.onDevModeChange += val =>
//             {
//                 btnPrev.gameObject.SetActive(val);
//                 btnNext.gameObject.SetActive(val);
//             };
//         }
//
//         public void Init(string heading)
//         {
//             SetActionName(heading);
//             _txtButtonsByPointer = new Dictionary<string, List<TxtButton>>();
//             if(listByPointer != null)
//                 foreach (var transform1 in listByPointer)
//                 {
//                     Destroy(transform1.Value.gameObject);
//                 }
//             listByPointer = new Dictionary<string, Transform>();
//         }
//
//         public void SetActivePanel(bool val)
//         {
//             canvas.SetActive(val);
//
//             if(!val)
//                 eventSystem.gameObject.SetActive(false);
//         }
//
//         public void SetValues(List<string> txt, List<string> pointers, List<UnityAction<Button>> calls, Button[] existingButtons)
//         {
//             for(var i = 0; i < txt.Count; ++i)
//             {
//                 if (existingButtons[i] != null)
//                 {
//                     existingButtons[i].GetComponentInChildren<TextMeshProUGUI>(true).text = txt[i];
//                     existingButtons[i].onClick.RemoveAllListeners();
//                     var i1 = i;
//                     existingButtons[i].onClick.AddListener(() => calls[i1](existingButtons[i1]));
//                     existingButtons[i].gameObject.SetActive(true);
//                 }
//                 else
//                 {
//                     _txtButtonsByPointer.TryGetValue(pointers[i], out var txtButtons);
//                     txtButtons ??= CreateButtonList(pointers[i]);
//             
//                     var parent = txtButtons[0].transform.parent;
//                     var obj = txtButtons[0].gameObject;
//                     var newButton = Instantiate(obj, parent).GetComponent<TxtButton>();
//                     txtButtons.Add(newButton);
//             
//                     newButton.tmpText.text = txt[i];
//                     newButton.button.onClick.RemoveAllListeners();
//                     var i1 = i;
//                     newButton.button.onClick.AddListener(() => calls[i1](newButton.button));
//                     newButton.gameObject.SetActive(true);
//                 }
//             }
//         }
//         
//
//          public void SetActionName(string val)
//          {
//              actionTitle.text = val;
//              actionTitle.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(val));
//          }
//
//          private List<TxtButton> CreateButtonList(string pointer)
//          {
//              var buttonList = Instantiate(buttonListPrefab, container);
//              ((RectTransform)buttonList.transform).anchoredPosition = new Vector2(0.0f, 70.0f);
//              var firstButton = buttonList.GetComponentInChildren<TxtButton>();
//              firstButton.gameObject.SetActive(false);
//              _txtButtonsByPointer.Add(pointer, new List<TxtButton>{firstButton});
//              listByPointer.Add(pointer, buttonList.transform);
//              buttonList.name = pointer;
//              return _txtButtonsByPointer[pointer];
//          }
//
//          public void ChangeGroup(Transform buttonList, string pointer)
//          {
//              listByPointer.TryGetValue(pointer, out var centralGroup);
//
//              if (centralGroup == null)
//                  CreateButtonList(pointer);
//              
//              listByPointer.TryGetValue(pointer, out centralGroup);
//              var oldGroup = listByPointer.
//                  FirstOrDefault(x => x.Value.Equals(buttonList));
//              
//              var children = buttonList.GetComponentsInChildren<TxtButton>(true);
//              foreach (var button in children)
//                  button.transform.SetParent(centralGroup);
//
//              DestroyImmediate(oldGroup.Value.gameObject);
//              listByPointer.Remove(oldGroup.Key);
//          }
//
//          public void ShowButtonLists(bool val)
//          {
//              container.gameObject.SetActive(val);
//          }
//          
//          public void ShowQuestionPanel(bool val)
//          {
//              questionRoot.SetActive(val);
//          }
//
//          public void SetAnswerOptions(string[] names, string[] pointer, Toggle[] toggles)
//          {
//              CheckItems(names.Length);
//
//              for (var i = 0; i < names.Length; ++i)
//              {
//                  var tgl = toggles[i] != null ? toggles[i] : checkboxes[i];
//                  
//                  var outline = tgl.image.gameObject.GetComponent<Outline>();
//                  if (outline == null)
//                  {
//                      outline = tgl.image.gameObject.AddComponent<Outline>();
//                      outline.effectDistance = new Vector2(2.0f, -2.0f);
//                  }
//                      
//                  
//                  outline.enabled = false;
//                  
//                  tgl.onValueChanged.RemoveAllListeners();
//                  tgl.onValueChanged.AddListener(val =>
//                  {
//                      outline.enabled = val;
//                      outline.effectColor = colors[0];
//                  });
//
//                  var block = ColorBlock.defaultColorBlock;
//                  block.disabledColor = Color.white;
//                  tgl.colors = block;
//                  
//                  tgl.graphic.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
//                  tgl.isOn = false;
//                  tgl.interactable = true;
//                  tgl.GetComponentInChildren<TextMeshProUGUI>(true).text = names[i];
//                  tgl.gameObject.SetActive(true);
//              }
//          }
//
//          private void CheckItems(int requiredSize)
//          {
//              var currentSize = checkboxes.Count;
//              if (requiredSize > currentSize)
//              {
//                  var parent = checkboxes[0].transform.parent;
//                  var obj = checkboxes[0].gameObject;
//             
//                  for (var i = 0; i < requiredSize - currentSize; i++)
//                  {
//                      var checkbox = Instantiate(obj, parent).GetComponent<Toggle>();
//                      checkboxes.Add(checkbox);
//                  }
//              }
//         
//              foreach (var txtButton in checkboxes)
//              {
//                  txtButton.graphic.color = colors[0];
//                  txtButton.isOn = false;
//                  txtButton.interactable = true;
//                  txtButton.gameObject.SetActive(false);
//              }
//          }
//     }
// }
