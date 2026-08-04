using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;

namespace LibSETests.Core.ContainerFormats.Mp4;

/// <summary>
/// Fragmented MP4 (DASH/CMAF) subtitle extraction: wvtt, stpp and tx3g samples
/// addressed via moof/traf/trun, with moov only providing track metadata.
/// </summary>
public class Mp4DashSubtitleTest
{
    // Fragment ticks (tfdt, sample durations) are media-track times, so the track's
    // mdhd timescale applies. Using the movie (mvhd) timescale skewed all cue times
    // by mvhd/mdhd - the classic GPAC layout is mvhd=600 vs track=1000, i.e. 5/3 off.
    [Fact]
    public void FragmentedWvtt_UsesTrackTimescale_NotMovieTimescale()
    {
        var init = BuildInit("text", "wvtt", trackId: 1, mdhdTimeScale: 1000, mvhdTimeScale: 600);
        var sample = WvttCueSample("Hello DASH");
        var segment = BuildSegment(trackId: 1, tfdtTicks: 1000, durations: new uint[] { 3000 }, samples: new[] { sample });

        var subtitle = ParseVttc(Concat(init, segment));

        Assert.NotNull(subtitle);
        var p = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Hello DASH", p.Text);
        Assert.Equal(1000, p.StartTime.TotalMilliseconds, 1);
        Assert.Equal(4000, p.EndTime.TotalMilliseconds, 1);
    }

    // A lone DASH media segment (no init) still yields the cue text, timed from tfdt
    // with the 1000 default timescale.
    [Fact]
    public void FragmentedWvtt_LoneSegmentWithoutInit_StillExtractsCue()
    {
        var sample = WvttCueSample("Lone segment");
        var segment = BuildSegment(trackId: 1, tfdtTicks: 4000, durations: new uint[] { 2000 }, samples: new[] { sample });

        var subtitle = ParseVttc(segment);

        Assert.NotNull(subtitle);
        var p = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Lone segment", p.Text);
        Assert.Equal(4000, p.StartTime.TotalMilliseconds, 1);
        Assert.Equal(6000, p.EndTime.TotalMilliseconds, 1);
    }

    // Muxed fMP4: one moof with a traf per track. Keeping only one traf (the old
    // behaviour) lost the subtitle track whenever audio/video fragments were present;
    // per-traf data offsets must be honoured so the right mdat bytes are sliced.
    [Fact]
    public void FragmentedMuxed_MultipleTrafs_ExtractsSubtitleTraf()
    {
        var init = BuildInit("text", "wvtt", trackId: 2, mdhdTimeScale: 1000, mvhdTimeScale: 600,
            extraTrack: BuildTrak("soun", null, trackId: 1, mdhdTimeScale: 48000));

        var audioJunk = new byte[] { 0xde, 0xad, 0xbe, 0xef, 0x01, 0x02, 0x03, 0x04 };
        var subtitleSample = WvttCueSample("Muxed cue");
        var segment = BuildMuxedSegment(
            (trackId: 1u, tfdtTicks: 0u, durations: new uint[] { 1024 }, samples: new[] { audioJunk }),
            (trackId: 2u, tfdtTicks: 2000u, durations: new uint[] { 1500 }, samples: new[] { subtitleSample }));

        var subtitle = ParseVttc(Concat(init, segment));

        Assert.NotNull(subtitle);
        var p = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Muxed cue", p.Text);
        Assert.Equal(2000, p.StartTime.TotalMilliseconds, 1);
        Assert.Equal(3500, p.EndTime.TotalMilliseconds, 1);
    }

