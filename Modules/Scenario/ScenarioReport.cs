using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Modules.Books;
using Modules.MainMenu;
using Modules.WDCore;
using Modules.Statistics;
using UnityEngine;

namespace Modules.Scenario
{
    public class ScenarioReport
    {
        public Action<int, string> onScoreAndPathSet;
        public IEnumerator CreateReport(bool isSimulation)
        {
            var triggers = GameManager.Instance.scenarioController.GetAllTriggers();
            if(triggers == null) yield break;
            var isAnyDone = triggers.SelectMany(x => x.Value.requiredAction.Values).Any(y => y.isDone);
            if(!isAnyDone && !isSimulation) yield break;
            
            var pdfCreator = new PdfCreator();
            var checkTableInstance = GameManager.Instance.checkTableController.GetCheckTable();
            
            var headers = new[]
            {
                TextData.Get(125),
                TextData.Get(126),
                TextData.Get(127),
                TextData.Get(128)
            };
            
            var widths = isSimulation ? new[] { 30, 70, 0, 0} : new[] { 50, 50, 50, 12};
            var startTime = GameManager.Instance.scenarioLoader.StartTime;
            var scenario = GameManager.Instance.scenarioLoader.CurrentScenario;
            var mode = GameManager.Instance.scenarioLoader.Mode;
            var date = startTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            var time = startTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var specs = scenario.specializationIds;
            var specialization = TextData.Get(64) + ": ";
            var modeTxt = mode switch
            {
                ScenarioModel.Mode.Learning => TextData.Get(207),
                ScenarioModel.Mode.Selfcheck => TextData.Get(208),
                ScenarioModel.Mode.Exam => TextData.Get(209),
                _ => ""
            };
            var modeTitle = TextData.Get(221) + ": " + modeTxt;
            var totalScore = 0;
            var username = GameManager.Instance.profileController.GetUsername();
            var testerName = $"{TextData.Get(249)}: " + username;

            for (var i = 0; i < specs.Length; ++i)
            {
                BookDatabase.Instance.MedicalBook.specializationById.TryGetValue(specs[i], out var sp);
                specialization += i == 0 ? sp?.name : ", " + sp?.name;
            }

            var vitalsInfo = $"{TextData.Get(298)}: {scenario.pulse}  |  " +
                             $"{TextData.Get(299)}: {scenario.breath}  |  " +
                             $"{TextData.Get(300)}: {scenario.pressure}  |  " +
                             $"{TextData.Get(301)}: {scenario.saturation}  |  " +
                             $"{TextData.Get(302)}: {scenario.temperature}";
            

            pdfCreator.CreateTable(headers, widths);

            foreach (var action in checkTableInstance.actions)
            {
                if (action.level == "0")
                {
                    pdfCreator.AddHeaderToMainTable(action.name);
                    continue;
                }
                
                triggers.TryGetValue(action.id, out var answer);
                if (answer == null) continue;
                
                var correctAction = answer.requiredAction.
                    Aggregate("", (current, action1) => current + (GetCorrectAnswer(action1) + "\n"));

                var actualAnswers = new List<(string, int)>();
                var isToSkip = false;
                foreach (var action1 in answer.requiredAction)
                {
                    if (action1.Key == "skip")
                    {
                        isToSkip = true;
                        continue;
                    }
                    
                    if(action1.Key == "continue")
                        actualAnswers.Add((TextData.Get(130), 1));
                    else if(!action1.Value.isDone)
                        actualAnswers.Add((TextData.Get(131), -1));
                    else if (action1.Value.isDone && action1.Key == "SelectAuscultation")
                    {
                        CheckOutOfOrder(answer, actualAnswers);
                        actualAnswers.AddRange(CheckPhysicalExam(Config.AuscultationParentId));
                    }
                    else if (action1.Value.isDone && action1.Key == "SelectVisualExam")
                    {
                        CheckOutOfOrder(answer, actualAnswers);
                        actualAnswers.AddRange(CheckPhysicalExam(Config.VisualExamParentId));
                    }
                    else if (action1.Value.isDone && action1.Key == "SelectPalpation")
                    {
                        CheckOutOfOrder(answer, actualAnswers);
                        actualAnswers.AddRange(CheckPhysicalExam(Config.PalpationParentId));
                    }
                    else if (action1.Value.isDone && action1.Key == "SelectPercussion")
                    {
                        CheckOutOfOrder(answer, actualAnswers);
                        actualAnswers.AddRange(CheckPhysicalExam(Config.PercussionParentId));
                    }
                    else if (action1.Value.isDone && action1.Key == "SelectComplaint")
                    {
                        CheckOutOfOrder(answer, actualAnswers);
                        actualAnswers.AddRange(CheckComplaint());
                    }
                    else if (action1.Value.isDone && action1.Key == "SelectAnamLife")
                    {
                        CheckOutOfOrder(answer, actualAnswers);
                        actualAnswers.AddRange(CheckAnamLife());
                    }
                    else if (action1.Value.isDone && action1.Key == "SelectAnamDisease")
                    {
                        CheckOutOfOrder(answer, actualAnswers);
                        actualAnswers.AddRange(CheckAnamDisease());
                    }
                    else if (action1.Value.isDone && action1.Key == "SelectLab")
                    {
                        CheckOutOfOrder(answer, actualAnswers);
                        actualAnswers.AddRange(CheckLab(action1.Value, 0));
                    }
                    else if (action1.Value.isDone && action1.Key == "SelectInstrumental")
                    {
                        CheckOutOfOrder(answer, actualAnswers);
                        actualAnswers.AddRange(CheckLab(action1.Value, 1));
                    }
                    else if (action1.Value.isDone && action1.Key == "SelectDiagnosis")
                    {
                        CheckOutOfOrder(answer, actualAnswers);
                        actualAnswers.AddRange(CheckDiagnosis(action1.Value));
                    }
                    else if (action1.Value.isDone && action1.Key == "SelectTreatment")
                    {
                        CheckOutOfOrder(answer, actualAnswers);
                        actualAnswers.AddRange(CheckTreatment(action1.Value));
                    }
                    else if (action1.Value.isDone && answer.isDoneInOrder)
                    {
                        actualAnswers.AddRange(GetActualAnswer(action1, true));
                    } else if (action1.Value.isDone && !answer.isDoneInOrder)
                    {
                        actualAnswers.AddRange(GetActualAnswer(action1, false));
                    }
                }

                var grade = CalculateGrade(actualAnswers);
                totalScore += Mathf.RoundToInt(grade * action.gradeMax / 100.0f);
                
                if(!isToSkip)
                    pdfCreator.AddCellToMainTable(action.name, correctAction, actualAnswers, grade);
            }

            pdfCreator.AddLastLine(TextData.Get(248), totalScore);
            
            var statData = new StatisticsManager.Statistics.Item
            {
                date = date,
                time = time,
                score = totalScore,
                mode = (int) mode
            };
            
            var path = "";
            yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(
                GameManager.Instance.statisticsManager.AddReport(scenario.id, scenario.specializationIds, 
                    statData, 2, val => path = val, isSimulation));
            
            pdfCreator.CreatePdf(path);
            pdfCreator.AddHeader(TextData.Get(129) + ": " + 
                                 scenario.name);
            pdfCreator.AddCustomParagraph(scenario.patientInfo, 1);
            pdfCreator.AddCustomParagraph(vitalsInfo, 1);

            if (!isSimulation)
            {
                if(!string.IsNullOrEmpty(username))
                    pdfCreator.AddCustomParagraph(testerName);
                
                pdfCreator.AddDate(statData, startTime, mode);
                pdfCreator.AddCustomParagraph(modeTitle);
                pdfCreator.AddCustomParagraph(specialization);
                pdfCreator.AddCustomParagraph($"{TextData.Get(212)}: {totalScore}%");

                pdfCreator.AddNewLine();
                pdfCreator.AddCustomParagraph(scenario.description);
                pdfCreator.AddNewLine();
                pdfCreator.AddCustomLink(TextData.Get(149), "Academix3D Page", ProfileController.WebsiteUrl);
            }

            pdfCreator.AddNewLine();
            pdfCreator.AddNewLine();
            pdfCreator.FinishPdf();
            
            onScoreAndPathSet?.Invoke(totalScore, path);
        }

