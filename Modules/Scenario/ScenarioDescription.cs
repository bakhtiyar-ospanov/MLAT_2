using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScenarioDescription : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler, IPointerExitHandler
{
    private TextMeshProUGUI _scenarioDescription;
    private static Dictionary<int, string> urlList = new Dictionary<int, string>();

    public void SetDescription(string description, string patientInfo = null)
    {        
        if (_scenarioDescription == null)
            _scenarioDescription = GetComponent<TextMeshProUGUI>();            
        
        _scenarioDescription.text = string.IsNullOrEmpty(patientInfo) ? description : $"{patientInfo}\n\n{description}";

        MatchCollection matchCollection = Regex.Matches(description,
                                                @"(?:http|https):\/\/[\S]*");        
                                                               
        foreach (Match match in matchCollection)
        {
            int linkID = AddURL(match.Value);
            _scenarioDescription.text = _scenarioDescription.text.Replace(match.Value, FormatLink(linkID, match.Value));            
        }
    }

    private string FormatLink(int linkID, string link)
    {        
        var text = link;        
        return string.Format("<link=\"{0}\"><#4a60d7><u>{1}</u></color></link>", linkID, text);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_scenarioDescription, eventData.position, eventData.pressEventCamera);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = _scenarioDescription.textInfo.linkInfo[linkIndex];
            string linkIDString = linkInfo.GetLinkID();

            Application.OpenURL(GetLinkURL(linkIDString));            
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if(_scenarioDescription == null) return;
        
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_scenarioDescription, eventData.position, eventData.pressEventCamera);

        if (linkIndex != -1)
        {
            GameManager.Instance.starterController.SelectReticule(2);
            GameManager.Instance.starterController.isBlockReticule = true;
        }
        else
        {
            GameManager.Instance.starterController.isBlockReticule = false;
            GameManager.Instance.starterController.SelectReticule(0);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.Instance.starterController.isBlockReticule = false;
        GameManager.Instance.starterController.SelectReticule(0);
    }
    
    private int AddURL(string url)
    {
        int hashCode = TMP_TextUtilities.GetSimpleHashCode(url);

        if (!urlList.ContainsKey(hashCode))
            urlList.Add(hashCode, url);

        return hashCode;
    }

    static string GetLinkURL(string stringID)
    {
        int id = Int32.Parse(stringID);

        return urlList.ContainsKey(id) ? urlList[id] : string.Empty;
    }
}
