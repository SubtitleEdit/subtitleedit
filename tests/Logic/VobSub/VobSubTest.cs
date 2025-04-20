using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;

namespace Tests.Logic.VobSub
{
    
    public class VobSubTest
    {
        [Fact]
        public void VobSubWriteAndReadTwoBitmaps()
        {
            string fileName = Guid.NewGuid() + ".sub";
            using (var writer = new VobSubWriter(fileName, 800, 600, 10, 10, 32, SKColors.White, SKColors.Black, true, DvdSubtitleLanguage.English))
            {
                var p1 = new Paragraph("Line1", 0, 1000);
                var p2 = new Paragraph("Line2", 2000, 3000);
                writer.WriteParagraph(p1, new SKBitmap(200, 20), BluRayContentAlignment.BottomCenter);
                writer.WriteParagraph(p2, new SKBitmap(200, 20), BluRayContentAlignment.BottomCenter);
            }

            var reader = new VobSubParser(true);
            reader.Open(fileName);
            var list = reader.MergeVobSubPacks();

            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void VobSubNonStandardLengthPackets()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_VodSub_nonStandardLength.sub");

            var reader = new VobSubParser(true);
            reader.Open(fileName);
            // check if we got the right number of packs
            Assert.Equal(6, reader.VobSubPacks.Count);
            var list = reader.MergeVobSubPacks();

            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void VobSubStuffingBytes()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_VodSub_stuffingBytes.sub");

            var reader = new VobSubParser(true);
            reader.Open(fileName);
            // check if we got the right number of packs
            Assert.Equal(3, reader.VobSubPacks.Count);
            // check if the stuffing data was correctly striped.
            Assert.Equal(0, reader.VobSubPacks[2].Mpeg2Header.PackStuffingLength);

            var list = reader.MergeVobSubPacks();
            Assert.Single(list);
        }

        [Fact]
        public void VodSubWindowPaddingWithoutPaddingStream()
        {
            // Older versions of SE and other encoders do not use padding streams to fit packs
            // to a byte 2048 window. they just fill the rest of the window with 0xFF instead.
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_VodSub_windowStuffingWithoutPaddingStream.sub");

            var reader = new VobSubParser(true);
            reader.Open(fileName);
            // check if we got the right number of packs
            Assert.Equal(8, reader.VobSubPacks.Count);

            var list = reader.MergeVobSubPacks();
            Assert.Equal(2, list.Count);
        }
    }
}