    // stpp samples are complete TTML documents; cues spanning several samples are
    // repeated in each document and must be emitted once, at their document times.
    [Fact]
    public void FragmentedStpp_TtmlSamples_UseDocumentTimes_AndDeduplicate()
    {
        var doc1 = TtmlDoc("<p begin=\"00:00:01.000\" end=\"00:00:06.000\">Long cue</p>");
        var doc2 = TtmlDoc("<p begin=\"00:00:01.000\" end=\"00:00:06.000\">Long cue</p><p begin=\"00:00:05.000\" end=\"00:00:07.000\">Second</p>");
        var init = BuildInit("subt", "stpp", trackId: 1, mdhdTimeScale: 1000, mvhdTimeScale: 600);
        var seg1 = BuildSegment(1, 0, new uint[] { 4000 }, new[] { Encoding.UTF8.GetBytes(doc1) });
        var seg2 = BuildSegment(1, 4000, new uint[] { 4000 }, new[] { Encoding.UTF8.GetBytes(doc2) });

        var subtitle = ParseVttc(Concat(init, seg1, seg2));

        Assert.NotNull(subtitle);
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Long cue", subtitle.Paragraphs[0].Text);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal(6000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 1);
        Assert.Equal("Second", subtitle.Paragraphs[1].Text);
    }

    // Smooth Streaming style documents restart at zero in every fragment; such cues
    // must be shifted by the sample start (tfdt) to land on the media timeline.
    [Fact]
    public void FragmentedStpp_ZeroBasedDocumentTimes_ShiftedBySampleStart()
    {
        var doc = TtmlDoc("<p begin=\"00:00:01.000\" end=\"00:00:03.000\">Relative</p>");
        var init = BuildInit("subt", "stpp", trackId: 1, mdhdTimeScale: 1000, mvhdTimeScale: 600);
        var segment = BuildSegment(1, 10_000, new uint[] { 4000 }, new[] { Encoding.UTF8.GetBytes(doc) });

        var subtitle = ParseVttc(Concat(init, segment));

        Assert.NotNull(subtitle);
        var p = Assert.Single(subtitle.Paragraphs);
        Assert.Equal(11_000, p.StartTime.TotalMilliseconds, 1);
        Assert.Equal(13_000, p.EndTime.TotalMilliseconds, 1);
    }

    // A progressive (non-fragmented) TTML track uses handler "subt", which was not
    // recognized as a text subtitle at all.
    [Fact]
    public void ProgressiveStpp_SubtHandler_TrackIsVisible()
    {
        var doc = Encoding.UTF8.GetBytes(TtmlDoc("<p begin=\"00:00:02.000\" end=\"00:00:05.000\">Progressive TTML</p>"));
        var file = BuildProgressive("subt", "stpp", new[] { doc }, sampleDurationTicks: 6000, timeScale: 1000);

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, file);
            var parser = new MP4Parser(tempFile);
            var tracks = parser.GetSubtitleTracks();
            var track = Assert.Single(tracks);
            var paragraphs = track.Mdia.Minf.Stbl.GetParagraphs();
            var p = Assert.Single(paragraphs);
            Assert.Equal("Progressive TTML", p.Text);
            Assert.Equal(2000, p.StartTime.TotalMilliseconds, 1);
            Assert.Equal(5000, p.EndTime.TotalMilliseconds, 1);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // The last DASH segment often has no sample duration in trun or tfhd; the
    // per-track default lives in moov/mvex/trex.
    [Fact]
    public void TrexDefaultDuration_FillsSamplesWithoutDuration()
    {
        var init = BuildInit("text", "wvtt", trackId: 1, mdhdTimeScale: 1000, mvhdTimeScale: 600, trexDefaultDuration: 2500);
        var sample = WvttCueSample("Trex default");
        var segment = BuildSegment(trackId: 1, tfdtTicks: 8000, durations: null, samples: new[] { sample });

        var subtitle = ParseVttc(Concat(init, segment));

        Assert.NotNull(subtitle);
        var p = Assert.Single(subtitle.Paragraphs);
        Assert.Equal(8000, p.StartTime.TotalMilliseconds, 1);
        Assert.Equal(10_500, p.EndTime.TotalMilliseconds, 1);
    }

