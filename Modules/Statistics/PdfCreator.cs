using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Modules.Scenario;
using Modules.WDCore;
using UnityEngine;
using Font = iTextSharp.text.Font;

namespace Modules.Statistics
{
    public class PdfCreator
    {
        private Font _font20;
        private Font _font14;
        private Font _font10Black;
        private Font _font10Blue;
        private Font _font10Red;
        private Font _font10Green;
        private Font _font10White;

        private BaseColor _black;
        private BaseColor _green;
        private BaseColor _red;
        private BaseColor _darkGrey;
        private BaseColor _blue;

        private Document _doc;
        private PdfWriter _writer;
        private PdfPTable _mainTable;
        private Dictionary<string, Paragraph> _storedParagraphs;

        public void CreatePdf(string path)
        {
            _doc = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
            _writer = PdfWriter.GetInstance(_doc, new FileStream(path, FileMode.Create));
            _doc.Open();
        }

        public void CreateTable(string[] headers, int[] widths)
        {
            InitStyle();
            _mainTable = new PdfPTable(headers.Length);
            _mainTable.SetWidths(widths);
            
            foreach (var header in headers)
                _mainTable.AddCell(CreateCell(header, _font10White, _darkGrey, Element.ALIGN_CENTER));
        }

        public void AddCellToMainTable(string hint, string correctAction, List<(string, int)> actualActions, int grade)
        {
            _mainTable.AddCell(CreateCell(hint));
            _mainTable.AddCell(CreateCell(correctAction));

            var phrase = new Phrase();
            for (var i = 0; i < actualActions.Count; i++)
            {
                phrase.Add(new Chunk(i == 0 ? actualActions[i].Item1 : "\n" + actualActions[i].Item1, 
                    actualActions[i].Item2 == 0 ? _font10Black : 
                        actualActions[i].Item2 ==  1 ? _font10Green: _font10Red));
            }

            _mainTable.AddCell(new PdfPCell(phrase){Padding = 5, HorizontalAlignment = Element.ALIGN_LEFT});
            _mainTable.AddCell(CreateCell(grade + "%"));
        }
        
        public void AddCellToMainTable(string question, string correctAnswer, int mistakeCount)
        {
            _mainTable.AddCell(CreateCell(question));
            _mainTable.AddCell(CreateCell(correctAnswer));
            _mainTable.AddCell(CreateCell(mistakeCount.ToString()));
        }
        
        public void AddCellToMainTable(string question, string correctAnswer, string status, bool isCorrect)
        {
            _mainTable.AddCell(CreateCell(question));
            _mainTable.AddCell(CreateCell(correctAnswer));
            _mainTable.AddCell(CreateCell(status, isCorrect ? _font10Green : _font10Red));
        }

        public void AddHeaderToMainTable(string title)
        {
            SetTitle(title, _mainTable);
        }

        public void FinishPdf()
        {
            _doc.Add(_mainTable);
            _doc.Close();
            _writer.Close();
        }

        private void InitStyle()
        {
#if UNITY_ANDROID
            string pathFonts = Application.persistentDataPath + "/segoeui.ttf";
#else
            string pathFonts = string.Format(Application.streamingAssetsPath + "/Fonts/{0}", "segoeui.ttf");
            if (!File.Exists(pathFonts)) return;
#endif

            FontFactory.Register(pathFonts, "segoeui");
            _font20 = FontFactory.GetFont("segoeui", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 20);
            _font14 = FontFactory.GetFont("segoeui", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 14);
            
            _font10Black = FontFactory.GetFont("segoeui", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 10);
            _font10Blue = FontFactory.GetFont("segoeui", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 10);
            _font10White = FontFactory.GetFont("segoeui", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 10);
            _font10Green = FontFactory.GetFont("segoeui", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 10);
            _font10Red = FontFactory.GetFont("segoeui", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 10);

            _black = new BaseColor(0, 0, 0);
            _green = new BaseColor(108, 150, 66);
            _red = new BaseColor(177, 1, 1);
            _blue = new BaseColor(76, 81, 109);
            _darkGrey = new BaseColor(87, 89, 93);

            _font10Black.Color = _black;
            _font10Blue.Color = BaseColor.BLUE;
            _font10White.Color = BaseColor.WHITE;
            _font10Green.Color = _green;
            _font10Red.Color = _red;
            
            _font10Blue.SetStyle(Font.UNDERLINE);
        }

        // private Paragraph User(UserInfo.Data data)
        // {
        //     var header = $"{TextFieldData.GetText(2711)}: {data.StudentName}\n{TextFieldData.GetText(2712)}: {data.GroupNumber}";
        //     return new Paragraph(header, _font16) {Alignment = Element.ALIGN_LEFT};
        // }

