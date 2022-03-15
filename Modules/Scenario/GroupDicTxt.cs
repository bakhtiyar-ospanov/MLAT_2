using System.Collections.Generic;
using Modules.WDCore;
using TMPro;
using UnityEngine;

namespace Modules.Scenario
{
    public class GroupDicTxt : MonoBehaviour
    {
        public TextMeshProUGUI titleTxt;
        public Dictionary<string, TextMeshProUGUI> simpleTxts;
        public Dictionary<string, List<string>> expandableTxts;
        public TxtButton launchButton;
        public GameObject emptySpace;
    }
}