        private string GetCorrectAnswer(KeyValuePair<string, ScenarioController.Trigger.Action> answer)
        {
            var action = answer.Value;
            action.isCorrect = true;
            var actionName = GetAnswer(answer);
            
            if (action.checkUpTrigger == null || 
                action.checkUpTrigger.GetInfo().typeId == "6" || 
                action.checkUpTrigger.GetInfo().typeId == "7") return actionName;
     
            var answers = GameManager.Instance.physicalExamController.passedAnswers;
            answers.TryGetValue(answer.Value.checkUpTrigger.id, out var ans);
            if (action.checkUpTrigger.children.Count == 0)
            {
                actionName += " - " + TextData.Get(74);
                action.isCorrect = ans == 1;
            }
            else
            {
                actionName += " - " + TextData.Get(112);
                action.isCorrect = ans == 2;
            }
            
            return actionName;
        }

        private List<(string, int)> GetActualAnswer(KeyValuePair<string, ScenarioController.Trigger.Action> answer, bool isInOrder)
        {
            var actualAnswers = new List<(string, int)>();
            var action = answer.Value;
            var actionName = GetAnswer(answer);
            
            if (string.IsNullOrEmpty(actionName)) actionName = TextData.Get(211);
            if (action.checkUpTrigger == null)
            {
                actualAnswers.Add((actionName, answer.Value.isCorrect ? 1 : -1));
                if(!isInOrder) actualAnswers.Add((" - " + TextData.Get(132), -1));
                return actualAnswers;
            }

            var pointInfo = action.checkUpTrigger.GetPointInfo();
            if (pointInfo?.values == null || pointInfo.values.Count == 0)
            {
                actualAnswers.Add((action.checkUpTrigger.GetInfo().name + ": ", 0));
                actualAnswers.AddRange(CheckVisualExamNoPointAnswer(action.checkUpTrigger));
            }
            else
            {
                if (action.isCorrect)
                    actionName += " - " + TextData.Get(74);
                else
                    actionName += " - " + TextData.Get(112);
                
                actualAnswers.Add((actionName, answer.Value.isCorrect ? 1 : -1));
            }

            if(!isInOrder) actualAnswers.Add((" - " + TextData.Get(132), -1));
            return actualAnswers;
        }
        
