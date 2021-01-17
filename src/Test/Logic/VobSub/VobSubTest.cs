using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Drawing;

namespace Test.Logic.VobSub
{
    [TestClass]
    public class VobSubTest
    {
        [TestMethod]
        public void VobSubWriteAndReadTwoBitmaps()
        {
            string fileName = Guid.NewGuid() + ".sub";
            using (var writer = new VobSubWriter(fileName, 800, 600, 10, 10, 32, Color.White, Color.Black, true, DvdSubtitleLanguage.English))
            {
                var p1 = new Paragraph("Line1", 0, 1000);
                var p2 = new Paragraph("Line2", 2000, 3000);
                writer.WriteParagraph(p1, new Bitmap(200, 20), ContentAlignment.BottomCenter);
                writer.WriteParagraph(p2, new Bitmap(200, 20), ContentAlignment.BottomCenter);
            }

            var reader = new VobSubParser(true);
            reader.Open(fileName);
            var list = reader.MergeVobSubPacks();

            Assert.IsTrue(list.Count == 2);
        }

    }
}
