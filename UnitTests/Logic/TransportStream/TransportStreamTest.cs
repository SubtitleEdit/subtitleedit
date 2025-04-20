using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;

namespace Tests.Logic.TransportStream
{
    public class TransportStreamTest
    {
        [Fact]
        public void TransportStreamTestImage()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_TS_with_graphics.ts");
            var parser = new TransportStreamParser();
            parser.Parse(fileName, null);
            var subtitles = parser.GetDvbSubtitles(41);

            Assert.True(subtitles.Count == 10);
            using (var bmp = subtitles[0].Pes.GetImageFull())
            {
                Assert.True(bmp.Width == 720);
                Assert.True(bmp.Height == 576);
            }
        }

        [Fact]
        public void TransportStreamTestTeletext()
        {
            string fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample_TS_with_teletext.ts");
            var parser = new TransportStreamParser();
            parser.Parse(fileName, null);
            Assert.Single(parser.TeletextSubtitlesLookup);
            Assert.Equal(5104, parser.TeletextSubtitlesLookup.First().Key); // package Id
            var packagePages = parser.TeletextSubtitlesLookup[parser.TeletextSubtitlesLookup.First().Key];
            Assert.Equal(2, packagePages.Count);
            Assert.Single(packagePages[150]); // first page number
            Assert.Equal(2, packagePages[799].Count); // second page number

            Assert.Equal("Für diese Klassenstufe ist er nicht" + Environment.NewLine +
                            "geeignet.  <font color=\"#00ffff\">  Stufen Sie ihn zurück!</font>", packagePages[150][0].Text);

            Assert.Equal("Han er ikke egnet" + Environment.NewLine +
                            "til dette klassetrin.", packagePages[799][0].Text);

            Assert.Equal("Så sæt ham et år ned, så han kan" + Environment.NewLine +
                            "indhente det forsømte.", packagePages[799][1].Text);
        }
    }
}