        private string GetAnswer(KeyValuePair<string, ScenarioController.Trigger.Action> answer)
        {
            var id = answer.Key;
            var action = answer.Value;
            var actionName = action.actionName;
            
            if (action.checkUpTrigger == null) return actionName;
            
            var type = action.checkUpTrigger.GetInfo().typeId;
            if (type == "12" || type == "9" || type == "11" || type == "10")
            {
                actionName = action.actionName + ": ";
                for (var i = 0; i < action.checkUpTrigger.children.Count; i++)
                {
                    var str = action.checkUpTrigger.children[i].GetInfo().name;
                    actionName += i == 0 ? str : ", " + str;
                }
                
                if (action.checkUpTrigger.children.Count == 0)
                    actionName += action.checkUpTrigger.GetPointInfo().description;

                actionName = GameManager.Instance.scenarioLoader.ReplaceCaseVariables(actionName);
            }

            return actionName;
        }

        private List<(string, int)> CheckDiagnosis(ScenarioController.Trigger.Action action)
        {
            var actualAnswers = new List<(string, int)> {(TextData.Get(144) + ": ", 0)};
            var passedAnswers = GameManager.Instance.diagnosisSelectorController.passedAnswers;
            
            var underlyingDiseases = passedAnswers.FirstOrDefault(x => x.Contains("underlyingDisease_"))?.
                Replace("underlyingDisease_", "");
            
            var icdVersion = PlayerPrefs.GetInt("ICD_VERSION");
            var icd = icdVersion switch
            {
                0 => BookDatabase.Instance.MedicalBook.ICD10ById,
                1 => BookDatabase.Instance.MedicalBook.ICD11ById,
                _ => default
            };
            
            if(icd == default) return actualAnswers;

            if (underlyingDiseases != null)
            {
                icd.TryGetValue(action.correctAnswers[0], out var diseaseName);
                actualAnswers.Add(underlyingDiseases == action.correctAnswers[0]
                    ? ($"   {diseaseName?.id} {diseaseName?.name} - {TextData.Get(217)}", 1)
                    : ($"   {diseaseName?.id} {diseaseName?.name} - {TextData.Get(218)}", -1));

                if (underlyingDiseases != action.correctAnswers[0])
                {
                    icd.TryGetValue(underlyingDiseases, out diseaseName);
                    actualAnswers.Add(($"   {diseaseName?.id} {diseaseName?.name} - {TextData.Get(216)}", -1));
                }
            
                passedAnswers.RemoveAll(x=> x.Contains("underlyingDisease_"));
                action.correctAnswers.RemoveAt(0);
            }
            
            actualAnswers.Add(("", 0));
            actualAnswers.Add((TextData.Get(145) + ": ", 0));
            var countBefore = actualAnswers.Count;
            
            foreach (var concomitantDisease in action.correctAnswers)
            {
                icd.TryGetValue(concomitantDisease, out var diseaseName);
                actualAnswers.Add(passedAnswers.Contains("concomitantDiseases_" + concomitantDisease)
                    ? ($"   {diseaseName?.id} {diseaseName?.name} - {TextData.Get(217)}", 1)
                    : ($"   {diseaseName?.id} {diseaseName?.name} - {TextData.Get(218)}", -1));
            }

            foreach (var actionAnswer in passedAnswers)
            {
                var id = actionAnswer.Replace("concomitantDiseases_", "");
                if(action.correctAnswers.Contains(id)) continue;
                icd.TryGetValue(id, out var diseaseName);
                actualAnswers.Add(($"   {diseaseName?.id} {diseaseName?.name} - {TextData.Get(216)}", -1));
            }
            
            if(countBefore == actualAnswers.Count)
                actualAnswers.RemoveAt(actualAnswers.Count-1);
            
            return actualAnswers;
        }
        
