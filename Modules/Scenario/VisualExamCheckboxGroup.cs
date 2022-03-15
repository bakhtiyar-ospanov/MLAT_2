using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using TMPro;
using UnityEngine;

namespace Modules.Scenario
{
    public class VisualExamCheckboxGroup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private List<Checkbox> checkboxes;

        public void SetTitle(string val)
        {
            title.text = val;
        }

        public void AddCheckboxes(FullCheckUp parent, List<string> selectedCheckups)
        {
            var vals = parent.children;
            var isLearning = GameManager.Instance.scenarioController.GetMode() == ScenarioModel.Mode.Learning;
            CheckItems(vals.Count + 1);
            var randomPool = new List<GameObject>();

            for (var i = 0; i < vals.Count; ++i)
            {
                checkboxes[i].name = vals[i].id;
                checkboxes[i].tmpText.text = vals[i].name;
                if(selectedCheckups != null && selectedCheckups.Contains(vals[i].id))
                    checkboxes[i].gameObject.SetActive(true);
                else
                {
                    randomPool.Add(checkboxes[i].gameObject);
                }
            }


            if (randomPool.Count > 0 && !isLearning)
            {
                var randomItem = randomPool[Random.Range(0, randomPool.Count-1)];
                if (randomItem != null)
                {
                    randomItem.SetActive(true);
                    randomPool.Remove(randomItem);
                }

                for (var i = 0; i < 4; i++)
                {
                    if(randomPool.Count == 0) return;
                    randomItem = randomPool[Random.Range(0, randomPool.Count-1)];
                    if (randomItem == null) continue;
                    randomItem.SetActive(Random.Range(0, 1) == 0);
                    randomPool.Remove(randomItem);
                }
            }
            
            if (parent.GetPointInfo() != null && (!isLearning || selectedCheckups?.Count == 0))
            {
                checkboxes[vals.Count].name = parent.GetPointInfo().id;
                checkboxes[vals.Count].tmpText.text = parent.GetPointInfo().description;
                checkboxes[vals.Count].gameObject.SetActive(true);
                checkboxes[vals.Count].toggle.onValueChanged.AddListener(DeactivatePathological);
            }
        }

        public List<string> GetCheckboxValues()
        {
            return (from t in checkboxes where t.toggle.isOn select t.name).ToList();
        }
        
        private void CheckItems(int requiredSize)
        {
            var currentSize = checkboxes.Count;
            if (requiredSize > currentSize)
            {
                var parent = checkboxes[0].transform.parent;
                var obj = checkboxes[0].gameObject;
            
                for (var i = 0; i < requiredSize - currentSize; i++)
                {
                    var checkbox = Instantiate(obj, parent).GetComponent<Checkbox>();
                    checkbox.toggle.isOn = false;
                    checkbox.toggle.interactable = true;
                    checkboxes.Add(checkbox);
                }
            }
        
            foreach (var txtButton in checkboxes)
            {
                txtButton.gameObject.SetActive(false);
            }
        }

        private void DeactivatePathological(bool val)
        {
            for (var i = 0; i < checkboxes.Count-1; i++)
            {
                checkboxes[i].toggle.isOn = false;
                checkboxes[i].toggle.interactable = !val;
            }
        }
    }
}
