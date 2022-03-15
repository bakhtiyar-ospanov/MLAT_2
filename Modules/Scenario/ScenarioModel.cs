using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Scenario
{
    public class ScenarioModel
    {
        public enum Mode
        {
            Learning,
            Selfcheck,
            Exam
        }
        public int actionIndex;
        public int sectionIndex;
        public string audioFolder;
        public bool isStarted;
        public string lastHint;
        public Mode mode;
        public bool isSimulation;
        public Dictionary<int, List<CheckTable.Action>> actionsBySection;
        public CheckTable.Action alternativeAction;
        public Dictionary<string, ScenarioController.Trigger> allTriggers;
        public Coroutine timerCoroutine;
        private List<string> _instrumentsExceptions = new List<string> {"6190", "6188", "6189", "3856", "6186"};

        public ScenarioModel(Mode _mode, bool _isSimulation)
        {
            mode = _mode;
            isSimulation = _isSimulation;
            
            var checkTable = GameManager.Instance.checkTableController.GetCheckTable();
            var sectionId = -1;
            actionsBySection = new Dictionary<int, List<CheckTable.Action>>();
            foreach (var tableAction in checkTable.actions)
            {
                if (tableAction.level == "0")
                {
                    sectionId++;
                    actionsBySection.Add(sectionId, new List<CheckTable.Action>());
                }
                else
                {
                    actionsBySection[sectionId].Add(tableAction);
                }
            }
            allTriggers = ParseAllTriggers();
            
            actionIndex = 0;
            sectionIndex = 0;
            lastHint = null;
            audioFolder = DirectoryPath.CheckTables + checkTable.id + "/audio";
            DirectoryPath.CheckDirectory(audioFolder);
            isStarted = true;
        }
        
        private Dictionary<string, ScenarioController.Trigger> ParseAllTriggers()
        {
            var allTriggers = new Dictionary<string, ScenarioController.Trigger>();
            foreach (var section in actionsBySection)
            {
                foreach (var action in section.Value)
                {
                    var trigger = new ScenarioController.Trigger {requiredAction = new Dictionary<string, ScenarioController.Trigger.Action>()};
                    var strTrigger = action.trigger;
                    if (string.IsNullOrEmpty(strTrigger))
                    {
                        if(!string.IsNullOrEmpty(action.nameButton))
                            trigger.requiredAction.Add("dialog_" + action.id, new ScenarioController.Trigger.Action
                                {actionName = action.nameButton});
                        else
                            trigger.requiredAction.Add("continue", new ScenarioController.Trigger.Action {actionName = TextData.Get(4202)});
                    }
                    else 
                    {
                        strTrigger = strTrigger.Replace(" ", "");
                        if (strTrigger.Contains(","))
                        {
                            var strTriggers = strTrigger.Split(',');
                            foreach (var s in strTriggers)
                                trigger.requiredAction = trigger.requiredAction
                                    .Concat(ParseTrigger(s)).GroupBy(d => d.Key)
                                    .ToDictionary(d => 
                                        d.Key, d => d.First().Value);;
                        
                            
                        }
                        else
                        {
                            trigger.requiredAction  = ParseTrigger(strTrigger);
                        }
                    }
                
                    allTriggers.Add(action.id, trigger);
                }
                
            }

            return allTriggers;
            
        }

        private Dictionary<string, ScenarioController.Trigger.Action> ParseTrigger(string strTrigger)
        {
            var requiredActions = new Dictionary<string, ScenarioController.Trigger.Action>();
            
            if (!_instrumentsExceptions.Contains(strTrigger) && int.TryParse(strTrigger, out _))
            {
                var answerCheckUp = GameManager.Instance.physicalExamController.GetCheckUpById(strTrigger);

                if(answerCheckUp != null)
                    requiredActions.Add(answerCheckUp.id, new ScenarioController.Trigger.Action
                        {actionName = answerCheckUp.GetInfo().name, checkUpTrigger = answerCheckUp});
                else
                    Debug.LogWarning("!!! REQUIRED CHECKUP: " + strTrigger);
            }
            else
            {
                switch (strTrigger)
                {
                    case "SelectDiagnosis":
                        var action = GameManager.Instance.diagnosisSelectorController.GetCorrectAction();
                        if(action == null)
                        {
                            GameManager.Instance.diseaseHistoryController.RemoveGroup("diagnosis");
                            requiredActions.Add("skip", new ScenarioController.Trigger.Action());
                        }
                        else
                            requiredActions.Add("SelectDiagnosis", action);
                        break;
                    case "SelectTreatment":
                        action = GameManager.Instance.treatmentSelectorController.GetCorrectAction();
                        if(action == null)
                        {
                            GameManager.Instance.diseaseHistoryController.RemoveGroup("treatments");
                            requiredActions.Add("skip", new ScenarioController.Trigger.Action());
                        }
                        else
                            requiredActions.Add("SelectTreatment", action);
                        break;
                    case "SelectLab":
                        action = GameManager.Instance.labSelectorController.GetCorrectAction(isSimulation);
                        if (action == null)
                        {
                            GameManager.Instance.diseaseHistoryController.RemoveGroup(Config.LabResearchParentId);
                            requiredActions.Add("skip", new ScenarioController.Trigger.Action());
                        }
                        else
                            requiredActions.Add("SelectLab", action);
                        break;
                    case "SelectInstrumental":
                        action = GameManager.Instance.instrumentalSelectorController.GetCorrectAction(isSimulation);
                        if(action == null)
                        {
                            GameManager.Instance.diseaseHistoryController.RemoveGroup(Config.InstrResearchParentd);
                            requiredActions.Add("skip", new ScenarioController.Trigger.Action());
                        }
                        else
                            requiredActions.Add("SelectInstrumental", action);
                        break;
                    case "SelectComplaint":
                        action = GameManager.Instance.complaintSelectorController.GetCorrectAction();
                        if (action == null)
                        {
                            GameManager.Instance.diseaseHistoryController.RemoveGroup(Config.ComplaintParentId);
                            requiredActions.Add("skip", new ScenarioController.Trigger.Action());
                        }
                        else
                            requiredActions.Add("SelectComplaint", action);
                        break;
                    case "SelectAnamLife":
                        action = GameManager.Instance.anamnesisLifeSelectorController.GetCorrectAction();
                        if (action == null)
                        {
                            GameManager.Instance.diseaseHistoryController.RemoveGroup(Config.AnamnesisLifeParentId);
                            requiredActions.Add("skip", new ScenarioController.Trigger.Action());
                        }
                        else
                            requiredActions.Add("SelectAnamLife", action);
                        break;
                    case "SelectAnamDisease":
                        action = GameManager.Instance.anamnesisDiseaseSelectorController.GetCorrectAction();
                        if (action == null)
                        {
                            GameManager.Instance.diseaseHistoryController.RemoveGroup(Config.AnamnesisDiseaseParentId);
                            requiredActions.Add("skip", new ScenarioController.Trigger.Action());
                        }
                        else
                            requiredActions.Add("SelectAnamDisease", action);
                        break;
                    case "SelectAuscultation":
                        action = GameManager.Instance.physicalExamController.GetCorrectAction(Config.AuscultationParentId);
                        if (action == null)
                        {
                            GameManager.Instance.diseaseHistoryController.RemoveGroup(Config.AuscultationParentId);
                            requiredActions.Add("skip", new ScenarioController.Trigger.Action());
                        }
                        else
                            requiredActions.Add("SelectAuscultation", action);
                        break;
                    case "SelectVisualExam":
                        action = GameManager.Instance.physicalExamController.GetCorrectAction(Config.VisualExamParentId);
                        if (action == null)
                        {
                            GameManager.Instance.diseaseHistoryController.RemoveGroup(Config.VisualExamParentId);
                            requiredActions.Add("skip", new ScenarioController.Trigger.Action());
                        }
                        else
                            requiredActions.Add("SelectVisualExam", action);
                        break;
                    case "SelectPalpation":
                        action = GameManager.Instance.physicalExamController.GetCorrectAction(Config.PalpationParentId);
                        if (action == null)
                        {
                            GameManager.Instance.diseaseHistoryController.RemoveGroup(Config.PalpationParentId);
                            requiredActions.Add("skip", new ScenarioController.Trigger.Action());
                        }
                        else
                            requiredActions.Add("SelectPalpation", action);
                        break;
                    case "SelectPercussion":
                        action = GameManager.Instance.physicalExamController.GetCorrectAction(Config.PercussionParentId);
                        if (action == null)
                        {
                            GameManager.Instance.diseaseHistoryController.RemoveGroup(Config.PercussionParentId);
                            requiredActions.Add("skip", new ScenarioController.Trigger.Action());
                        }
                        else
                            requiredActions.Add("SelectPercussion", action);
                        break;
                    default:
                        requiredActions.Add(strTrigger, new ScenarioController.Trigger.Action{actionName = ""});
                        break;
                }
            }
            
            
            return requiredActions;
        }
        
        public List<string> GetAllTriggerIds()
        {
            var all = new List<string>();
            foreach (var action in actionsBySection.SelectMany(section => section.Value))
            {
                all.Add(string.IsNullOrEmpty(action.trigger) ? "dialog_" + action.id : action.trigger);
            }
            return all;
        }

        public Dictionary<string, ScenarioController.Trigger.Action> GetUnorderedActionsInSection()
        {
            var actionById = new Dictionary<string, ScenarioController.Trigger.Action>();

            foreach (var action in actionsBySection[sectionIndex])
            {
                if (action.unordered)
                    actionById = actionById.Concat(allTriggers[action.id].requiredAction).
                        GroupBy(d => d.Key)
                        .ToDictionary(d => 
                            d.Key, d => d.First().Value);
            }

            return actionById;
        }
    }
}