        private List<(string, int)> CheckTreatment(ScenarioController.Trigger.Action action)
        {
            var actualAnswers = new List<(string, int)>();
            var passedAnswers = GameManager.Instance.treatmentSelectorController.passedAnswers;
            var status = GameManager.Instance.scenarioLoader.CurrentScenario;

            // Recommendations
            var treatments = status.treatments.Where(x => x.StartsWith("RE"))
                .Select(x => BookDatabase.Instance.MedicalBook.recommendationById[x]).ToList();

            actualAnswers.Add((TextData.Get(159) + ": ", 0));
            var countBefore = actualAnswers.Count;
            actualAnswers.AddRange(treatments.Select(treatment => passedAnswers.Contains(treatment.id)
                ? ("   " + treatment.name + $" - {TextData.Get(217)}", 1)
                : ("   " + treatment.name + $" - {TextData.Get(218)}", -1)));

            var treatmentIds = treatments.Select(x => x.id).ToList();
            foreach (var answer in passedAnswers.Where(x => x.StartsWith("RE") && !treatmentIds.Contains(x)))
            {
                BookDatabase.Instance.MedicalBook.recommendationById.TryGetValue(answer, out var extraTreatment);
                if(extraTreatment == null) continue;
                actualAnswers.Add(("   " + extraTreatment.name+ $" - {TextData.Get(216)}", -1));
            }
            
            if(countBefore == actualAnswers.Count)
                actualAnswers.RemoveAt(actualAnswers.Count-1);
            actualAnswers.Add(("", 0));

            // Surgeries
            treatments = status.treatments.Where(x => x.StartsWith("SG"))
                .Select(x => BookDatabase.Instance.MedicalBook.surgeryById[x]).ToList();

            actualAnswers.Add((TextData.Get(158) + ": ", 0));
            countBefore = actualAnswers.Count;
            actualAnswers.AddRange(treatments.Select(treatment => passedAnswers.Contains(treatment.id)
                ? ("   " + treatment.name + $" - {TextData.Get(217)}", 1)
                : ("   " + treatment.name + $" - {TextData.Get(218)}", -1)));

            treatmentIds = treatments.Select(x => x.id).ToList();
            foreach (var answer in passedAnswers.Where(x => x.StartsWith("SG") && !treatmentIds.Contains(x)))
            {
                BookDatabase.Instance.MedicalBook.surgeryById.TryGetValue(answer, out var extraTreatment);
                if(extraTreatment == null) continue;
                actualAnswers.Add(("   " + extraTreatment.name+ $" - {TextData.Get(216)}", -1));
            }
            
            if(countBefore == actualAnswers.Count)
                actualAnswers.RemoveAt(actualAnswers.Count-1);
            actualAnswers.Add(("", 0));
            
            // Therapies
            treatments = status.treatments.Where(x => x.StartsWith("TH")).
                Select(x => BookDatabase.Instance.MedicalBook.therapyById[x]).ToList();

            actualAnswers.Add((TextData.Get(160) + ": ", 0));
            countBefore = actualAnswers.Count;
            actualAnswers.AddRange(treatments.Select(treatment => passedAnswers.Contains(treatment.id)
                ? ("   " + treatment.name + $" - {TextData.Get(217)}", 1)
                : ("   " + treatment.name + $" - {TextData.Get(218)}", -1)));

            treatmentIds = treatments.Select(x => x.id).ToList();
            foreach (var answer in passedAnswers.Where(x => x.StartsWith("TH") && !treatmentIds.Contains(x)))
            {
                BookDatabase.Instance.MedicalBook.therapyById.TryGetValue(answer, out var extraTreatment);
                if(extraTreatment == null) continue;
                actualAnswers.Add(("   " + extraTreatment.name+ $" - {TextData.Get(216)}", -1));
            }
            
            if(countBefore == actualAnswers.Count)
                actualAnswers.RemoveAt(actualAnswers.Count-1);
            actualAnswers.Add(("", 0));
            
            // ATC
            treatments = status.treatments.Where(x => !x.StartsWith("TH") && !x.StartsWith("SG") && !x.StartsWith("RE")).
                Select(x => BookDatabase.Instance.MedicalBook.ATCById[x]).ToList();

            actualAnswers.Add((TextData.Get(157) + ": ", 0));
            countBefore = actualAnswers.Count;
            actualAnswers.AddRange(treatments.Select(treatment => passedAnswers.Contains(treatment.id)
                ? ("   " + treatment.name + $" - {TextData.Get(217)}", 1)
                : ("   " + treatment.name + $" - {TextData.Get(218)}", -1)));

            treatmentIds = treatments.Select(x => x.id).ToList();
            foreach (var answer in passedAnswers.Where(x => !x.StartsWith("TH") && !x.StartsWith("SG") && !x.StartsWith("RE") && !treatmentIds.Contains(x)))
            {
                BookDatabase.Instance.MedicalBook.ATCById.TryGetValue(answer, out var extraTreatment);
                if(extraTreatment == null) continue;
                actualAnswers.Add(("   " + extraTreatment.name+ $" - {TextData.Get(216)}", -1));
            }
            
            if(countBefore == actualAnswers.Count)
                actualAnswers.RemoveAt(actualAnswers.Count-1);
            actualAnswers.Add(("", 0));

            return actualAnswers;
        }
        