    // A muxed fragmented file can carry several text tracks (e.g. languages); all of
    // them must be exposed so the UI can offer a track picker. VttcSubtitle stays the
    // first track for callers that only handle one.
    [Fact]
    public void FragmentedMultiTrack_ExposesAllTextTracks()
    {
        var init = BuildInit("subt", "stpp", trackId: 2, mdhdTimeScale: 1000, mvhdTimeScale: 600,
            extraTrack: BuildTrak("text", "wvtt", trackId: 1, mdhdTimeScale: 1000));
        var wvttSample = WvttCueSample("English cue");
        var ttmlSample = Encoding.UTF8.GetBytes(TtmlDoc("<p begin=\"00:00:01.000\" end=\"00:00:02.000\">Dansk cue</p>"));
        var segment = BuildMuxedSegment(
            (trackId: 1u, tfdtTicks: 0u, durations: new uint[] { 2000 }, samples: new[] { wvttSample }),
            (trackId: 2u, tfdtTicks: 0u, durations: new uint[] { 4000 }, samples: new[] { ttmlSample }));

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, Concat(init, segment));
            var parser = new MP4Parser(tempFile);

            Assert.Equal(2, parser.FragmentedSubtitleTracks.Count);

            var first = parser.FragmentedSubtitleTracks[0];
            Assert.Equal(1u, first.TrackId);
            Assert.Equal("wvtt", first.Codec);
            Assert.Equal("English cue", Assert.Single(first.Subtitle.Paragraphs).Text);

            var second = parser.FragmentedSubtitleTracks[1];
            Assert.Equal(2u, second.TrackId);
            Assert.Equal("stpp", second.Codec);
            Assert.Equal("Dansk cue", Assert.Single(second.Subtitle.Paragraphs).Text);

            Assert.Same(first.Subtitle, parser.VttcSubtitle);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void SplitTtmlDocuments_ConcatenatedDocuments_SplitsEach()
    {
        var doc1 = TtmlDoc("<p begin=\"00:00:01.000\" end=\"00:00:02.000\">One</p>");
        var doc2 = TtmlDoc("<p begin=\"00:00:03.000\" end=\"00:00:04.000\">Two</p>");

        var docs = Mp4TtmlHelper.SplitTtmlDocuments(doc1 + doc2);

        Assert.Equal(2, docs.Count);
        Assert.Contains("One", docs[0]);
        Assert.DoesNotContain("Two", docs[0]);
        Assert.Contains("Two", docs[1]);
    }

    // "<tt" prefix matching alone would also hit "<ttm:title>" in the head and
    // "</ttm:title>" as a bogus close tag.
    [Fact]
    public void SplitTtmlDocuments_TtmMetadataInHead_DoesNotSplitInsideDocument()
    {
        var doc = "<?xml version=\"1.0\"?><tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:ttm=\"http://www.w3.org/ns/ttml#metadata\"><head><metadata><ttm:title>My title</ttm:title></metadata></head><body><div><p begin=\"00:00:01.000\" end=\"00:00:02.000\">One</p></div></body></tt>";

        var docs = Mp4TtmlHelper.SplitTtmlDocuments(doc + doc);

        Assert.Equal(2, docs.Count);
        Assert.Contains("My title", docs[0]);
        Assert.Contains("</tt>", docs[0]);
    }

