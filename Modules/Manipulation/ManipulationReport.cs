// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Globalization;
// using System.Linq;
// using System.Text.RegularExpressions;
// using iTextSharp.text;
// using Modules.Books;
// using Modules.WDCore;
// using Modules.Scenario;
// using Modules.Statistics;
// using UnityEngine;
//
// namespace Modules.Manipulation
// {
//     public class ManipulationReport
//     {
//         public Action<int, string> onScoreAndPathSet;
//         public IEnumerator CreateReport(ManipulationModel.ReportInfo reportInfo, bool isCritical)
//         {
//             if(reportInfo.answers.Length == 0) yield break;
//             
//             var pdfCreator = new PdfCreator();
//             var headers = new[]
//             {
//                 TextData.Get(193),
//                 TextData.Get(133),
//                 TextData.Get(194)
//             };
//
//             var widths = new[] {50, 50, 15};
//             var specs = reportInfo.specializationIds;
//             var specialization = TextData.Get(64) + ": ";
//             var title = BookDatabase.Instance.MedicalBook.scenarios.FirstOrDefault(x => 
//                 x.id == reportInfo.scenarioId)?.name;
//             
//             for (var i = 0; i < specs.Length; ++i)
//             {
//                 BookDatabase.Instance.MedicalBook.specializationById.TryGetValue(specs[i], out var sp);
//                 specialization += i == 0 ? sp?.name : ", " + sp?.name;
//             }
//             
//             var date = reportInfo.sTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
//             var time = reportInfo.sTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
//             var modeTxt = reportInfo.mode switch
//             {
//                 ScenarioModel.Mode.Learning => TextData.Get(207),
//                 ScenarioModel.Mode.Exam => TextData.Get(209),
//                 ScenarioModel.Mode.Selfcheck => "",
//                 _ => ""
//             };
//             var modeTitle = TextData.Get(221) + ": " + modeTxt;
//             
//             pdfCreator.CreateTable(headers, widths);
//             var correctCounter = 0;
//             var totalCounter = 0;
//             
//             for (var i = 0; i < reportInfo.complexActions.Count; i++)
//             {
//                 if (reportInfo.complexActions[i].timeEnd > 0.0f) continue;
//                 
//                 var isCorrect = reportInfo.answers[i] == 1;
//                 var answer = reportInfo.answers[i] switch
//                 {
//                     1 => TextData.Get(195),
//                     -1 => TextData.Get(196),
//                     _ => TextData.Get(131)
//                 };
//
//                 if (reportInfo.answers[i] == -1 && !string.IsNullOrEmpty(reportInfo.complexActions[i].criticalError))
//                     answer = TextData.Get(277);
//                 pdfCreator.AddCellToMainTable(reportInfo.complexActions[i].heading, 
//                     GetCorrectAnswer(reportInfo.complexActions[i]), answer, isCorrect);
//                 
//                 if(isCorrect)
//                     correctCounter++;
//                 
//                 totalCounter++;
//             }
//             
//             var score = isCritical ? 0 : Mathf.RoundToInt((100.0f * correctCounter) / totalCounter);
//
//             var statData = new StatisticsManager.Statistics.Item
//             {
//                 date = date,
//                 time = time,
//                 score = score,
//                 mode = (int) reportInfo.mode
//             };
//             
//             var path = "";
//             yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(
//                 GameManager.Instance.statisticsManager.AddReport(reportInfo.scenarioId, reportInfo.specializationIds, 
//                     statData, 1, val => path = val));
//             
//             pdfCreator.CreatePdf(path);
//             pdfCreator.AddHeader(TextData.Get(197) + ": " + title);
//             //pdfCreator.AddDate(statData);
//             pdfCreator.AddCustomParagraph(modeTitle);
//             pdfCreator.AddCustomParagraph(specialization);
//             pdfCreator.AddScore(score);
//             pdfCreator.AddNewLine();
//             pdfCreator.FinishPdf();
//             
//             onScoreAndPathSet?.Invoke(score, path);
//         }
//         
//         private string GetCorrectAnswer(CoursesDimedus.ManipulationScenario complexAction)
//         {
//             if (complexAction.buttons == null)
//             {
//                 var allTrues = new List<string>();
//                 foreach (var s in complexAction.multiselectTrue)
//                 {
//                     var match = Regex.Match(s, @"(.*)<(.*),(.*)>");
//                     if(!match.Success) continue;
//                     allTrues.Add(match.Groups[1].Value);
//                 }
//                 return string.Join("\n", allTrues);
//             }
//
//             foreach (var button in complexAction.buttons)
//             {
//                 var match = Regex.Match(button, @"(.*)<(.*),(.*),(.*)>");
//                 if(!match.Success) continue;
//
//                 if (match.Groups[2].Value == "1")
//                     return match.Groups[1].Value;
//             }
//             return "";
//         }
//     }
// }
