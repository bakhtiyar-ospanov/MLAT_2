using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Modules.WorldCourse
{
    public class WCourseSelectorView : MonoBehaviour, IRecyclableScrollRectDataSource
    {
        [Serializable]
        public class CourseFilter
        {
            public WCourseSelectorController.FilterTypes type;
            public TMP_Dropdown dropdown;
        }
        
        public GameObject canvas;
        public GameObject selectorRoot;
        public Button outfieldButton;
        public Button closeButton;
        public TMP_InputField searchField;
        public TextMeshProUGUI sortTitle;
        public Button sortByDateButton;
        public Button sortByNameButton;
        [SerializeField] private List<CourseFilter> filterDropdowns;
        [SerializeField] private RecyclableScrollRect _recyclableScrollRect;
        private List<WCourse.Course> _activeCourses;
        
        [Header("Pop-up")]
        public GameObject popUpRoot;
        public Button closePopUpButton;
        public Button launchButton;
        public RawImage preview;
        public TextMeshProUGUI courseName;
        public TextMeshProUGUI description;
        public TextMeshProUGUI category;
        public TextMeshProUGUI duration;
        //public TextMeshProUGUI objects;
        //public TextMeshProUGUI processes;

        private void Awake()
        {
            closeButton.onClick.AddListener(() => SetActivePanel(false));
            outfieldButton.onClick.AddListener(() => SetActivePanel(false));
            canvas.SetActive(false);
            popUpRoot.SetActive(false);
            selectorRoot.SetActive(false);
            closePopUpButton.onClick.AddListener(() => SetActivePanel(true));
            _recyclableScrollRect.DataSource = this;
        }
        
        public void SetActivePanel(bool val)
        {
            canvas.SetActive(val);
            selectorRoot.SetActive(val);
            popUpRoot.SetActive(false);
            Starter.Cursor.ActivateCursor(val);
        }

        public void SetValue(List<WCourse.Course> courses)
        {
            _activeCourses = courses;
            _recyclableScrollRect.ReloadData();
        }

        public void ShowPopUp(string cName, string cDescription, string[] cCategory, Texture2D previewText)
        {
            courseName.text = cName;
            description.text = cDescription;
            category.text = $"{TextData.Get(162)}: {string.Join(',', cCategory)}";
            // objects.text = $"{TextData.Get(164)}: {cObjects}";
            // processes.text = $"{TextData.Get(165)}: {cProcesses}";
            preview.texture = previewText;
            selectorRoot.SetActive(false);
            popUpRoot.SetActive(true);
        }
        
        
        public void SetFilter(WCourseSelectorController.FilterTypes filterType, List<string> vals, string filterTitle)
        {
            vals = new List<string>(vals);
            vals.Insert(0, $"{filterTitle} ({TextData.Get(120)})");
            foreach (var caseFilter in filterDropdowns.Where(caseFilter => caseFilter.type == filterType))
            {
                caseFilter.dropdown.options = vals.Select(text => new TMP_Dropdown.OptionData(text)).ToList();
                return;
            }
        }

        public void AddFilterListener(UnityAction<WCourseSelectorController.FilterTypes, int> call)
        {
            foreach (var filterDropdown in filterDropdowns)
            {
                filterDropdown.dropdown.onValueChanged.RemoveAllListeners();
                filterDropdown.dropdown.onValueChanged.AddListener(val => call?.Invoke(filterDropdown.type, val));
            }
        }

        public int GetFilterValue(WCourseSelectorController.FilterTypes filterType)
        {
            foreach (var caseFilter in filterDropdowns.Where(caseFilter => caseFilter.type == filterType))
                return caseFilter.dropdown.value;
            return -1;
        }

        public int GetItemCount()
        {
            return _activeCourses.Count;
        }

        public void SetCell(ICell cell, int index)
        {
            var item = cell as WCoursePreview;
            item.ConfigureCell(_activeCourses[index]);
        }
    }
}
