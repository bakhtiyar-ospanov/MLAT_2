using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Scenario
{
    public class DebriefingView : MonoBehaviour
    {
        public GameObject canvas;
        public Image radialImg;
        public TextMeshProUGUI percentageTxt;
        public TextMeshProUGUI statusTxt;
        public Button openReportButton;
        public Button repeatScenarioButton;
        public Button openNewScenarioButton;
        public TextMeshProUGUI repeatScenarioTxt;
        
        public TextMeshProUGUI titleTxt;
        public TextMeshProUGUI patientTxt;
        public TextMeshProUGUI descriptionTxt;
        public ScenarioDescription scenarioDescription;

        private void Awake()
        {
            canvas.SetActive(false);
        }

        public void SetResult(string txt, int percentage)
        {
            var x = percentage / 100.0f;
            radialImg.fillAmount = x;
            radialImg.color = new Color(Mathf.Clamp(2.0f * (1 - x), 0.0f, 1.0f), 
                Mathf.Clamp(2.0f * x, 0.0f, 1.0f), 0);
            percentageTxt.text = $"{percentage}%";
            statusTxt.text = txt;
        }

        public void SetInfo(string title, string patient, string description)
        {
            titleTxt.text = title;
            patientTxt.text = patient;
            scenarioDescription.SetDescription(description);
        }
    }
}
