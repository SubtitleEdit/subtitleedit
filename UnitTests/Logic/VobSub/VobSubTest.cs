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

    }
}
