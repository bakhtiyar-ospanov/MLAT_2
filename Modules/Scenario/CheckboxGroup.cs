using System;
using System.Collections.Generic;
using System.Linq;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Scenario
{
    public class CheckboxGroup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private List<Checkbox> checkboxes;
        [SerializeField] private Button expandButton;
        [SerializeField] private GameObject[] arrows;

        private List<string> _ids;
        private List<string> _names;
        private ToggleGroup _group = null;
        private List<string> _passedAnswers = null;
        private List<CheckboxGroup> _checkboxGroups;
        private ScrollRect _scrollRect;
        private bool _isExpanded;

        private void Awake()
        {
            expandButton.onClick.AddListener(Expand);
        }

        public void SetTitle(string val)
        {
            title.text = val;
        }

        public void AddCheckboxes(List<string> ids, List<string> names, ToggleGroup group = null, 
            List<string> passedAnswers = null, List<CheckboxGroup> checkboxGroups = null, ScrollRect scrollRect = null)
        {
            _ids = ids;
            _names = names;
            _group = group;
            _passedAnswers = passedAnswers;
            _checkboxGroups = checkboxGroups;
            _scrollRect = scrollRect;
        }

        private void Expand()
        {
            if (_isExpanded)
            {
                Shrink();
                return;
            }
            foreach (var checkboxGroup in _checkboxGroups)
                checkboxGroup.Shrink();
            
            CheckItems(_ids.Count);

            for (var i = 0; i < _ids.Count; ++i)
            {
                var id = _ids[i];
                checkboxes[i].name = id;
                checkboxes[i].tmpText.text = _names[i];
                checkboxes[i].toggle.group = _group;
                checkboxes[i].gameObject.SetActive(true);
                checkboxes[i].toggle.onValueChanged.RemoveAllListeners();
                
                checkboxes[i].toggle.onValueChanged.AddListener(val =>
                {
                    if (val) _passedAnswers.Add(id);
                    else _passedAnswers.Remove(id);
                });

                if (_passedAnswers != null && _passedAnswers.Contains(id))
                    checkboxes[i].toggle.SetIsOnWithoutNotify(true);
            }

            arrows[0].SetActive(false);
            arrows[1].SetActive(true);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            
            _scrollRect.ScrollToTop((RectTransform) title.transform);
            _isExpanded = true;
        }

        private void Shrink()
        {
            for (var i = checkboxes.Count-1; i > 0; i--)
            {
                var checkbox = checkboxes[i].gameObject;
                checkboxes.Remove(checkboxes[i]);
                DestroyImmediate(checkbox);
            }
            
            checkboxes[0].gameObject.SetActive(false);
            arrows[0].SetActive(true);
            arrows[1].SetActive(false);
            
            _isExpanded = false;
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
    }
}
