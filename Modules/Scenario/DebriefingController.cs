using System.Collections.Generic;
using System.Linq;
using Modules.SpeechKit;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.XR;

namespace Modules.Scenario
{
    public class DebriefingController : MonoBehaviour
    {
        public bool isInit;
        private DebriefingView _view;
        private string _pdfPath;
        private int _score;
        private string _specId;
        private ScenarioModel.Mode _scenarioMode;
        private bool _isManipulation;
        private void Awake()
        {
            _view = GetComponent<DebriefingView>();
            _view.openReportButton.onClick.AddListener(OpenReport);
            _view.repeatScenarioButton.onClick.AddListener(RepeatScenario);
            _view.openNewScenarioButton.onClick.AddListener(OpenNewScenario);
            
            GameManager.Instance.checkTableController.onScoreAndPathSet += SetScoreAndPath;
        }

        public void Init(bool isManipulation, string criticalError = null)
        {
            if(string.IsNullOrEmpty(_pdfPath)) return;
            
            var statusTxt = "";
            var assistantPhrase = "";
            _isManipulation = isManipulation;
            _scenarioMode = GameManager.Instance.scenarioController.GetMode();
            _view.repeatScenarioTxt.text = TextData.Get(265);

            if (!string.IsNullOrEmpty(criticalError))
            {
                statusTxt = TextData.Get(277);
                assistantPhrase = criticalError;
            }
            else if (_score >= 90)
            {
                statusTxt = TextData.Get(255);
                assistantPhrase = TextData.Get(259);
            } else if (_score <= 89 && _score >= 70)
            {
                statusTxt = TextData.Get(256);
                assistantPhrase = TextData.Get(260);
            } else if (_score <= 69 && _score >= 50)
            {
                statusTxt = TextData.Get(257);
                assistantPhrase = TextData.Get(261);
            } else
            {
                statusTxt = TextData.Get(258);
                assistantPhrase = TextData.Get(262);
                _scenarioMode = ScenarioModel.Mode.Learning;
                _view.repeatScenarioTxt.text = TextData.Get(271);
            }

            var scenario = GameManager.Instance.scenarioSelectorController.SelectedScenario;
            _view.SetResult(statusTxt, _score);
            _view.SetInfo(scenario.name, scenario.patientInfo, scenario.description);

            assistantPhrase += "\n" + TextData.Get(263);
            
            if (!PlayerPrefs.HasKey("PROF_PIN"))
                TextToSpeech.Instance.Speak(assistantPhrase, TextToSpeech.Character.Assistant, false);
            
            _specId = scenario.specializationIds[0];

            GameManager.Instance.mainMenuController.AddPopUpModule("Debriefing", SetActivePanel, new []{_view.canvas.transform});
            isInit = true;
        }
        
        private void SetScoreAndPath(int score, string path)
        {
            _score = score;
            _pdfPath = path;
        }

        public void OpenReport()
        {
            if(!XRSettings.enabled)
                DirectoryPath.OpenPDF(_pdfPath);
        }
        
        private void RepeatScenario()
        {
            Clean();
            StartCoroutine(GameManager.Instance.scenarioSelectorController.LaunchScenario(_scenarioMode));
        }
        
        private void OpenNewScenario()
        {
            Clean();
            GameManager.Instance.mainMenuController.ShowMenu("ScenarioSelector");
            _specId = null;
        }
        
        private void SetActivePanel(bool val)
        {
            _view.canvas.SetActive(val);
        }
        
        public void Clean()
        {
            SetActivePanel(false);
            TextToSpeech.Instance.StopSpeaking();
            GameManager.Instance.mainMenuController.RemovePopUpModule("Debriefing");
            _pdfPath = null;
            _score = -1;
            isInit = false;
        }
    }
}
