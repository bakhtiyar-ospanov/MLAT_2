// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Modules.Books;
// using Modules.WDCore;
// using PolyAndCode.UI;
// using TMPro;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.UI;
//
// namespace Modules.DimedusCourse
// {
//     public class DCourseSelectorView : MonoBehaviour, IRecyclableScrollRectDataSource
//     {
//         [Serializable]
//         public class CourseFilter
//         {
//             public DCourseSelectorController.FilterTypes type;
//             public TMP_Dropdown dropdown;
//         }
//         
//         public GameObject canvas;
//         public GameObject selectorRoot;
//         public Button outfieldButton;
//         public Button closeButton;
//         public TMP_InputField searchField;
//         public TextMeshProUGUI sortTitle;
//         public Button sortByDateButton;
//         public Button sortByNameButton;
//         [SerializeField] private List<CourseFilter> filterDropdowns;
//         [SerializeField] private RecyclableScrollRect _recyclableScrollRect;
//         private List<CoursesDimedus.Course> _activeCourses;
//         
//         [Header("Pop-up")]
//         public GameObject popUpRoot;
//         public Button closePopUpButton;
//         public Button launchButton;
//         public RawImage preview;
//         public TextMeshProUGUI courseName;
//         public TextMeshProUGUI description;        
//         public TextMeshProUGUI specializations;
//         public TextMeshProUGUI skills;
//         public TextMeshProUGUI link;
//
//         private void Awake()
//         {
//             closeButton.onClick.AddListener(() => SetActivePanel(false));
//             outfieldButton.onClick.AddListener(() => SetActivePanel(false));
//             canvas.SetActive(false);
//             popUpRoot.SetActive(false);
//             selectorRoot.SetActive(false);
//             closePopUpButton.onClick.AddListener(() => SetActivePanel(true));
//             _recyclableScrollRect.DataSource = this;
//         }
//         
//         public void SetActivePanel(bool val)
//         {
//             canvas.SetActive(val);
//             selectorRoot.SetActive(val);
//             popUpRoot.SetActive(false);
//             if(!Input.touchSupported)
//                 Starter.Cursor.ActivateCursor(val);
//         }
//
//         public void SetValue(List<CoursesDimedus.Course> courses)
//         {
//             _activeCourses = courses;
//             _recyclableScrollRect.ReloadData();
//         }
//
//         public void ShowPopUp(string cName, string cDescription, string cSpecs, string cSkills, Texture2D previewText)
//         {
//             courseName.text = cName;
//             String[] separatorURL = { "http://", "https://" };
//             String[] strList = cDescription.Split(separatorURL, 2, StringSplitOptions.None);
//             if (strList.Length == 1) description.text = cDescription;
//             else
//             {
//                 description.text = strList[0].Replace("URL: ", "");
//                 link.text = "URL: " + strList[1].Replace(" ", "");
//
//                 var hyperlinkButton = link.GetComponent<Button>();                
//                 hyperlinkButton.onClick.RemoveAllListeners();                
//                 hyperlinkButton.onClick.AddListener(() => Application.OpenURL("http://" + strList[1].Replace(" ", "")));
//                 
//                 
//             }
//             specializations.text = $"{TextData.Get(64)}: {cSpecs}";
//             skills.text = $"{TextData.Get(167)}: {cSkills}";
//             preview.texture = previewText;
//             selectorRoot.SetActive(false);
//             popUpRoot.SetActive(true);
//         }
//         
//         
//         public void SetFilter(DCourseSelectorController.FilterTypes filterType, List<string> vals, string filterTitle)
//         {
//             vals.Insert(0, $"{filterTitle} ({TextData.Get(120)})");
//             foreach (var caseFilter in filterDropdowns.Where(caseFilter => caseFilter.type == filterType))
//             {
//                 caseFilter.dropdown.options = vals.Select(text => new TMP_Dropdown.OptionData(text)).ToList();
//                 return;
//             }
//         }
//
//         public void AddFilterListener(UnityAction<DCourseSelectorController.FilterTypes, int> call)
//         {
//             foreach (var filterDropdown in filterDropdowns)
//             {
//                 filterDropdown.dropdown.onValueChanged.RemoveAllListeners();
//                 filterDropdown.dropdown.onValueChanged.AddListener(val => call?.Invoke(filterDropdown.type, val));
//             }
//         }
//
//         public int GetFilterValue(DCourseSelectorController.FilterTypes filterType)
//         {
//             foreach (var caseFilter in filterDropdowns.Where(caseFilter => caseFilter.type == filterType))
//                 return caseFilter.dropdown.value;
//             return -1;
//         }
//
//         public int GetItemCount()
//         {
//             return _activeCourses.Count;
//         }
//
//         public void SetCell(ICell cell, int index)
//         {
//             var item = cell as DCoursePreview;
//             item.ConfigureCell(_activeCourses[index]);
//         }
//     }
// }