        private static Paragraph Line =>
            new Paragraph(new Chunk(
                new LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));

        public void AddHeader(string header)
        {
            _doc.Add(new Paragraph(header, _font20) {Alignment = Element.ALIGN_CENTER});
            _doc.Add(new Paragraph("\n", _font10Black) {Alignment = Element.ALIGN_CENTER});
        }

        public void AddDate(StatisticsManager.Statistics.Item stat, DateTime scenarioTime, ScenarioModel.Mode mode)
        {
            var startDate = TextData.Get(136) + ": " + stat.date;
            var startTime = TextData.Get(137) + ": " + stat.time;
            _doc.Add(new Paragraph(startDate, _font10Black) {Alignment = Element.ALIGN_LEFT});
            _doc.Add(new Paragraph(startTime, _font10Black) {Alignment = Element.ALIGN_LEFT});

            if (mode == ScenarioModel.Mode.Exam)
            {
                var limit = new[] {0, 25, 30, 35, 40, 45, 50, 55, 60};
                var index = PlayerPrefs.GetInt("TIME_LIMIT");
                var timeLimit = limit[index] * 60;

                if (timeLimit != 0)
                {
                    var durTime = (DateTime.Now - scenarioTime).ToString(@"mm\:ss");
                    var duration = $"{TextData.Get(150)}: {durTime} {TextData.Get(152)} " +
                                   $"({TextData.Get(151)} {TimeSpan.FromSeconds(timeLimit):mm} {TextData.Get(86)})";
                    _doc.Add(new Paragraph(duration, _font10Black) {Alignment = Element.ALIGN_LEFT});
                }
            }
            
            _doc.Add(new Paragraph("\n", _font10Black) {Alignment = Element.ALIGN_CENTER});
        }

        public void AddScore(double val)
        {
            var score = TextData.Get(138) + ": " + val + "%";
            _doc.Add(new Paragraph(score, _font10Black) {Alignment = Element.ALIGN_LEFT});
        }
        
        public void AddCustomParagraph(string val, int alignment = 0)
        {
            _doc.Add(new Paragraph(val, _font10Black) {Alignment = alignment});
        }
        
        public void AddCustomLink(string title, string txt, string link)
        {
            var anchor = new Chunk(txt){Font = _font10Blue};
            anchor.SetAnchor(link);
            var p = new Paragraph(title + " ", _font10Black) {anchor};
            _doc.Add(p);
        }

        public void AddNewLine()
        {
            _doc.Add(new Paragraph("\n", _font10Black) {Alignment = Element.ALIGN_CENTER});
        }

        public void AddLastLine(string txt, int score)
        {
            _mainTable.AddCell(CreateCell(txt, _font10White, _darkGrey, Element.ALIGN_LEFT));
            _mainTable.AddCell(CreateCell("", _font10White, _darkGrey, Element.ALIGN_LEFT));
            _mainTable.AddCell(CreateCell("", _font10White, _darkGrey, Element.ALIGN_LEFT));
            _mainTable.AddCell(CreateCell(score + "%", _font10White, _darkGrey, Element.ALIGN_LEFT));
        }

        // private Paragraph Score(float score, float totalPoints)
        // {
        //     var percentage = score / totalPoints * 100.0f;
        //     return new Paragraph($"{TextFieldData.GetText(2740)}: {String.Format("{0:0.00}", percentage)}% ({score} / {totalPoints} {TextFieldData.GetText(2763)})", _font16) {Alignment = Element.ALIGN_LEFT};
        // }


        private void SetTitle(string txt, PdfPTable table)
        {
            table.AddCell(new PdfPCell(new Paragraph(txt, _font10White))
                {Padding = 5, Colspan = 4, BackgroundColor = _blue, HorizontalAlignment = Element.ALIGN_CENTER});
        }
        
        // private void SetEnd(string id, PdfPTable table)
        // {
        //     if (!id.Contains("_end"))
        //         return;
        //     
        //     table.AddCell(new PdfPCell(new Paragraph("~", _font10Black) {IndentationLeft = 5})
        //         {PaddingTop = 0, PaddingBottom = 0, Colspan = 4, BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_CENTER});
        // }

        private PdfPCell CreateCell(string val, Font font = null)
        {
            return new PdfPCell(new Paragraph(val, font ?? _font10Black)) {Padding = 5};
        }
        
        private PdfPCell CreateCell(string val, Font font, BaseColor bcgClr, int alignment)
        {
            return new PdfPCell(new Paragraph(val, font)) {Padding = 5, HorizontalAlignment = alignment, BackgroundColor = bcgClr};
        }
    }
}