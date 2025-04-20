using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;
using System.Text;

namespace Tests.Logic.BluRaySup
{
    public class BluRaySupParserTest
    {
        [Fact]
        public void BluRaySupWriteAndReadTwoBitmaps()
        {
            var fileName = Guid.NewGuid() + ".sup";
            using (var binarySubtitleFile = new FileStream(fileName, FileMode.Create))
            {
                var brSub = new BluRaySupPicture
                {
                    StartTime = 0,
                    EndTime = 1000,
                    Width = 1080,
                    Height = 720
                };
                var buffer = BluRaySupPicture.CreateSupFrame(brSub, new SKBitmap(100, 20), 10, 25, 10, BluRayContentAlignment.BottomCenter);
                binarySubtitleFile.Write(buffer, 0, buffer.Length);
                brSub.StartTime = 2000;
                brSub.EndTime = 3000;
                buffer = BluRaySupPicture.CreateSupFrame(brSub, new SKBitmap(100, 20), 10, 25, 10, BluRayContentAlignment.BottomCenter);
                binarySubtitleFile.Write(buffer, 0, buffer.Length);
            }

            var log = new StringBuilder();
            var subtitles = BluRaySupParser.ParseBluRaySup(fileName, log);

            Assert.Equal(2, subtitles.Count);
        }

        [Fact]
        public void BluRaySupReadMultiImagePic()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_BDSUP_multi_image.sup");
            var log = new StringBuilder();
            var subtitles = BluRaySupParser.ParseBluRaySup(fileName, log);
            Assert.Equal(2, subtitles.Count);
            Assert.Equal(2, subtitles[0].PcsObjects.Count);
            using (var bmp = subtitles[0].GetBitmap())
            {
                Assert.Equal(784, bmp.Width);
                Assert.Equal(911, bmp.Height);
            }
        }
    }
}
