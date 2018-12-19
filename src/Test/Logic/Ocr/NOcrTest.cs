using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic.Ocr;
using System.Drawing;
using System.IO;

namespace Test.Logic.Ocr
{
    [TestClass]
    public class NOcrTest
    {

        [TestMethod]
        public void TestNOcrSaveLoad()
        {
            string tempFileName = FileUtil.GetTempFileName(".nocr");
            var db = new NOcrDb(tempFileName);

            var nOcrChar = new NOcrChar("t");
            nOcrChar.ExpandCount = 0;
            nOcrChar.Italic = false;
            nOcrChar.MarginTop = 2;
            nOcrChar.Width = 10;
            nOcrChar.Height = 10;
            nOcrChar.LinesForeground.Add(new NOcrPoint(new Point(1, 1), new Point(2, 2)));
            nOcrChar.LinesBackground.Add(new NOcrPoint(new Point(3, 4), new Point(5, 6)));
            db.Add(nOcrChar);

            var nOcrChar2 = new NOcrChar("u");
            nOcrChar2.ExpandCount = 0;
            nOcrChar2.Italic = false;
            nOcrChar2.MarginTop = 3;
            nOcrChar2.Width = 12;
            nOcrChar2.Height = 12;
            nOcrChar2.LinesForeground.Add(new NOcrPoint(new Point(1, 1), new Point(2, 2)));
            nOcrChar2.LinesBackground.Add(new NOcrPoint(new Point(3, 4), new Point(5, 6)));
            db.Add(nOcrChar2);
            db.Save();

            db = new NOcrDb(tempFileName);
            Assert.IsTrue(db.OcrCharacters.Count == 2);

            Assert.IsTrue(db.OcrCharacters[0].Text == nOcrChar2.Text);
            Assert.IsTrue(db.OcrCharacters[0].Italic == nOcrChar2.Italic);
            Assert.IsTrue(db.OcrCharacters[0].MarginTop == nOcrChar2.MarginTop);
            Assert.IsTrue(db.OcrCharacters[0].LinesForeground.Count == nOcrChar2.LinesForeground.Count);
            Assert.IsTrue(db.OcrCharacters[0].LinesForeground[0].Start.X == nOcrChar2.LinesForeground[0].Start.X);
            Assert.IsTrue(db.OcrCharacters[0].LinesForeground[0].Start.Y == nOcrChar2.LinesForeground[0].Start.Y);
            Assert.IsTrue(db.OcrCharacters[0].LinesBackground.Count == nOcrChar2.LinesBackground.Count);
            Assert.IsTrue(db.OcrCharacters[0].LinesBackground[0].Start.X == nOcrChar2.LinesBackground[0].Start.X);
            Assert.IsTrue(db.OcrCharacters[0].LinesBackground[0].Start.Y == nOcrChar2.LinesBackground[0].Start.Y);

            Assert.IsTrue(db.OcrCharacters[1].Text == nOcrChar.Text);

            try
            {
                File.Delete(tempFileName);
            }
            catch
            {
            }
        }
    }
}
