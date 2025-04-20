using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;

namespace Tests.Logic.VideoFormats
{
    public class MatroskaTest
    {
        [Fact]
        public void MatroskaTestValid()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_MKV_SRT.mkv");
            using (var parser = new MatroskaFile(fileName))
            {
                Assert.True(parser.IsValid);
            }
        }

        [Fact]
        public void MatroskaTestInvalid()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_TS_with_graphics.ts");
            using (var parser = new MatroskaFile(fileName))
            {
                Assert.False(parser.IsValid);
            }
        }

        [Fact]
        public void MatroskaTestIsSrt()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_MKV_SRT.mkv");
            using (var parser = new MatroskaFile(fileName))
            {
                var tracks = parser.GetTracks(true);
                Assert.True(tracks[0].CodecId == "S_TEXT/UTF8");
            }
        }

        [Fact]
        public void MatroskaTestSrtContent()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_MKV_SRT.mkv");
            using (var parser = new MatroskaFile(fileName))
            {
                var tracks = parser.GetTracks(true);
                var subtitles = parser.GetSubtitle(Convert.ToInt32(tracks[0].TrackNumber), null);
                Assert.True(subtitles.Count == 2);
                Assert.True(subtitles[0].GetText(tracks[0]) == "Line 1");
                Assert.True(subtitles[1].GetText(tracks[0]) == "Line 2");
            }
        }

        [Fact]
        public void MatroskaTestVobSubPgs()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_MKV_VobSub_PGS.mkv");
            using (var parser = new MatroskaFile(fileName))
            {
                var tracks = parser.GetTracks(true);
                Assert.True(tracks[0].CodecId == "S_VOBSUB");
                Assert.True(tracks[1].CodecId == "S_HDMV/PGS");
            }
        }

        [Fact]
        public void MatroskaTestVobSubPgsContent()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_MKV_VobSub_PGS.mkv");
            using (var parser = new MatroskaFile(fileName))
            {
                var tracks = parser.GetTracks(true);
                var subtitles = parser.GetSubtitle(Convert.ToInt32(tracks[0].TrackNumber), null);
                Assert.True(subtitles.Count == 2);
                // TODO: Check bitmaps

                //subtitles = parser.GetSubtitle(Convert.ToInt32(tracks[1].TrackNumber), null);
                //Assert.True(subtitles.Count == 2);
                //check bitmaps
            }
        }

        [Fact]
        public void MatroskaTestDelayed500Ms()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_MKV_delayed.mkv");
            using (var parser = new MatroskaFile(fileName))
            {
                var delay = parser.GetTrackStartTime(parser.GetTracks()[0].TrackNumber);
                Assert.True(delay == 500);
            }
        }

    }
}
