using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Test.Logic
{
    [TestClass]
    public class UnknownFormatImporterCsvTest
    {
        [TestMethod]
        public void TestUnknownCsv1()
        {
            var raw = @"ROLLE;IN;OUT;DIALOG
PAUL;00:00:06,901;00:00:09,901; So, du hast also den Schwangerschaftstest gebraucht?
CATE;00:00:10,000;00:00:12,800; Ja. Ich habe den Test gemacht.";

            var importer = new UnknownFormatImporterCsv();
            var subtitle = importer.AutoGuessImport(raw.Trim().SplitToLines());
            Assert.AreEqual(2, subtitle.Paragraphs.Count);
            Assert.AreEqual("Ja. Ich habe den Test gemacht.", subtitle.Paragraphs[1].Text);
        }
    }
}