        private List<(string, int)> CheckLab(ScenarioController.Trigger.Action action, int type)
        {
            var passedAnswers = type == 0 ? 
                GameManager.Instance.labSelectorController.passedAnswers:
                GameManager.Instance.instrumentalSelectorController.passedAnswers;
            
            var actualAnswers = new List<(string, int)>();

            var allCheckups = BookDatabase.Instance.allCheckUps.FirstOrDefault(x =>
                (type == 0 ? Config.LabResearchParentId : Config.InstrResearchParentd) == x.id);
            
            foreach (var allInGroup in allCheckups.children)
            {
                actualAnswers.Add((allInGroup.name + ":", 0));
                var isAny = false;
                var researchChild = action.checkUpTrigger.children.FirstOrDefault(x => x.id == allInGroup.id);
                
                foreach (var allGroupChild in allInGroup.children)
                {
                    var childChild = researchChild?.children.FirstOrDefault(x => x.id == allGroupChild.id);
                    if (childChild == null)
                    {
                        if(passedAnswers.Contains(allGroupChild.id))
                            actualAnswers.Add(("   " + allGroupChild.name + $" - {TextData.Get(216)}", -1));
                        continue;
                    }
                    if (childChild.children.Count == 0 && !passedAnswers.Contains(childChild.id))
                        continue;
                    if (childChild.children.Count == 0 && passedAnswers.Contains(childChild.id))
                        actualAnswers.Add(("   " + childChild.GetInfo().name + $" - {TextData.Get(216)}", -1));
                    else if(passedAnswers.Contains(childChild.id))
                        actualAnswers.Add(("   " + childChild.GetInfo().name + $" - {TextData.Get(217)}", 1));
                    else if(!passedAnswers.Contains(childChild.id))
                        actualAnswers.Add(("   " + childChild.GetInfo().name + $" - {TextData.Get(218)}", -1));

                    isAny = true;
                }
                if(!isAny)
                    actualAnswers.RemoveAt(actualAnswers.Count-1);
                else
                    actualAnswers.Add(("", 0));
            }
            
            return actualAnswers;
        }
        
