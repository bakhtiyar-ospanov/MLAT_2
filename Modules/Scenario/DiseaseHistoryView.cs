using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Modules.Scenario
{
    public class DiseaseHistoryView : MonoBehaviour
    {
        public GameObject root;
        [SerializeField] private Transform container;

        [Header("Prefabs")]
        [SerializeField] private TextMeshProUGUI simpleTxtPrefab;
        [SerializeField] private GroupDicTxt groupDicTxtPrefab;
        [SerializeField] private TxtButton labResearchPrefab;
        [SerializeField] private TxtButton instrumentalResearchPrefab;
        [SerializeField] private TxtButton launchButtonPrefab;
        //[SerializeField] private GameObject emptyObjectPrefab;

        private Dictionary<string, GroupDicTxt> _groupTxts = new Dictionary<string, GroupDicTxt>();

        private void Awake()
        {
            root.SetActive(false);
        }

        public void SetActivePanel(bool val)
        {
            root.SetActive(val);
        }
        
        public void AddGroup(string groupId, string text)
        {
            _groupTxts.TryGetValue(groupId, out var group);
            if (group == null)
            {
                group = Instantiate(groupDicTxtPrefab, container).GetComponent<GroupDicTxt>();
                group.titleTxt.text = text;
                group.simpleTxts = new Dictionary<string, TextMeshProUGUI>();
                group.expandableTxts = new Dictionary<string, List<string>>();
                _groupTxts.Add(groupId, group);
                AddNewValue(groupId, "empty", TextData.Get(175));
            }
            else
            {
                group.titleTxt.text = text;
            }
        }

        public bool CheckGroup(string groupId)
        {
            return _groupTxts.ContainsKey(groupId);
        }

        public void CleanGroup(string groupId)
        {
            _groupTxts.TryGetValue(groupId, out var groupTxt);
            if(groupTxt == null || groupTxt.simpleTxts.ContainsKey("empty")) return;

            foreach (var groupTxtSimpleTxt in groupTxt.simpleTxts)
            {
                DestroyImmediate(groupTxtSimpleTxt.Value.transform.parent.name.Contains("Button")
                    ? groupTxtSimpleTxt.Value.transform.parent.gameObject
                    : groupTxtSimpleTxt.Value.gameObject);
            }

            groupTxt.simpleTxts.Clear();
            groupTxt.expandableTxts.Clear();
            AddNewValue(groupId, "empty", TextData.Get(175));
        }

        public void AddLaunchButton(string groupId, string text, UnityAction call)
        {
            _groupTxts.TryGetValue(groupId, out var groupTxt);
            if(groupTxt == null) return;
            
            groupTxt.emptySpace.gameObject.SetActive(true);
            groupTxt.emptySpace.transform.SetAsLastSibling();

            var launchButton = Instantiate(launchButtonPrefab, groupTxt.transform);
            launchButton.tmpText.text = text;
            launchButton.button.onClick.AddListener(call);
            launchButton.transform.SetAsLastSibling();
            groupTxt.launchButton = launchButton;
        }

        public void RemoveLaunchButton(string groupId)
        {
            _groupTxts.TryGetValue(groupId, out var groupTxt);
            if(groupTxt == null) return;

            groupTxt.emptySpace.gameObject.SetActive(false);
            DestroyImmediate(groupTxt.GetComponentInChildren<Button>().gameObject);
        }

        public void RemoveGroup(string groupId)
        {
            _groupTxts.TryGetValue(groupId, out var groupTxt);
            if(groupTxt == null) return;
            DestroyImmediate(groupTxt.gameObject);
            _groupTxts.Remove(groupId);
        }

        public void AddNewValue(string groupId, string id, string text)
        {
            _groupTxts.TryGetValue(groupId, out var groupTxt);
            if(groupTxt == null) return;
            
            groupTxt.simpleTxts.TryGetValue(id, out var value);
            if (groupTxt.simpleTxts.ContainsKey("empty"))
            {
                DestroyImmediate(groupTxt.simpleTxts["empty"].gameObject);
                groupTxt.simpleTxts.Remove("empty");
            }

            if (value == null)
            {
                value = Instantiate(simpleTxtPrefab, groupTxt.transform).GetComponent<TextMeshProUGUI>();
                value.text = text;
                groupTxt.simpleTxts.Add(id, value);
                groupTxt.expandableTxts.Add(id, new List<string>());
                if(groupTxt.launchButton != null)
                    groupTxt.launchButton.transform.SetAsLastSibling();
            }
            else
            {
                value.text = text;
            }

            groupTxt.emptySpace.transform.SetSiblingIndex(groupTxt.transform.childCount - 2);

        }
        public void AddNewLabValue(string groupId, FullCheckUp question)
        {
            _groupTxts.TryGetValue(groupId, out var groupTxt);
            if(groupTxt == null) return;
            
            groupTxt.simpleTxts.TryGetValue(question.id, out var labVal);
            if (groupTxt.simpleTxts.ContainsKey("empty"))
            {
                DestroyImmediate(groupTxt.simpleTxts["empty"].gameObject);
                groupTxt.simpleTxts.Remove("empty");
            }

            if (labVal == null)
            {
                var newVal = Instantiate(labResearchPrefab, groupTxt.transform).GetComponent<TxtButton>();
                newVal.tmpText.text = question.name;
                newVal.button.onClick.AddListener(() => 
                    StartCoroutine(GameManager.Instance.labResultsController.Init(new List<FullCheckUp>{question}, true)));
                groupTxt.simpleTxts.Add(question.id, newVal.tmpText);
                if(groupTxt.launchButton != null)
                    groupTxt.launchButton.transform.SetAsLastSibling();
            }
            
            groupTxt.emptySpace.transform.SetSiblingIndex(groupTxt.transform.childCount - 2);
        }
        
        public void AddNewInstrumentalValue(string groupId, string text, FullCheckUp question)
        {
            _groupTxts.TryGetValue(groupId, out var groupTxt);
            if(groupTxt == null) return;
            
            groupTxt.simpleTxts.TryGetValue(question.id, out var labVal);
            if (groupTxt.simpleTxts.ContainsKey("empty"))
            {
                DestroyImmediate(groupTxt.simpleTxts["empty"].gameObject);
                groupTxt.simpleTxts.Remove("empty");
            }

            if (labVal == null)
            {
                var newVal = Instantiate(instrumentalResearchPrefab, groupTxt.transform).GetComponent<TxtButton>();
                newVal.tmpText.text = text;
                newVal.button.onClick.AddListener(() => GameManager.Instance.instrumentalSelectorController.ShowMedia(text, question));
                groupTxt.simpleTxts.Add(question.id, newVal.tmpText);
                if(groupTxt.launchButton != null)
                    groupTxt.launchButton.transform.SetAsLastSibling();
            }
            
            groupTxt.emptySpace.transform.SetSiblingIndex(groupTxt.transform.childCount - 2);
        }

        public void ExpandValue(string groupId, string parentId, string id, string newInfo)
        {
            _groupTxts.TryGetValue(groupId, out var groupTxt);
            if(groupTxt == null) return;
            groupTxt.simpleTxts.TryGetValue(parentId, out var existingTxt);

            var realParentId = "";
            
            if (existingTxt == null)
            {
                foreach (var expandableTxt in groupTxt.expandableTxts.
                    Where(expandableTxt => expandableTxt.Value.Contains(parentId)))
                {
                    realParentId = expandableTxt.Key;
                    groupTxt.simpleTxts.TryGetValue(realParentId, out existingTxt);
                    break;
                }
            }

            realParentId = string.IsNullOrEmpty(realParentId) ? parentId : realParentId; 

            groupTxt.expandableTxts.TryGetValue(realParentId, out var expandTxts);
            expandTxts?.Add(id);
            groupTxt.expandableTxts[realParentId] = expandTxts;
            
            if (expandTxts?.Contains(parentId) == true)
            {
                var pos = expandTxts.IndexOf(parentId);
                var split = existingTxt.text.Split(',');
                split[pos] += $" ({newInfo})";
                existingTxt.text = string.Join(",", split);
                return;
            }

            if (existingTxt.text.Contains(":"))
                existingTxt.text += $", {newInfo}";
            else
                existingTxt.text += $": {newInfo}";
            
        }

        public void Clean()
        {
            foreach (var groupTxt in _groupTxts)
            {
                Destroy(groupTxt.Value.gameObject);
            }
            _groupTxts.Clear();
        }
    }
}
