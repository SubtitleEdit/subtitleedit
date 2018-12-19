using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic.Ocr.Binary;
using System.Drawing;
using System.IO;

namespace Test.Logic.Ocr
{
    [TestClass]
    public class BinaryOcrTest
    {
        [TestMethod]
        public void TestMethodBinOcrSaveLoad()
        {
            string tempFileName = FileUtil.GetTempFileName(".db");
            var db = new BinaryOcrDb(tempFileName);
            var nbmp = new NikseBitmap(2, 2);
            nbmp.SetPixel(0, 0, Color.Transparent);
            nbmp.SetPixel(1, 0, Color.Transparent);
            nbmp.SetPixel(1, 0, Color.Transparent);
            nbmp.SetPixel(1, 1, Color.White);

            var bob = new BinaryOcrBitmap(nbmp);
            bob.Text = "Debug";
            db.Add(bob);

            nbmp.SetPixel(0, 0, Color.White);
            var bob2 = new BinaryOcrBitmap(nbmp);
            bob2.X = 2;
            bob2.Y = 4;
            bob2.Text = null;
            bob2.Italic = true;
            bob2.ExpandCount = 2;
            bob2.ExpandedList = new System.Collections.Generic.List<BinaryOcrBitmap>();
            bob2.ExpandedList.Add(bob2);
            db.Add(bob2);
            db.Save();

            db = new BinaryOcrDb(tempFileName, true);
            Assert.IsTrue(db.CompareImages.Count == 1);
            Assert.IsTrue(db.CompareImagesExpanded.Count == 1);

            Assert.IsTrue(bob.Width == db.CompareImages[0].Width);
            Assert.IsTrue(bob.Height == db.CompareImages[0].Height);
            Assert.IsTrue(bob.NumberOfColoredPixels == db.CompareImages[0].NumberOfColoredPixels);
            Assert.IsTrue(bob.Hash == db.CompareImages[0].Hash);
            Assert.IsTrue(bob.Italic == db.CompareImages[0].Italic);
            Assert.IsTrue(bob.ExpandCount == db.CompareImages[0].ExpandCount);
            Assert.IsTrue(bob.Text == db.CompareImages[0].Text);

            Assert.IsTrue(bob2.Width == db.CompareImagesExpanded[0].Width);
            Assert.IsTrue(bob2.Height == db.CompareImagesExpanded[0].Height);
            Assert.IsTrue(bob2.NumberOfColoredPixels == db.CompareImagesExpanded[0].NumberOfColoredPixels);
            Assert.IsTrue(bob2.Hash == db.CompareImagesExpanded[0].Hash);
            Assert.IsTrue(bob2.Italic == db.CompareImagesExpanded[0].Italic);
            Assert.IsTrue(bob2.ExpandCount == db.CompareImagesExpanded[0].ExpandCount);
            Assert.IsTrue(bob2.Text == db.CompareImagesExpanded[0].Text);
            Assert.IsTrue(bob2.X == db.CompareImagesExpanded[0].X);
            Assert.IsTrue(bob2.Y == db.CompareImagesExpanded[0].Y);

            try
            {
                File.Delete(tempFileName);
            }
            catch
            {
            }
        }

        [TestMethod]
        public void TestMethodBinOcrSaveLoadTestExceptions()
        {
            string tempFileName = FileUtil.GetTempFileName(".db");
            var db = new BinaryOcrDb(tempFileName);
            var nbmp = new NikseBitmap(2, 2);
            nbmp.SetPixel(0, 0, Color.Transparent);
            nbmp.SetPixel(1, 0, Color.Transparent);
            nbmp.SetPixel(1, 0, Color.Transparent);
            nbmp.SetPixel(1, 1, Color.White);

            var bob = new BinaryOcrBitmap(nbmp);
            bob.Text = "S";
            db.Add(bob);

            nbmp.SetPixel(0, 0, Color.White);
            var bob2 = new BinaryOcrBitmap(nbmp);
            bob2.X = 2;
            bob2.Y = 4;
            bob2.Text = null;
            bob2.Italic = true;
            bob2.ExpandCount = 3;
            bob2.ExpandedList = new System.Collections.Generic.List<BinaryOcrBitmap>();
            bob2.ExpandedList.Add(bob2);
            try
            {
                db.Add(bob2);
            }
            catch
            {
                return;
            }
            Assert.Fail();

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