        private List<(string, int)> CheckComplaint()
        {
            var actualAnswers = new List<(string, int)>();
            var correctAnswers = GameManager.Instance.complaintSelectorController.correctAnswers;
            var passedAnswers = GameManager.Instance.complaintSelectorController.passedAnswers;
            foreach (var answer in correctAnswers)
            {
                var level = answer.GetInfo().level - 3;
                var answerName = answer.GetInfo().name;
                var isAnswered = passedAnswers.Contains(answer.id);
                var answerTxt = answerName.PadLeft(answerName.Length + level, ' ')
                                + (isAnswered ? $" - {TextData.Get(211)}" : $" - {TextData.Get(131)}");
                actualAnswers.Add((answerTxt, isAnswered ? 1:-1));
            }

            return actualAnswers;
        }
        
        private List<(string, int)> CheckAnamLife()
        {
            var actualAnswers = new List<(string, int)>();
            var correctAnswers = GameManager.Instance.anamnesisLifeSelectorController.correctAnswers;
            var passedAnswers = GameManager.Instance.anamnesisLifeSelectorController.passedAnswers;
            foreach (var answer in correctAnswers)
            {
                var answerName = answer.GetInfo().name;
                var isAnswered = passedAnswers.Contains(answer.id);
                var answerTxt = answerName + (isAnswered ? $" - {TextData.Get(211)}" : $" - {TextData.Get(131)}");
                actualAnswers.Add((answerTxt, isAnswered ? 1:-1));
            }

            return actualAnswers;
        }
        private List<(string, int)> CheckAnamDisease()
        {
            var actualAnswers = new List<(string, int)>();
            var correctAnswers = GameManager.Instance.anamnesisDiseaseSelectorController.correctAnswers;
            var passedAnswers = GameManager.Instance.anamnesisDiseaseSelectorController.passedAnswers;
            foreach (var answer in correctAnswers)
            {
                var answerName = answer.GetInfo().name;
                var isAnswered = passedAnswers.Contains(answer.id);
                var answerTxt = answerName + (isAnswered ? $" - {TextData.Get(211)}" : $" - {TextData.Get(131)}");
                actualAnswers.Add((answerTxt, isAnswered ? 1:-1));
            }

            return actualAnswers;
        }

        private void CheckOutOfOrder(ScenarioController.Trigger answer, List<(string, int)> actualAnswers)
        {
            if (answer.isDoneInOrder) return;
            actualAnswers.Add((TextData.Get(132), -1));
            actualAnswers.Add(("", 0));
        }