    // Matching "</tt" loosely also hit "</tt:p>", so a namespaced document was cut off after
    // its first cue and the truncated fragment no longer parsed as XML at all.
    [Theory]
    [InlineData("tt")]
    [InlineData("ttml")]
    public void SplitTtmlDocuments_NamespacedRoot_KeepsWholeDocument(string prefix)
    {
        var doc = $"<{prefix}:tt xmlns:{prefix}=\"http://www.w3.org/ns/ttml\"><{prefix}:body><{prefix}:div>" +
                  $"<{prefix}:p begin=\"00:00:01.000\" end=\"00:00:02.000\">One</{prefix}:p>" +
                  $"<{prefix}:p begin=\"00:00:03.000\" end=\"00:00:04.000\">Two</{prefix}:p>" +
                  $"</{prefix}:div></{prefix}:body></{prefix}:tt>";

        var docs = Mp4TtmlHelper.SplitTtmlDocuments(doc + doc);

        Assert.Equal(2, docs.Count);
        Assert.All(docs, d =>
        {
            Assert.EndsWith($"</{prefix}:tt>", d);
            var paragraphs = Mp4TtmlHelper.ParseTtmlDocument(d);
            Assert.Equal(2, paragraphs.Count);
            Assert.Equal("One", paragraphs[0].Text);
            Assert.Equal("Two", paragraphs[1].Text);
        });
    }

    [Fact]
    public void FragmentedStpp_NamespacedTtml_ExtractsAllCues()
    {
        var doc = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><tt:tt xmlns:tt=\"http://www.w3.org/ns/ttml\"><tt:body><tt:div>" +
                  "<tt:p begin=\"00:00:01.000\" end=\"00:00:02.000\">One</tt:p>" +
                  "<tt:p begin=\"00:00:02.500\" end=\"00:00:03.500\">Two</tt:p></tt:div></tt:body></tt:tt>";
        var init = BuildInit("subt", "stpp", trackId: 1, mdhdTimeScale: 1000, mvhdTimeScale: 600);
        var segment = BuildSegment(1, 0, new uint[] { 4000 }, new[] { Encoding.UTF8.GetBytes(doc) });

        var subtitle = ParseVttc(Concat(init, segment));

        Assert.NotNull(subtitle);
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("One", subtitle.Paragraphs[0].Text);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal("Two", subtitle.Paragraphs[1].Text);
        Assert.Equal(2500, subtitle.Paragraphs[1].StartTime.TotalMilliseconds, 1);
    }

    // The zero-based-times heuristic used to look only at cue ends versus the sample duration,
    // so an absolute cue that happened to end inside a long sample was shifted by tfdt.
    [Fact]
    public void FragmentedStpp_AbsoluteDocumentTimes_AreNotShifted()
    {
        var doc = TtmlDoc("<p begin=\"00:00:02.500\" end=\"00:00:05.000\">Absolute</p>");
        var init = BuildInit("subt", "stpp", trackId: 1, mdhdTimeScale: 1000, mvhdTimeScale: 600);
        var segment = BuildSegment(1, 2000, new uint[] { 10_000 }, new[] { Encoding.UTF8.GetBytes(doc) });

        var subtitle = ParseVttc(Concat(init, segment));

        Assert.NotNull(subtitle);
        var p = Assert.Single(subtitle.Paragraphs);
        Assert.Equal(2500, p.StartTime.TotalMilliseconds, 1);
        Assert.Equal(5000, p.EndTime.TotalMilliseconds, 1);
    }

    // A DASH file is styp+moof+mdat per segment, so a flat 3000-box scan limit stopped the
    // parse after ~1000 segments and silently dropped the rest of the subtitle.
    [Fact]
    public void ManySegments_AreNotTruncatedByTheBoxScanLimit()
    {
        const int segmentCount = 1100; // > 3000 top-level boxes
        var parts = new System.Collections.Generic.List<byte[]> { BuildInit("text", "wvtt", 1, 1000, 600) };
        for (var i = 0; i < segmentCount; i++)
        {
            parts.Add(BuildSegment(1, (uint)(i * 2000), new uint[] { 2000 }, new[] { WvttCueSample($"Cue {i}") }));
        }

        var subtitle = ParseVttc(Concat(parts.ToArray()));

        Assert.NotNull(subtitle);
        Assert.Equal(segmentCount, subtitle.Paragraphs.Count);
        Assert.Equal($"Cue {segmentCount - 1}", subtitle.Paragraphs[segmentCount - 1].Text);
    }

