using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Modules.Scenario
{
    public class DiagnosisSelectorController : MonoBehaviour
    {
        public List<string> passedAnswers = new List<string>();
        
        private DiagnosisSelectorView _view;
        private bool isFilled;

        private void Awake()
        {
            _view = GetComponent<DiagnosisSelectorView>();
            _view.applyButton.onClick.AddListener(() =>
            {
                GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
                AddToDiseaseHistory();
            });
            _view.backToHistoryButton.onClick.AddListener(() =>
            {
                GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
            });
            
        }

        public void Init()
        {
            if (isFilled)
            {
                GameManager.Instance.mainMenuController.ShowMenu("DiagnosisSelector");
                return;
            }
            
            var scenarioId = GameManager.Instance.scenarioLoader.CurrentScenario.id;
            BookDatabase.Instance.MedicalBook.diagnosisById.TryGetValue(scenarioId, out var diagnosis);
            if(diagnosis == null) return;
            
            var icdVersion = PlayerPrefs.GetInt("ICD_VERSION");
            var diseaseIds = new List<string>();
            
            var underlyingDisease = icdVersion switch
            {
                0 => diagnosis.underlyingDiseaseICD10,
                1 => diagnosis.underlyingDiseaseICD11,
                _ => default
            };
            
            var concomitantDiseases = icdVersion switch
            {
                0 => diagnosis.concomitantDiseasesICD10,
                1 => diagnosis.concomitantDiseasesICD11,
                _ => default
            };
            
            var destructorsDiseases = icdVersion switch
            {
                0 => diagnosis.destructorsDiseasesICD10,
                1 => diagnosis.destructorsDiseasesICD11,
                _ => default
            };

            diseaseIds.Add(underlyingDisease);
                
            if(concomitantDiseases != null)
                diseaseIds.AddRange(concomitantDiseases);

            if (destructorsDiseases != null 
                && GameManager.Instance.scenarioController.GetMode() != ScenarioModel.Mode.Learning)
            {
                var shuffled = destructorsDiseases.ToList().OrderBy(x => Guid.NewGuid()).ToList();
                diseaseIds.AddRange(shuffled);
            }

            _view.AddCheckboxGroup(diseaseIds, passedAnswers);
            isFilled = true;
            GameManager.Instance.mainMenuController.AddPopUpModule("DiagnosisSelector", SetActivePanel, new []{_view.root.transform});
            GameManager.Instance.mainMenuController.ShowMenu("DiagnosisSelector");
        }

        private void AddToDiseaseHistory()
        {
            var dHistory = GameManager.Instance.diseaseHistoryController;
            dHistory.CleanGroup("diagnosis");
            
            var icdVersion = PlayerPrefs.GetInt("ICD_VERSION");
            var icd = icdVersion switch
            {
                0 => BookDatabase.Instance.MedicalBook.ICD10ById,
                1 => BookDatabase.Instance.MedicalBook.ICD11ById,
                _ => default
            };
            
            if(icd == default) return;
            
            if (passedAnswers.Count(x => x.Contains("underlyingDisease_")) > 0)
            {
                var underlyingDiseases = passedAnswers.FirstOrDefault(x => x.Contains("underlyingDisease_"))?.
                    Replace("underlyingDisease_", "");

                if (underlyingDiseases != default)
                {
                    icd.TryGetValue(underlyingDiseases, out var diseaseName);
                    dHistory.AddNewValue("diagnosis", "underlyingDiseaseSubtitle", TextData.Get(144));
                    dHistory.ExpandValue("diagnosis", "underlyingDiseaseSubtitle", "underlyingDisease", 
                        $"{underlyingDiseases} {diseaseName?.name}");
                }
            }

            if (passedAnswers.Count(x => x.Contains("concomitantDiseases_")) > 0)
            {
                var answers = new List<string>();
                foreach (var concomitantDisease in passedAnswers.Where(x => x.Contains("concomitantDiseases_")))
                {
                    var id = concomitantDisease.Replace("concomitantDiseases_", "");
                    icd.TryGetValue(id, out var diseaseName);
                    answers.Add($"{id} {diseaseName?.name}");
                }
                dHistory.AddNewValue("diagnosis", "concomitantDiseasesSubtitle", TextData.Get(145));
                dHistory.ExpandValue("diagnosis", "concomitantDiseasesSubtitle", "concomitantDiseases", string.Join( ", ",answers));
            }
            
            GameManager.Instance.checkTableController.RegisterTriggerInvoke("SelectDiagnosis");
        }

        public ScenarioController.Trigger.Action GetCorrectAction()
        {
            var diseaseAnswers = new List<string>();
            var actionName = "";
            
            var scenarioId = GameManager.Instance.scenarioLoader.CurrentScenario.id;
            BookDatabase.Instance.MedicalBook.diagnosisById.TryGetValue(scenarioId, out var diagnosis);
            if(diagnosis == null) return null;
            
            var icdVersion = PlayerPrefs.GetInt("ICD_VERSION");

            var underlyingDisease = icdVersion switch
            {
                0 => diagnosis.underlyingDiseaseICD10,
                1 => diagnosis.underlyingDiseaseICD11,
                _ => default
            };
            
            var concomitantDiseases = icdVersion switch
            {
                0 => diagnosis.concomitantDiseasesICD10,
                1 => diagnosis.concomitantDiseasesICD11,
                _ => default
            };
            
            var icd = icdVersion switch
            {
                0 => BookDatabase.Instance.MedicalBook.ICD10ById,
                1 => BookDatabase.Instance.MedicalBook.ICD11ById,
                _ => default
            };
            
            if(icd == default) return null;

            if (!string.IsNullOrEmpty(underlyingDisease))
            {
                icd.TryGetValue(underlyingDisease, out var diseaseName);

                if (diseaseName != null)
                {
                    diseaseAnswers.Add(diseaseName.id);
                    actionName += TextData.Get(144) + ":\n   " + $"{diseaseName.id} {diseaseName.name}" + "\n\n";
                }
            }

            if (concomitantDiseases is {Length: > 0})
            {
                actionName += TextData.Get(145) + ":";
                foreach (var diseaseId in concomitantDiseases)
                {
                    icd.TryGetValue(diseaseId, out var diseaseName);
                    diseaseAnswers.Add(diseaseName?.id);
                    actionName += "\n   " + $"{diseaseName?.id} {diseaseName?.name}";
                }
            }
            
            return string.IsNullOrEmpty(actionName) ? null :
                new ScenarioController.Trigger.Action {actionName = actionName, correctAnswers = diseaseAnswers};
        }
        
        private void SetActivePanel(bool val)
        {
            _view.root.SetActive(val);
        }
        
        public void Clean()
        {
            isFilled = false;
            _view.Clean();
            passedAnswers.Clear();
            GameManager.Instance.mainMenuController.RemovePopUpModule("DiagnosisSelector");
        }
    }
}
