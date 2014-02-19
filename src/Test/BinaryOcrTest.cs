using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Logic.OCR.Binary;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Test
{
    [TestClass]
    public class BinaryOcrTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            string tempFileName = System.IO.Path.GetTempFileName();
            var db = new BinaryOcrDb(tempFileName);
            var nbmp = new NikseBitmap(2, 2);
            nbmp.SetPixel(0, 0, Color.Transparent);
            nbmp.SetPixel(1, 0, Color.Transparent);
            nbmp.SetPixel(1, 0, Color.Transparent);
            nbmp.SetPixel(1, 1, Color.White);
            
            var bob = new BinaryOcrBitmap(nbmp);
            bob.Text = "Debug";
            db.CompareImages.Add(bob);

            nbmp.SetPixel(0, 0, Color.White);
            var bob2 = new BinaryOcrBitmap(nbmp);
            bob2.Text = "tt";
            bob2.Italic = true;
            bob2.ExpandCount = 2;
            db.CompareImages.Add(bob2);
            db.Save();

            db = new BinaryOcrDb(tempFileName, true);
            Assert.IsTrue(db.CompareImages.Count == 2);

            Assert.IsTrue(bob.Width == db.CompareImages[0].Width);
            Assert.IsTrue(bob.Height == db.CompareImages[0].Height);
            Assert.IsTrue(bob.NumberOfColoredPixels == db.CompareImages[0].NumberOfColoredPixels);
            Assert.IsTrue(bob.Hash == db.CompareImages[0].Hash);
            Assert.IsTrue(bob.Italic == db.CompareImages[0].Italic);
            Assert.IsTrue(bob.ExpandCount == db.CompareImages[0].ExpandCount);
            Assert.IsTrue(bob.Text == db.CompareImages[0].Text);

            Assert.IsTrue(bob2.Width == db.CompareImages[1].Width);
            Assert.IsTrue(bob2.Height == db.CompareImages[1].Height);
            Assert.IsTrue(bob2.NumberOfColoredPixels == db.CompareImages[1].NumberOfColoredPixels);
            Assert.IsTrue(bob2.Hash == db.CompareImages[1].Hash);
            Assert.IsTrue(bob2.Italic == db.CompareImages[1].Italic);
            Assert.IsTrue(bob2.ExpandCount == db.CompareImages[1].ExpandCount);
            Assert.IsTrue(bob2.Text == db.CompareImages[1].Text);

            try
            {
                System.IO.File.Delete(tempFileName);
            }
            catch
            {
                System.IO.File.Delete(tempFileName);
            }
        }
    }
}
