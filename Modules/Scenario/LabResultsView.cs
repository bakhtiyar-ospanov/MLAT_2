using System.Collections.Generic;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Scenario
{
    public class LabResultsView : MonoBehaviour
    {
        public GameObject root;
        public TextMeshProUGUI title;
        public Button btnClose;
        public List<GroupTxt> parameters;
        [SerializeField] private Transform container;
        [SerializeField] private GroupTxt groupDicTxtPrefab;

        private void Awake()
        {
            root.SetActive(false);
        }

        public void AddNewParameter(string parameterName, string result, string comment, string refValue, string units)
        {
            var newVal = Instantiate(groupDicTxtPrefab, container);

            newVal.txts[0].text = parameterName;
            newVal.txts[1].text = result;
            newVal.txts[2].text = comment;
            newVal.txts[3].text = refValue;
            newVal.txts[4].text = units;

            parameters.Add(newVal);
        }

        public void Clean()
        {
            foreach (var parameter in parameters)
                Destroy(parameter.gameObject);
            
            parameters.Clear();
        }
        
    }
}
