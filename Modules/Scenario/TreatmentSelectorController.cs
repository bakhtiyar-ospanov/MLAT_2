using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Modules.Scenario
{
    public class TreatmentSelectorController : MonoBehaviour
    {
        public List<string> passedAnswers = new List<string>();
        private TreatmentSelectorView _view;
        private bool isFilled;

        private void Awake()
        {
            _view = GetComponent<TreatmentSelectorView>();
            _view.applyButton.onClick.AddListener(() =>
            {
                GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
                AddToDiseaseHistory();
            });
            _view.backToHistoryButton.onClick.AddListener(() => 
                { GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory"); });
            
        }

        public void Init()
        {
            if (isFilled)
            {
                GameManager.Instance.mainMenuController.ShowMenu("TreatmentSelector");
                return;
            }
            
            var scenario = GameManager.Instance.scenarioLoader.CurrentScenario;
            var treatments = new List<(string, List<MedicalBase.Treatment>)>();
            
            // Recommendations
            if (GameManager.Instance.scenarioController.GetMode() == ScenarioModel.Mode.Learning)
            {
                var currentRecommendations = scenario.treatments.Where(x => x.StartsWith("RE")).
                    Select(x => BookDatabase.Instance.MedicalBook.recommendationById[x]).ToList();
                treatments.Add((TextData.Get(159), currentRecommendations));
            }
            else
            {
                treatments.Add((TextData.Get(159), BookDatabase.Instance.MedicalBook.recommendations));
            }

            // Surgeries
            if (GameManager.Instance.scenarioController.GetMode() == ScenarioModel.Mode.Learning)
            {
                var currentSurgeries = scenario.treatments.Where(x => x.StartsWith("SG"))
                    .Select(x => BookDatabase.Instance.MedicalBook.surgeryById[x]).ToList();
                treatments.Add((TextData.Get(158), currentSurgeries));
            }
            else
            {
                treatments.Add((TextData.Get(158), BookDatabase.Instance.MedicalBook.surgeries));
            }
            

            // Therapies
            if (GameManager.Instance.scenarioController.GetMode() == ScenarioModel.Mode.Learning)
            {
                var currentTherapies = scenario.treatments.Where(x => x.StartsWith("TH"))
                    .Select(x => BookDatabase.Instance.MedicalBook.therapyById[x]).ToList();
                treatments.Add((TextData.Get(160), currentTherapies));
            }
            else
            {
                treatments.Add((TextData.Get(160), BookDatabase.Instance.MedicalBook.therapies));
            }
            
            
            // ATC
            var currentATCs = scenario.treatments.Where(x => !x.StartsWith("TH") && !x.StartsWith("SG") && !x.StartsWith("RE")).
                Select(x => BookDatabase.Instance.MedicalBook.ATCById[x]).ToList();

            if (GameManager.Instance.scenarioController.GetMode() != ScenarioModel.Mode.Learning)
            {
                var shuffled = BookDatabase.Instance.MedicalBook.ATCs.Where(x => !currentATCs.Contains(x))
                    .OrderBy(x => Guid.NewGuid()).ToList();

                for (var i = 0; i < Random.Range(5, 11); ++i)
                    currentATCs.Add(shuffled[i]);
            }

            currentATCs = currentATCs.OrderBy(x => Guid.NewGuid()).ToList();
            treatments.Add((TextData.Get(157), currentATCs));

            isFilled = true;
            _view.AddCheckboxGroup(treatments, passedAnswers);
            GameManager.Instance.mainMenuController.AddPopUpModule("TreatmentSelector", SetActivePanel, new []{_view.root.transform});
            GameManager.Instance.mainMenuController.ShowMenu("TreatmentSelector");
        }

        private void AddToDiseaseHistory()
        {
            var dHistory = GameManager.Instance.diseaseHistoryController;
            dHistory.CleanGroup("treatments");
            
            // Recommendations
            var answers = new List<string>();
            foreach (var val in passedAnswers)
            {
                BookDatabase.Instance.MedicalBook.recommendationById.TryGetValue(val, out var treatment);
                if(treatment != null)
                    answers.Add(treatment.name);
            }

            if (answers.Count > 0)
            {
                dHistory.AddNewValue("treatments", "Recommendations", TextData.Get(159));
                dHistory.ExpandValue("treatments", "Recommendations", "0", string.Join( ", ", answers));
            }
            
            // Surgeries
            answers.Clear();
            foreach (var val in passedAnswers)
            {
                BookDatabase.Instance.MedicalBook.surgeryById.TryGetValue(val, out var treatment);
                if(treatment != null)
                    answers.Add(treatment.name);
            }

            if (answers.Count > 0)
            {
                dHistory.AddNewValue("treatments", "Surgeries", TextData.Get(158));
                dHistory.ExpandValue("treatments", "Surgeries", "1", string.Join(", ", answers));
            }

            // Therapies
            answers.Clear();
            foreach (var val in passedAnswers)
            {
                BookDatabase.Instance.MedicalBook.therapyById.TryGetValue(val, out var treatment);
                if(treatment != null)
                    answers.Add(treatment.name);
            }

            if (answers.Count > 0)
            {
                dHistory.AddNewValue("treatments", "Therapies", TextData.Get(160));
                dHistory.ExpandValue("treatments", "Therapies", "2", string.Join(", ", answers));
            }
            
            // ATC
            answers.Clear();
            foreach (var val in passedAnswers)
            {
                BookDatabase.Instance.MedicalBook.ATCById.TryGetValue(val, out var treatment);
                if(treatment != null)
                    answers.Add(treatment.name);
            }

            if (answers.Count > 0)
            {
                dHistory.AddNewValue("treatments", "ATC", TextData.Get(157));
                dHistory.ExpandValue("treatments", "ATC", "3", string.Join(", ", answers));
            }
            
            GameManager.Instance.checkTableController.RegisterTriggerInvoke("SelectTreatment");
        }

        public ScenarioController.Trigger.Action GetCorrectAction()
        {
            var scenario = GameManager.Instance.scenarioLoader.CurrentScenario;
            var actionName = "";
            var correctAnswers = new List<string>();
            
            if(scenario.treatments == null)
                return null;
            
            // Recommendations
            var treatments = scenario.treatments.Where(x => x.StartsWith("RE"))
                .Select(x => BookDatabase.Instance.MedicalBook.recommendationById[x]).ToList();

            if (treatments.Count > 0)
            {
                actionName += TextData.Get(159) + ":";
                actionName = treatments.Aggregate(actionName, (current, treatment) => current + ("\n   " + treatment.name));
                actionName += "\n\n";
                correctAnswers.AddRange(treatments.Select(x => x.id).ToList());
            }

            // Surgeries
            treatments = scenario.treatments.Where(x => x.StartsWith("SG"))
                .Select(x => BookDatabase.Instance.MedicalBook.surgeryById[x]).ToList();
            
            if (treatments.Count > 0)
            {
                actionName += TextData.Get(158) + ":";
                actionName = treatments.Aggregate(actionName, (current, treatment) => current + ("\n   " + treatment.name));
                actionName += "\n\n";
                correctAnswers.AddRange(treatments.Select(x => x.id).ToList());
            }
               
            // Therapies
            treatments = scenario.treatments.Where(x => x.StartsWith("TH")).
                    Select(x => BookDatabase.Instance.MedicalBook.therapyById[x]).ToList();
            
            if (treatments.Count > 0)
            {
                actionName += TextData.Get(160) + ":";
                actionName = treatments.Aggregate(actionName, (current, treatment) => current + ("\n   " + treatment.name));
                actionName += "\n\n";
                correctAnswers.AddRange(treatments.Select(x => x.id).ToList());
            }
            
            // ATC
            treatments = scenario.treatments.Where(x => !x.StartsWith("TH") && !x.StartsWith("SG") && !x.StartsWith("RE")).
                    Select(x => BookDatabase.Instance.MedicalBook.ATCById[x]).ToList();
            
            if (treatments.Count > 0)
            {
                actionName += TextData.Get(157) + ":";
                actionName = treatments.Aggregate(actionName, (current, treatment) => current + ("\n   " + treatment.name));
                actionName += "\n\n";
                correctAnswers.AddRange(treatments.Select(x => x.id).ToList());
            }
            return string.IsNullOrEmpty(actionName) ? null : 
                new ScenarioController.Trigger.Action {actionName = actionName, correctAnswers = correctAnswers};
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
            GameManager.Instance.mainMenuController.RemovePopUpModule("TreatmentSelector");
        }
    }
}