    // IsmtDfxp splits each mdat into TTML documents before parsing; a namespaced document must
    // survive that split intact, as the whole mdat used to go straight to TimedText10.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IsmtDfxp_TtmlInMdat_LoadsAllCues(bool namespaced)
    {
        var xml = namespaced
            ? "<?xml version=\"1.0\" encoding=\"UTF-8\"?><tt:tt xmlns:tt=\"http://www.w3.org/ns/ttml\"><tt:body><tt:div>" +
              "<tt:p begin=\"00:00:01.000\" end=\"00:00:02.000\">One</tt:p>" +
              "<tt:p begin=\"00:00:03.000\" end=\"00:00:04.000\">Two</tt:p></tt:div></tt:body></tt:tt>"
            : TtmlDoc("<p begin=\"00:00:01.000\" end=\"00:00:02.000\">One</p><p begin=\"00:00:03.000\" end=\"00:00:04.000\">Two</p>");

        var file = Concat(
            Box("ftyp", Ascii("isml"), UInt32Be(1), Ascii("piff"), Ascii("iso2")),
            Box("moov", BuildTrak("sbtl", "dfxp", trackId: 1, mdhdTimeScale: 10_000_000)),
            Box("mdat", Encoding.UTF8.GetBytes(xml)));

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, file);
            var subtitle = new Nikse.SubtitleEdit.Core.Common.Subtitle();
            new Nikse.SubtitleEdit.Core.SubtitleFormats.IsmtDfxp().LoadSubtitle(subtitle, new System.Collections.Generic.List<string>(), tempFile);

