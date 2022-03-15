// using System;
// using System.Collections;
// using System.Globalization;
// using System.Linq;
// using Modules.Books;
// using Modules.WDCore;
// using Modules.Statistics;
// using UnityEngine;
//
// namespace Modules.DimedusCourse
// {
//     public class DCourseReport
//     {
//         public IEnumerator CreateReport(DCourseModel.ReportInfo reportInfo)
//         {
//             if(reportInfo.answeredQs.Count == 0) yield break;
//         
//             var pdfCreator = new PdfCreator();
//             var headers = new[]
//             {
//                 TextData.Get(111),
//                 TextData.Get(133),
//                 TextData.Get(134)
//             };
//             
//             var questions = reportInfo.complexActions.Where(x => 
//                 !string.IsNullOrEmpty(x.question)).ToList();
//             var widths = new[] {50, 50, 12};
//             var mistakesSum = reportInfo.answeredQs.Values.Sum() + (questions.Count - reportInfo.answeredQs.Count) * 4;
//             var score = Mathf.RoundToInt(100.0f*(4 * questions.Count - mistakesSum) 
//                                    / (4 * questions.Count));
//             var specs = reportInfo.specializationIds;
//             var specialization = TextData.Get(64) + ": ";
//             var title = BookDatabase.Instance.CoursesDimedus.courseById[reportInfo.courseId].name;
//             
//             for (var i = 0; i < specs.Length; ++i)
//             {
//                 BookDatabase.Instance.MedicalBook.specializationById.TryGetValue(specs[i], out var sp);
//                 specialization += i == 0 ? sp?.name : ", " + sp?.name;
//             }
//
//             var date = reportInfo.sTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
//             var time = reportInfo.sTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
//             var modeTxt = 0 switch
//             {
//                 0 => TextData.Get(207),
//                 1 => TextData.Get(208),
//                 2 => TextData.Get(209),
//                 _ => throw new ArgumentOutOfRangeException()
//             };
//             var modeTitle = TextData.Get(221) + ": " + modeTxt;
//
//             var statData = new StatisticsManager.Statistics.Item
//             {
//                 date = date,
//                 time = time,
//                 score = score,
//                 mode = 0
//             };
//
//             var path = "";
//             yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(
//                 GameManager.Instance.statisticsManager.AddReport(reportInfo.courseId, reportInfo.specializationIds, 
//                     statData, 0, val => path = val));
//             
//             pdfCreator.CreateTable(headers, widths);
//             pdfCreator.CreatePdf(path);
//             pdfCreator.AddHeader(TextData.Get(135) + ": " + title);
//             //pdfCreator.AddDate(statData);
//             pdfCreator.AddCustomParagraph(modeTitle);
//             pdfCreator.AddCustomParagraph(specialization);
//             pdfCreator.AddScore(score);
//             pdfCreator.AddNewLine();
//             
//             foreach (var question in questions)
//             {
//                 var q = question.question;
//                 var mistakeCount = 4;
//                 if (reportInfo.answeredQs.ContainsKey(q))
//                     mistakeCount = reportInfo.answeredQs[q];
//                 pdfCreator.AddCellToMainTable(q, question.answers[0], mistakeCount);
//             }
//
//             pdfCreator.FinishPdf();
//             DirectoryPath.OpenPDF(path);
//         }
//
//
//     }
// }
