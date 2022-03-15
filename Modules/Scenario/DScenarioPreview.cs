using Modules.Books;
using Modules.WDCore;
using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Scenario
{
    public class DScenarioPreview : RecyclingListViewItem
    {
        //public RawImage preview;
        public TextMeshProUGUI scenarioName;
        public TextMeshProUGUI scenarioDescription;
        public Button button;

        private MedicalBase.Scenario _scenario;
        private Color _inactiveColor = new Color(0.8f, 0.8f, 0.8f);
        private void Awake()
        {
            button.onClick.AddListener(ButtonListener);
        }

        public void ConfigureCell(MedicalBase.Scenario scenario)
        {
            _scenario = scenario;
            var isTeacher = PlayerPrefs.GetInt("TEACHER_MODE") == 1;
            scenarioName.text = isTeacher ? scenario.name : scenario.nameComplaintBased;
            scenarioDescription.text = scenario.patientInfo;
            //button.interactable = scenario.isAvailable;
            scenarioName.color = scenario.isAvailable ? Color.black : _inactiveColor;
            scenarioDescription.color = scenario.isAvailable ? Color.black : _inactiveColor;
            //preview.texture = null;
            //StopAllCoroutines();
            //StartCoroutine(SetPreviewRoutine(course));
        }

        // private IEnumerator SetPreviewRoutine(CoursesDimedus.Course course)
        // {
        //     yield return new WaitUntil(() => course.preview != null);
        //     preview.texture = course.preview;
        // }

        private void ButtonListener()
        {
            if (_scenario.isAvailable)
                GameManager.Instance.scenarioSelectorController.OpenPopup(_scenario);
            else
                GameManager.Instance.warningController.ShowExitWarning(TextData.Get(314), () => 
                {
                    GameManager.Instance.mainMenuController.ShowMenu("Profile");
                    GameManager.Instance.GSAuthController.ShowActivationForm(true);
                }, false, null, TextData.Get(315));
        }
    }
}
