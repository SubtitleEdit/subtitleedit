using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Drawing;
using System.IO;

namespace Tests.Logic.VobSub
{
    [TestClass]
    [DeploymentItem("Files")]
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

            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void VobSubNonStandardLengthPackets()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_VodSub_nonStandardLength.sub");

            var reader = new VobSubParser(true);
            reader.Open(fileName);
            // check if we got the right number of packs
            Assert.AreEqual(6, reader.VobSubPacks.Count);
            var list = reader.MergeVobSubPacks();

            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void VobSubStuffingBytes()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_VodSub_stuffingBytes.sub");

            var reader = new VobSubParser(true);
            reader.Open(fileName);
            // check if we got the right number of packs
            Assert.AreEqual(3, reader.VobSubPacks.Count);
            // check if the stuffing data was correctly striped.
            Assert.AreEqual(0, reader.VobSubPacks[2].Mpeg2Header.PackStuffingLength);

            var list = reader.MergeVobSubPacks();
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void VodSubWindowPaddingWithoutPaddingStream()
        {
            // Older versions of SE and other encoders do not use padding streams to fit packs
            // to a byte 2048 window. they just fill the rest of the window with 0xFF instead.
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_VodSub_windowStuffingWithoutPaddingStream.sub");

            var reader = new VobSubParser(true);
            reader.Open(fileName);
            // check if we got the right number of packs
            Assert.AreEqual(8, reader.VobSubPacks.Count);

            var list = reader.MergeVobSubPacks();
            Assert.AreEqual(2, list.Count);
        }

    }
}
