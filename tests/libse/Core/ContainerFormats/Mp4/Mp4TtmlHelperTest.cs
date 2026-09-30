using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;

namespace LibSETests.Core.ContainerFormats.Mp4;

public class Mp4TtmlHelperTest
{
    /// <summary>
    /// An stpp sample declaring the IMSC1 profile: TimedText10 leaves those to the IMSC format,
    /// so every sample of a DASH IMSC1 track parsed to nothing and the track was dropped.
    /// </summary>
    [Fact]
    public void Imsc1ProfileDocumentIsParsed()
    {
        const string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<tt xmlns=""http://www.w3.org/ns/ttml"" xmlns:ttp=""http://www.w3.org/ns/ttml#parameter"" xmlns:tts=""http://www.w3.org/ns/ttml#styling""
    ttp:profile=""http://www.w3.org/ns/ttml/profile/imsc1/text"" xml:lang=""cs"">
  <body>
    <div>
      <p begin=""00:00:30.240"" end=""00:00:33.120"">Duben 1915.<br/>Válka trvá už osm měsíců.</p>
    </div>
  </body>
</tt>";

        var paragraphs = Mp4TtmlHelper.ParseTtmlDocument(xml);

        var paragraph = Assert.Single(paragraphs);
        Assert.Equal(30240, paragraph.StartTime.TotalMilliseconds);
        Assert.Equal(33120, paragraph.EndTime.TotalMilliseconds);
        Assert.Equal("Duben 1915." + Environment.NewLine + "Válka trvá už osm měsíců.", paragraph.Text);
    }
}