            Assert.Equal(2, subtitle.Paragraphs.Count);
            Assert.Equal("One", subtitle.Paragraphs[0].Text);
            Assert.Equal("Two", subtitle.Paragraphs[1].Text);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private static Nikse.SubtitleEdit.Core.Common.Subtitle ParseVttc(byte[] fileBytes)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, fileBytes);
            return new MP4Parser(tempFile).VttcSubtitle;
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private static string TtmlDoc(string paragraphs) =>
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?><tt xmlns=\"http://www.w3.org/ns/ttml\" xml:lang=\"en\"><body><div>" + paragraphs + "</div></body></tt>";

    private static byte[] WvttCueSample(string text) =>
        Box("vttc", Box("payl", Encoding.UTF8.GetBytes(text)));

    /// <summary>Init segment: ftyp + moov with one (or two) tracks and optional mvex/trex.</summary>
    private static byte[] BuildInit(string handlerType, string stsdCodec, uint trackId, uint mdhdTimeScale, uint mvhdTimeScale, byte[]? extraTrack = null, uint trexDefaultDuration = 0)
    {
        var ftyp = Box("ftyp", Ascii("iso6"), UInt32Be(512), Ascii("iso6"), Ascii("dash"));

        var mvhd = BuildMvhd(mvhdTimeScale);
        var trak = BuildTrak(handlerType, stsdCodec, trackId, mdhdTimeScale);

        var moovParts = new System.Collections.Generic.List<byte[]> { mvhd };
        if (extraTrack != null)
        {
            moovParts.Add(extraTrack);
        }

        moovParts.Add(trak);

        if (trexDefaultDuration > 0)
        {
            var trex = Box("trex",
                new byte[4],                     // version + flags
                UInt32Be(trackId),
                UInt32Be(1),                     // default_sample_description_index
                UInt32Be(trexDefaultDuration),
                UInt32Be(0),                     // default_sample_size
                UInt32Be(0));                    // default_sample_flags
            moovParts.Add(Box("mvex", trex));
        }

        var moov = Box("moov", moovParts.ToArray());
        return Concat(ftyp, moov);
    }

    private static byte[] BuildMvhd(uint timeScale)
    {
        // v0 mvhd content is 100 bytes; the parser reads timescale at offset 12.
        var content = new byte[100];
        WriteUInt32Be(content, 12, timeScale);
        WriteUInt32Be(content, 16, timeScale * 30); // duration
        return Box("mvhd", content);
    }

    private static byte[] BuildTrak(string handlerType, string? stsdCodec, uint trackId, uint mdhdTimeScale)
    {
        // v0 tkhd content is 84 bytes; track id at offset 12.
        var tkhdContent = new byte[84];
        WriteUInt32Be(tkhdContent, 12, trackId);
        var tkhd = Box("tkhd", tkhdContent);

        var hdlr = Box("hdlr",
            new byte[4],
            new byte[4],
            Ascii(handlerType),
            new byte[12],
            new byte[] { 0 });

        var mdhd = Box("mdhd",
            new byte[4],
            new byte[4],
            new byte[4],
            UInt32Be(mdhdTimeScale),
            UInt32Be(mdhdTimeScale * 30),
            UInt16Be(0x55C4),                    // language = "und"
            UInt16Be(0));

        if (stsdCodec != null)
        {
            var stsd = Box("stsd", new byte[4], UInt32Be(1), Box(stsdCodec));
            var stbl = Box("stbl", stsd,
                Box("stts", new byte[4], UInt32Be(0)),
                Box("stsc", new byte[4], UInt32Be(0)),
                Box("stsz", new byte[4], UInt32Be(0), UInt32Be(0)),
                Box("stco", new byte[4], UInt32Be(0)));
            var minf = Box("minf", stbl);
            return Box("trak", tkhd, Box("mdia", hdlr, mdhd, minf));
        }

        return Box("trak", tkhd, Box("mdia", hdlr, mdhd));
    }

    /// <summary>Media segment: styp + moof (single traf) + mdat.</summary>
    private static byte[] BuildSegment(uint trackId, uint tfdtTicks, uint[]? durations, byte[][] samples)
    {
        return BuildMuxedSegment((trackId, tfdtTicks, durations, samples));
    }

    /// <summary>
    /// Media segment with one traf per track; all sample data shares one mdat and each
    /// traf's trun uses a moof-relative data offset to its own byte range.
    /// </summary>
    private static byte[] BuildMuxedSegment(params (uint trackId, uint tfdtTicks, uint[]? durations, byte[][] samples)[] tracks)
    {
        var styp = Box("styp", Ascii("msdh"), UInt32Be(0), Ascii("msdh"), Ascii("dash"));

        // two passes: trun data offsets depend on the finished moof size
        var moof = BuildMoof(tracks, new int[tracks.Length]);
        var dataOffsets = new int[tracks.Length];
        var runningOffset = moof.Length + 8; // + mdat header
        for (var i = 0; i < tracks.Length; i++)
        {
            dataOffsets[i] = runningOffset;
            runningOffset += tracks[i].samples.Sum(s => s.Length);
        }

        moof = BuildMoof(tracks, dataOffsets);
        var mdat = Box("mdat", tracks.SelectMany(t => t.samples).ToArray());
        return Concat(styp, moof, mdat);
    }

    private static byte[] BuildMoof((uint trackId, uint tfdtTicks, uint[]? durations, byte[][] samples)[] tracks, int[] dataOffsets)
    {
        var mfhd = Box("mfhd", new byte[4], UInt32Be(1));
        var trafs = new byte[tracks.Length][];
        for (var i = 0; i < tracks.Length; i++)
        {
            var (trackId, tfdtTicks, durations, samples) = tracks[i];

            var tfhd = Box("tfhd",
                UInt32Be(0x020000),              // default-base-is-moof, no optional fields
                UInt32Be(trackId));

            var tfdt = Box("tfdt", new byte[4], UInt32Be(tfdtTicks));

            var trunFlags = 0x000201u;           // data-offset + sample-size
            if (durations != null)
            {
                trunFlags |= 0x000100;           // + sample-duration
            }

            var trunParts = new System.Collections.Generic.List<byte[]>
            {
                UInt32Be(trunFlags),
                UInt32Be((uint)samples.Length),
                UInt32Be((uint)dataOffsets[i]),
            };
            for (var s = 0; s < samples.Length; s++)
            {
                if (durations != null)
                {
                    trunParts.Add(UInt32Be(durations[s]));
                }

                trunParts.Add(UInt32Be((uint)samples[s].Length));
            }

            var trun = Box("trun", trunParts.ToArray());
            trafs[i] = Box("traf", tfhd, tfdt, trun);
        }

        return Box("moof", new[] { mfhd }.Concat(trafs).ToArray());
    }

    /// <summary>Progressive MP4 with a single subtitle track (classic stbl sample tables).</summary>
    private static byte[] BuildProgressive(string handlerType, string stsdCodec, byte[][] samples, uint sampleDurationTicks, uint timeScale)
    {
        var ftyp = Box("ftyp", Ascii("isom"), UInt32Be(512), Ascii("isom"), Ascii("iso2"));
        var mdat = Box("mdat", samples);
        var sampleDataOffset = (uint)(ftyp.Length + 8);

        var stsd = Box("stsd", new byte[4], UInt32Be(1), Box(stsdCodec));
        var stts = Box("stts", new byte[4], UInt32Be(1), UInt32Be((uint)samples.Length), UInt32Be(sampleDurationTicks));
        var stsc = Box("stsc", new byte[4], UInt32Be(1), UInt32Be(1), UInt32Be((uint)samples.Length), UInt32Be(1));
        var stsz = Box("stsz", new byte[4], UInt32Be(0), UInt32Be((uint)samples.Length),
            Concat(samples.Select(s => UInt32Be((uint)s.Length)).ToArray()));
        var stco = Box("stco", new byte[4], UInt32Be(1), UInt32Be(sampleDataOffset));
        var stbl = Box("stbl", stsd, stts, stsc, stsz, stco);
        var minf = Box("minf", stbl);

        var hdlr = Box("hdlr", new byte[4], new byte[4], Ascii(handlerType), new byte[12], new byte[] { 0 });
        var mdhd = Box("mdhd", new byte[4], new byte[4], new byte[4],
            UInt32Be(timeScale), UInt32Be(sampleDurationTicks * (uint)samples.Length), UInt16Be(0x55C4), UInt16Be(0));

        var tkhdContent = new byte[84];
        WriteUInt32Be(tkhdContent, 12, 1);
        var trak = Box("trak", Box("tkhd", tkhdContent), Box("mdia", hdlr, mdhd, minf));
        var moov = Box("moov", trak);

        return Concat(ftyp, mdat, moov);
    }

    private static byte[] Box(string name, params byte[][] parts)
    {
        var total = 8;
        foreach (var p in parts)
        {
            total += p.Length;
        }

        var box = new byte[total];
        WriteUInt32Be(box, 0, (uint)total);
        Encoding.ASCII.GetBytes(name, 0, 4, box, 4);
        var off = 8;
        foreach (var p in parts)
        {
            System.Buffer.BlockCopy(p, 0, box, off, p.Length);
            off += p.Length;
        }

        return box;
    }

    private static byte[] Concat(params byte[][] parts)
    {
        var total = 0;
        foreach (var p in parts)
        {
            total += p.Length;
        }

        var result = new byte[total];
        var off = 0;
        foreach (var p in parts)
        {
            System.Buffer.BlockCopy(p, 0, result, off, p.Length);
            off += p.Length;
        }

        return result;
    }

    private static byte[] Ascii(string s) => Encoding.ASCII.GetBytes(s);

    private static byte[] UInt32Be(uint v) => new[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v };

    private static byte[] UInt16Be(ushort v) => new[] { (byte)(v >> 8), (byte)v };

    private static void WriteUInt32Be(byte[] dst, int off, uint v)
    {
        dst[off] = (byte)(v >> 24);
        dst[off + 1] = (byte)(v >> 16);
        dst[off + 2] = (byte)(v >> 8);
        dst[off + 3] = (byte)v;
    }
}
