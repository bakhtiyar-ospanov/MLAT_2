using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Scenario
{
    public class InstrumentalSelectorView : MonoBehaviour
    {
        public GameObject root;
        public GameObject[] mediaRoot;
        public Button backToHistoryButton;
        public Button applyButton;
        public Button[] acceptMedia;
        
        public Transform container;
        public Transform mediaContainer;
        public TextMeshProUGUI noMediaTxt;
        public ScrollRect scrollRect;
        
        [Header("Prefabs")]
        public CheckboxGroup checkboxGroupPrefab;
        public TxtImage txtImagePrefab;

        private List<CheckboxGroup> _checkboxGroups = new List<CheckboxGroup>();

        private void Awake()
        {
            root.SetActive(false);
            mediaRoot[0].SetActive(false);
            mediaRoot[1].SetActive(false);
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
                    FirstOrDefault(x => x.id == Config.InstrResearchParentd)?.children;
                
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
        
        
        public void LoadMedia(Dictionary<string, Object> media, RawImage[] placeholders)
        {
            foreach (var medium in media)
            {
                if (medium.Key.Contains("jpg") || medium.Key.Contains("jpeg") || medium.Key.Contains("png"))
                {
                    var mediaElement = Instantiate(placeholders[0].transform.parent, 
                        placeholders[0].transform.parent.parent);
                    mediaElement.gameObject.SetActive(true);

                    var imagePlaceholder = mediaElement.GetComponentInChildren<RawImage>();
                    var texture = (Texture2D) medium.Value;
                    imagePlaceholder.texture = texture;
                    var rt = (RectTransform) imagePlaceholder.transform;
                    var referenceLength = rt.rect.width;
                    imagePlaceholder.rectTransform.sizeDelta = texture.width > texture.height ? 
                        new Vector2(referenceLength, referenceLength*(float) texture.height / texture.width) : 
                        new Vector2(referenceLength*(float) texture.height / texture.width, referenceLength);
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
