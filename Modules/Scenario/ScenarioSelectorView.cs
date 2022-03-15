using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Modules.Scenario
{
    public class ScenarioSelectorView : MonoBehaviour
    {
        [Serializable]
        public class ScenarioFilter
        {
            public ScenarioSelectorController.FilterTypes type;
            public TMP_Dropdown dropdown;
        }
        
        public GameObject canvas;
        public GameObject selectorRoot;
        public TMP_InputField searchField;
        public List<ScenarioFilter> filterDropdowns;
        public Button randomScenario;
        public RecyclingListView _recyclableScrollRect;
        private List<MedicalBase.Scenario> _activeScenarios;
        
        [Header("Pop up")]
        public GameObject popUpRoot;
        public RawImage preview;
        public Button closePopUpButton;
        public TextMeshProUGUI scenarioTitle;
        public ScenarioDescription scenarioDescription;
        public Button[] launchButton;

        private void Awake()
        {
            canvas.SetActive(false);
            popUpRoot.SetActive(false);
            selectorRoot.SetActive(false);
            _recyclableScrollRect.ItemCallback = PopulateItem;
        }
        
        public void SetActivePanel(bool val)
        {
            canvas.SetActive(val);
            selectorRoot.SetActive(val);
        }

        public void SetActivePopUp(bool val)
        {
            canvas.SetActive(val);
            popUpRoot.SetActive(val);
        }

        public void SetValue(List<MedicalBase.Scenario> courses)
        {
            _activeScenarios = courses;
            
            StopCoroutine(UpdateValueCount());
            StartCoroutine(UpdateValueCount());
        }

        private IEnumerator UpdateValueCount()
        {
            yield return new WaitUntil(() => _recyclableScrollRect.isActiveAndEnabled);
            _recyclableScrollRect.RowCount = 0;
            _recyclableScrollRect.RowCount = _activeScenarios.Count;
        }

        public void SetPopUpInfo(string title, string description, string patientInfo)
        {
            scenarioTitle.text = title;
            scenarioDescription.SetDescription(description, patientInfo);
        }

        public void SetFilter(ScenarioSelectorController.FilterTypes filterType, List<string> vals, string filterTitle)
        {
            vals?.Insert(0, $"{filterTitle} ({TextData.Get(120)})");
            foreach (var caseFilter in filterDropdowns.Where(caseFilter => caseFilter.type == filterType))
            {
                if(vals != null)
                    caseFilter.dropdown.options = vals.Select(text => new TMP_Dropdown.OptionData(text)).ToList();
                caseFilter.dropdown.gameObject.SetActive(vals != null);
                return;
            }
        }

        public void SetFilterValue(ScenarioSelectorController.FilterTypes filterType, int index)
        {
            foreach (var caseFilter in filterDropdowns.Where(caseFilter => caseFilter.type == filterType))
            {
                caseFilter.dropdown.SetValueWithoutNotify(index);
                caseFilter.dropdown.onValueChanged.Invoke(index);
                return;
            }
        }

        public void AddFilterListener(UnityAction<ScenarioSelectorController.FilterTypes, int> call)
        {
            foreach (var filterDropdown in filterDropdowns)
            {
                filterDropdown.dropdown.onValueChanged.RemoveAllListeners();
                filterDropdown.dropdown.onValueChanged.AddListener(val => call?.Invoke(filterDropdown.type, val));
            }
        }

        public int GetFilterValue(ScenarioSelectorController.FilterTypes filterType)
        {
            foreach (var caseFilter in filterDropdowns.Where(caseFilter => caseFilter.type == filterType))
                return caseFilter.dropdown.value;
            return -1;
        }

        private void PopulateItem(RecyclingListViewItem item, int rowIndex)
        {
            var child = item as DScenarioPreview;
            child.ConfigureCell(_activeScenarios[rowIndex]);
        }

        public void SetPreview(Texture2D text)
        {
            StopCoroutine(ShowPreview());
            preview.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            preview.texture = text;

            if (text != null)
                StartCoroutine(ShowPreview());
        }

        private IEnumerator ShowPreview()
        {
            var time = 0.0f;
            var startValue = 0.0f;
            var endValue = 1.0f;
            var duration = 0.15f;

            while (time < duration)
            {
                preview.color = new Color(1.0f, 1.0f, 1.0f,Mathf.Lerp(startValue, endValue, time / duration));
                time += Time.deltaTime;
                yield return null;
            }
            preview.color = new Color(1.0f, 1.0f, 1.0f, endValue);
        }
    }
}
