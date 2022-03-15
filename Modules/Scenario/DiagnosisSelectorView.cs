using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Scenario
{
    public class DiagnosisSelectorView : MonoBehaviour
    {
        public GameObject root;
        public Button backToHistoryButton;
        public Button applyButton;
        [SerializeField] private Transform container;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Prefabs")]
        [SerializeField] private CheckboxGroup checkboxGroupPrefab;

        private List<CheckboxGroup> _checkboxGroups = new List<CheckboxGroup>();
        private ToggleGroup _toggleGroup;

        private void Awake()
        {
            root.SetActive(false);
            _toggleGroup = GetComponent<ToggleGroup>();
        }
        
        
        public void AddCheckboxGroup(List<string> diseaseIds, List<string> passedAnswers)
        {
            CheckItems(2);
            
            _checkboxGroups[0].name = "underlyingDisease";
            _checkboxGroups[0].SetTitle(TextData.Get(144));
            _checkboxGroups[1].name = "concomitantDiseases";
            _checkboxGroups[1].SetTitle(TextData.Get(145));

            var icdVersion = PlayerPrefs.GetInt("ICD_VERSION");
            var icd = icdVersion switch
            {
                0 => BookDatabase.Instance.MedicalBook.ICD10ById,
                1 => BookDatabase.Instance.MedicalBook.ICD11ById,
                _ => default
            };
            
            if(icd == default) return;

            if (GameManager.Instance.scenarioController.GetMode() != ScenarioModel.Mode.Learning)
            {
                diseaseIds = diseaseIds.OrderBy(x => Guid.NewGuid()).ToList();
                var diseaseNames = new List<string>();

                foreach (var diseaseId in diseaseIds)
                {
                    icd.TryGetValue(diseaseId, out var diseaseName);
                    diseaseNames.Add($"{diseaseId} {diseaseName?.name}");
                }

                if (diseaseIds.Count > 0)
                {
                    _checkboxGroups[0].AddCheckboxes(diseaseIds.Select(x => "underlyingDisease_" + x).ToList(), 
                        diseaseNames, _toggleGroup, passedAnswers, _checkboxGroups, scrollRect);
                    _checkboxGroups[1].AddCheckboxes(diseaseIds.Select(x => "concomitantDiseases_" + x).ToList(), 
                        diseaseNames, null,passedAnswers, _checkboxGroups, scrollRect);
                    _checkboxGroups[0].gameObject.SetActive(true);
                    _checkboxGroups[1].gameObject.SetActive(true);
                }
                
            }
            else
            {
                var diseaseNames = new List<string>();

                foreach (var diseaseId in diseaseIds)
                {
                    icd.TryGetValue(diseaseId, out var diseaseName);
                    diseaseNames.Add($"{diseaseId} {diseaseName?.name}");
                }
                
                _checkboxGroups[0].AddCheckboxes(new List<string>{"underlyingDisease_" + diseaseIds[0]}, 
                    new List<string>{diseaseNames[0]}, _toggleGroup, passedAnswers, _checkboxGroups, scrollRect);
                _checkboxGroups[0].gameObject.SetActive(true);
                
                diseaseIds.RemoveAt(0);
                diseaseNames.RemoveAt(0);
                if (diseaseIds.Count > 0)
                {
                    _checkboxGroups[1].AddCheckboxes(diseaseIds.Select(x => "concomitantDiseases_" + x).ToList(), 
                        diseaseNames, null,passedAnswers, _checkboxGroups, scrollRect);
                    _checkboxGroups[1].gameObject.SetActive(true);
                }
            }
            
        }
        
        private void CheckItems(int requiredSize)
        {
            var currentSize = _checkboxGroups.Count;
            if (requiredSize > currentSize)
            {
                for (var i = 0; i < requiredSize - currentSize; i++)
                {
                    _checkboxGroups.Add(Instantiate(checkboxGroupPrefab, container).GetComponent<CheckboxGroup>());
                }
            }
        
            foreach (var txtButton in _checkboxGroups)
            {
                txtButton.gameObject.SetActive(false);
            }
        }
        
        public void Clean()
        {
            foreach (var txtButton in _checkboxGroups)
                DestroyImmediate(txtButton.gameObject);
            
            _checkboxGroups.Clear();
        }
    }
}
