using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Scenario
{
    public class LabSelectorView : MonoBehaviour
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
        
        public void AddCheckboxGroup(List<StatusInstance.Status.CheckUp> parentCheckups, List<string> passedAnswers)
        {
            if (GameManager.Instance.scenarioController.GetMode() == ScenarioModel.Mode.Learning)
            {
                CheckItems(parentCheckups.Count);
                
                for (var i = 0; i < parentCheckups.Count; ++i)
                {
                    if(parentCheckups[i].children.
                        Count(x => x.children.Count == 0) == parentCheckups[i].children.Count) continue;
                
                    _checkboxGroups[i].name = parentCheckups[i].id;
                    _checkboxGroups[i].SetTitle(parentCheckups[i].GetInfo().name);
                    
                    var ids = parentCheckups[i].children
                        .Where(x => x.children.Count > 0).Select(x => x.id).ToList();
                    var names = parentCheckups[i].children
                        .Where(x => x.children.Count > 0).Select(x => x.GetInfo().name).ToList();
                
                    _checkboxGroups[i].AddCheckboxes(ids, names, null, passedAnswers, _checkboxGroups, scrollRect);
                    _checkboxGroups[i].gameObject.SetActive(true);
                }
            }
            else
            {
                var allLabs = BookDatabase.Instance.allCheckUps.
                    FirstOrDefault(x => x.id == Config.LabResearchParentId)?.children;
                
                CheckItems(allLabs.Count);
                
                for (var i = 0; i < allLabs.Count; ++i)
                {
                    _checkboxGroups[i].name = allLabs[i].id;
                    _checkboxGroups[i].SetTitle(allLabs[i].name);

                    var ids = allLabs[i].children.Select(x => x.id).ToList();
                    var names = allLabs[i].children.Select(x => x.name).ToList();
                
                    _checkboxGroups[i].AddCheckboxes(ids, names, null, passedAnswers, _checkboxGroups, scrollRect);
                    _checkboxGroups[i].gameObject.SetActive(true);
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