        private List<(string, int)> CheckPhysicalExam(string groupId)
        {
            var actualAnswers = new List<(string, int)>();
            
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var mainCheckUp = caseInstance.FullStatus.checkUps.FirstOrDefault(x => x.id == groupId);
            var passedAnswers = GameManager.Instance.physicalExamController.passedAnswers;

            if (mainCheckUp?.children != null)
            {
                foreach (var answer in mainCheckUp.children)
                {
                    var actionName =  answer.GetInfo().name + ": ";
                    var pointInfo = answer.GetPointInfo();

                    if (groupId == Config.VisualExamParentId &&
                        (pointInfo?.values == null || pointInfo.values.Count == 0))
                    {
                        actualAnswers.Add((actionName, 0));
                        actualAnswers.AddRange(CheckVisualExamNoPointAnswer(answer));
                    }
                    else
                    {
                        passedAnswers.TryGetValue(answer.id, out var ans);
                        var isNorma = answer.children.Count == 0;
                        if(!isNorma)
                            actionName +=  answer.children[0].GetInfo().name;
                        else
                            actionName +=  pointInfo.description;

                        actionName +=  " - " + TextData.Get(ans == 2 ? 112 : ans == 1 ? 74 : 218);
                        actionName = GameManager.Instance.scenarioLoader.ReplaceCaseVariables(actionName);
                        actualAnswers.Add((actionName, isNorma && ans == 1 || !isNorma && ans == 2 ? 1 : -1));
                    }
                    actualAnswers.Add(("", 0));
                }

                var childrenIds = mainCheckUp.children.Select(x => x.id).ToList();
                var appInstance = BookDatabase.Instance.allCheckUps.FirstOrDefault(x => x.id == groupId);
                var allGroupChildren = appInstance?.children.Select(x => x.id).ToList();
                foreach (var passedAnswer in passedAnswers)
                {
                    if(childrenIds.Contains(passedAnswer.Key) || 
                       (allGroupChildren != null && !allGroupChildren.Contains(passedAnswer.Key))) continue;
                    
                    var fullCheckUp = appInstance?.children.FirstOrDefault(x => x.id == passedAnswer.Key);
                    var actionName =  fullCheckUp?.name + ": " + fullCheckUp?.GetPointInfo().description;
                    actionName +=  " - " + TextData.Get(passedAnswer.Value == 2 ? 112 : 74);
                    actionName = GameManager.Instance.scenarioLoader.ReplaceCaseVariables(actionName);
                    actualAnswers.Add((actionName, passedAnswer.Value == 1 ? 1 : -1));
                    actualAnswers.Add(("", 0));
                }

                if (groupId == Config.VisualExamParentId)
                {
                    var passedVisualAnswers = GameManager.Instance.visualExamController.passedAnswers;

                    foreach (var passedVisualAnswer in passedVisualAnswers)
                    {
                        if(childrenIds.Contains(passedVisualAnswer.Key)) continue;
                        var fullCheckUp = appInstance?.children.FirstOrDefault(x => x.id == passedVisualAnswer.Key);
                        var actionName =  fullCheckUp?.name + ": ";
                        
                        if(passedVisualAnswer.Value.Count == 1 && passedVisualAnswer.Value[0] == fullCheckUp?.GetPointInfo().id)
                            actualAnswers.Add((actionName + fullCheckUp?.GetPointInfo().description, 1));
                        else
                        {
                            actionName += string.Join(", ", fullCheckUp?.children.
                                Where(x => passedVisualAnswer.Value.Contains(x.id)).Select(x => x.name).ToList()!);
                            actualAnswers.Add((actionName + $" - {TextData.Get(216)}", -1));
                        }
                        actualAnswers.Add(("", 0));
                    }
                }
                
            }

            return actualAnswers;
        }
        
        private List<(string, int)> CheckVisualExamNoPointAnswer(StatusInstance.Status.CheckUp answer)
        {
            var actualAnswers = new List<(string, int)>();
            var passedAnswers = GameManager.Instance.visualExamController.passedAnswers;
            passedAnswers.TryGetValue(answer.id, out var selectedAns);
            
            foreach (var answerChild in answer.children)
            {
                if (selectedAns != null && selectedAns.Contains(answerChild.id))
                    actualAnswers.Add((answerChild.GetInfo().name + $" - {TextData.Get(217)}", 1));
                else
                    actualAnswers.Add((answerChild.GetInfo().name + $" - {TextData.Get(218)}", -1));
            }

            if (answer.children.Count == 0)
            {
                if (selectedAns != null && selectedAns.Contains(answer.GetPointInfo().id))
                    actualAnswers.Add((answer.GetPointInfo().description + $" - {TextData.Get(217)}", 1));
                else
                    actualAnswers.Add((answer.GetPointInfo().description + $" - {TextData.Get(218)}", -1));
            }

            if (selectedAns != null)
            {
                var childrenIds = answer.children.Select(x => x.id).ToList();
                var appInstance = BookDatabase.Instance.allCheckUps.FirstOrDefault(x => x.id == Config.VisualExamParentId);
                var fullCheckUp = appInstance?.children.FirstOrDefault(x => x.id == answer.id);
            
                foreach (var selected in selectedAns)
                {
                    if (!childrenIds.Contains(selected))
                    {
                        var selectedFull = fullCheckUp?.children.FirstOrDefault(x => x.id == selected);
                        
                        if (selectedFull == null && selected == answer.GetPointInfo().id && answer.children.Count != 0)
                            actualAnswers.Add((answer.GetPointInfo().description + $" - {TextData.Get(216)}", -1));
                        else if (selectedFull != null)
                            actualAnswers.Add((selectedFull.name + $" - {TextData.Get(216)}", -1));
                    }
                }
            }

            return actualAnswers;
        }

        private int CalculateGrade(List<(string, int)> answers)
        {
            var correctCount = answers.Count(x => x.Item2 == 1);
            var incorrectCount = answers.Count(x => x.Item2 == -1);
            var totalCount = correctCount + incorrectCount;
            totalCount = totalCount == 0 ? 1 : totalCount;
            
            var grade = 100.0f*correctCount / totalCount;
            return Mathf.RoundToInt(grade);
        }
    }
}
