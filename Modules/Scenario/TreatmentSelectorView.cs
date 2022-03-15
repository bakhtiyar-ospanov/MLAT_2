using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Scenario
{
    public class TreatmentSelectorView : MonoBehaviour
    {
        public GameObject root;
        public Button backToHistoryButton;
        public Button applyButton;
        [SerializeField] private Transform container;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Prefabs")]
        [SerializeField] private CheckboxGroup checkboxGroupPrefab;

        private List<CheckboxGroup> _checkboxGroups = new List<CheckboxGroup>();

        private void Awake()
        {
            SetActivePanel(false);
        }

        public void SetActivePanel(bool val)
        {
            root.SetActive(val);
        }
        
        public void AddCheckboxGroup(List<(string, List<MedicalBase.Treatment>)> treatments, List<string> passedAnswers)
        {
            CheckItems(treatments.Count);
            
            for (var i = 0; i < treatments.Count; ++i)
            {
                if(treatments[i].Item2.Count == 0) continue;
                _checkboxGroups[i].name = ""+ i;
                _checkboxGroups[i].SetTitle(treatments[i].Item1);
                _checkboxGroups[i].AddCheckboxes(treatments[i].Item2.Select(x => x.id).ToList(), 
                    treatments[i].Item2.Select(x => x.name).ToList(), null, passedAnswers, _checkboxGroups, scrollRect);
                _checkboxGroups[i].gameObject.SetActive(true);
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
