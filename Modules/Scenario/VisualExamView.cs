using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Scenario
{
    public class VisualExamView : MonoBehaviour
    {
        public GameObject root;
        public Button backToHistoryButton;
        public Button applyButton;
        [SerializeField] private Transform container;

        [Header("Prefabs")]
        [SerializeField] private VisualExamCheckboxGroup checkboxGroupPrefab;

        private List<VisualExamCheckboxGroup> _checkboxGroups = new List<VisualExamCheckboxGroup>();

        private void Awake()
        {
            root.SetActive(false);
        }
        
        public void AddCheckboxGroup(List<FullCheckUp> allCheckups, List<StatusInstance.Status.CheckUp> caseCheckups)
        {
            if(caseCheckups == null || allCheckups == null) return;
            var isLearning = GameManager.Instance.scenarioController.GetMode() == ScenarioModel.Mode.Learning;

            if (isLearning)
            {
                var caseCheckupsIds = caseCheckups.Select(x => x.id).ToList();
                var noPointsCaseCheckups = allCheckups.Where(x => caseCheckupsIds.Contains(x.id)).ToList();
                CheckItems(noPointsCaseCheckups.Count);
                
                for (var i = 0; i < noPointsCaseCheckups.Count; ++i)
                {
                    _checkboxGroups[i].name = noPointsCaseCheckups[i].id;
                    _checkboxGroups[i].SetTitle(noPointsCaseCheckups[i].name);
                    _checkboxGroups[i].AddCheckboxes(noPointsCaseCheckups[i], 
                        caseCheckups.FirstOrDefault(x => x.id == noPointsCaseCheckups[i].id)?.children.Select(x => x.id).ToList());
                    _checkboxGroups[i].gameObject.SetActive(true);
                }
            }
            else
            {
                CheckItems(allCheckups.Count);
                for (var i = 0; i < allCheckups.Count; ++i)
                {
                    var caseCheckup = caseCheckups?.Where(x => x.id == allCheckups[i].id).ToList();
                    _checkboxGroups[i].name = allCheckups[i].id;
                    _checkboxGroups[i].SetTitle(allCheckups[i].name);
                    _checkboxGroups[i].AddCheckboxes(allCheckups[i], 
                        caseCheckup?.Count != 0 ? caseCheckup?[0].children.Select(x => x.id).ToList() : null);
                    _checkboxGroups[i].gameObject.SetActive(true);
                }
            }
            
        }

        public List<string> GetCheckboxValues(string checkUpId)
        {
            var group = _checkboxGroups.FirstOrDefault(x => x.name == checkUpId);
            return group != null ? group.GetCheckboxValues() : new List<string>();
        }
        
        
        private void CheckItems(int requiredSize)
        {
            var currentSize = _checkboxGroups.Count;
            if (requiredSize > currentSize)
            {
                for (var i = 0; i < requiredSize - currentSize; i++)
                {
                    _checkboxGroups.Add(Instantiate(checkboxGroupPrefab, container).GetComponent<VisualExamCheckboxGroup>());
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
