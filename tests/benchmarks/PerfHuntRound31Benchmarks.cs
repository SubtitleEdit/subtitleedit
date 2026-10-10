using System.Globalization;
using System.Reflection;
using System.Text;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.CDG;
using Nikse.SubtitleEdit.Core.Common;
using AribCaptionParser = Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream.AribCaptionParser;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Benchmarks;

// Round 31. Every benchmark calls the real code, so the class is run once against the commit
// before the change and once after; ControlAutoBreak and SubtitleCopyControl are drift controls.

/// <summary>Fix common errors rules, JSON parsing, CD+G frames, ARIB roll-up merge.</summary>
[MemoryDiagnoser]
public class Round31Benchmarks
{
    private static readonly string[] Texts =
    {
        "It was the best of times {0}, it was the worst",
        "<i>I told you already {0}</i>",
        "- No, Mrs. smith {0}.\r\n- Then <i>stay</i> here and wait",
        "...somewhere in Denmark {0},\r\na quiet evening",
        "♪ Plain line {0} with <b>bold</b> ♪",
        "Tom and \"Jerry\" {0} were here",
        "[DOOR SLAMS] {0}",
        "MAN: Are you coming with us, John {0}",
        "Hvad så, går det godt {0}? Ja, det gør det",
        "Somebody said hello {0}...",
    };

    private Subtitle _subtitle = null!;
    private readonly StubFixCallbacks _callbacks = new();
    private string _json = null!;
    private List<Packet> _cdgPackets = null!;
    private Action<List<Paragraph>> _aribMerge = null!;

    [Params(5000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _subtitle = new Subtitle();
        for (var i = 0; i < Lines; i++)
        {
            _subtitle.Paragraphs.Add(new Paragraph(string.Format(Texts[i % Texts.Length], i), i * 2500, i * 2500 + 2000));
        }

        _subtitle.Renumber();

        // Word-level transcript JSON, as TranscriptiveJson / JsonType14 / UnknownFormatImporter read it.
        var sb = new StringBuilder("{\"words\":[");
        for (var i = 0; i < Lines * 8; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            sb.Append("{\"text\":\"word").Append(i).Append("\",\"start\":").Append((i * 0.25m).ToString(CultureInfo.InvariantCulture)).Append(",\"end\":")
              .Append((i * 0.25m + 0.2m).ToString(CultureInfo.InvariantCulture)).Append(",\"speaker\":\"Speaker ").Append(i % 3).Append("\",\"confidence\":0.98,\"index\":").Append(i).Append('}');
        }

        sb.Append("]}");
        _json = sb.ToString();

        _cdgPackets = BuildCdgPackets(2000);

        var method = typeof(AribCaptionParser).GetMethod("MergeContinuations", BindingFlags.NonPublic | BindingFlags.Static)!;
        _aribMerge = (Action<List<Paragraph>>)Delegate.CreateDelegate(typeof(Action<List<Paragraph>>), method);
    }

    private static List<Packet> BuildCdgPackets(int count)
    {
        var packets = new List<Packet>(count);
        var random = new Random(31);
        for (var i = 0; i < count; i++)
        {
            var data = new byte[24];
            data[0] = 9; // Graphic
            if (i == 0)
            {
                data[1] = 30; // LoadColorTableLower
                for (var c = 0; c < 8; c++)
                {
                    data[4 + c * 2] = (byte)(c * 7 & 0x3F);
                    data[5 + c * 2] = (byte)(c * 5 & 0x3F);
                }
            }
            else if (i == 1)
            {
                data[1] = 1; // MemoryPreset color 0
            }
            else
            {
                data[1] = 6; // TileBlockNormal
                data[4] = 0;
                data[5] = (byte)(1 + i % 7);
                data[6] = (byte)random.Next(18);
                data[7] = (byte)random.Next(50);
                for (var k = 8; k < 20; k++)
                {
                    data[k] = (byte)random.Next(64);
                }
            }

            packets.Add(new Packet(data));
        }

        return packets;
    }

    private Subtitle Copy() => new Subtitle(_subtitle);

    [Benchmark]
    public int FceNormalizeStrings()
    {
        var s = Copy();
        new NormalizeStrings().Fix(s, _callbacks);
        return s.Paragraphs.Count;
    }

    [Benchmark]
    public int FceFixMusicNotation()
    {
        var s = Copy();
        new FixMusicNotation().Fix(s, _callbacks);
        return s.Paragraphs.Count;
    }

    [Benchmark]
    public int FceFixUnnecessaryLeadingDots()
    {
        var s = Copy();
        new FixUnnecessaryLeadingDots().Fix(s, _callbacks);
        return s.Paragraphs.Count;
    }

    /// <summary>What the three FCE rows above pay for their own copy of the subtitle.</summary>
    [Benchmark]
    public int SubtitleCopyControl() => Copy().Paragraphs.Count;

    [Benchmark]
    public object JsonParse() => new JsonParser().Parse(_json);

    /// <summary>One CD+G frame grab every 20 packets, as CdgToImageList does.</summary>
    [Benchmark]
    public int CdgToBitmap()
    {
        var graphics = new CdgGraphics(_cdgPackets);
        var total = 0;
        for (var packet = 20; packet < _cdgPackets.Count; packet += 20)
        {
            using var bmp = graphics.ToBitmap(packet);
            total += bmp?.Width ?? 0;
        }

        return total;
    }

    /// <summary>Roll-up ARIB captions: every word repaints the line, so ten statements collapse into one.</summary>
    [Benchmark]
    public int AribMergeContinuations()
    {
        var list = new List<Paragraph>(Lines * 4);
        for (var i = 0; i < Lines * 4; i++)
        {
            var chain = i / 10;
            var step = i % 10;
            var text = "Caption " + chain + new string('w', step + 1);
            var start = i * 300.0;
            list.Add(new Paragraph(text, start, start + 300));
        }

        _aribMerge(list);
        return list.Count;
    }

    /// <summary>Drift control - untouched by this round.</summary>
    [Benchmark]
    public int ControlAutoBreak()
    {
        var total = 0;
        for (var i = 0; i < 3000; i++)
        {
            total += Utilities.AutoBreakLine(BenchmarkSubtitles.Sentences[i % BenchmarkSubtitles.Sentences.Length]).Length;
        }

        return total;
    }

    private sealed class StubFixCallbacks : IFixCallbacks
    {
        public bool AllowFix(Paragraph p, string action) => true;
        public void AddFixToListView(Paragraph p, string action, string before, string after) { }
        public void AddFixToListView(Paragraph p, string action, string before, string after, bool isChecked) { }
        public void LogStatus(string sender, string message) { }
        public void LogStatus(string sender, string message, bool isImportant) { }
        public void UpdateFixStatus(int fixes, string message) { }
        public bool IsName(string candidate) => false;
        public HashSet<string> GetAbbreviations() => new();
        public void AddToTotalErrors(int count) { }
        public void AddToDeleteIndices(int index) { }
        public SubtitleFormat Format { get; } = new SubRip();
        public Encoding Encoding { get; } = Encoding.UTF8;
        public string Language => "en";
    }
}

/// <summary>
/// UI view-model paths reached by reflection (both run against the commit before the change and
/// after): Apply duration limits preview with shot changes, and the TTS "spoken text in video"
/// lookup for a translation whose lines no longer share the original's start times.
/// </summary>
[MemoryDiagnoser]
public class Round31UiBenchmarks
{
    private Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits.ApplyDurationLimitsViewModel _durationLimits = null!;
    private Action _buildPreview = null!;
    private Subtitle _translation = null!;
    private Func<Paragraph, string?> _spokenText = null!;

    [Params(5000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        // Every line is 300 ms (too short), one shot change every ~2.3 s.
        var lines = new List<SubtitleLineViewModel>(Lines);
        var srt = new SubRip();
        for (var i = 0; i < Lines; i++)
        {
            lines.Add(new SubtitleLineViewModel(new Paragraph("Line " + i, i * 3000, i * 3000 + 300), srt));
        }

        var shotChanges = new List<double>();
        for (var t = 1.1; t < Lines * 3.0; t += 2.3)
        {
            shotChanges.Add(t);
        }

        var type = typeof(Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits.ApplyDurationLimitsViewModel);
        _durationLimits = new Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits.ApplyDurationLimitsViewModel();
        type.GetField("_allSubtitles", flags)!.SetValue(_durationLimits, lines);
        type.GetField("_shotChanges", flags)!.SetValue(_durationLimits, shotChanges);
        _durationLimits.DoNotGoPastShotChange = true;
        _durationLimits.FixMinDurationMs = true;
        _durationLimits.FixMaxDurationMs = false;
        _durationLimits.MinDurationMsOrFrames = 1200;
        _durationLimits.MaxDurationMsOrFrames = 8000;
        _buildPreview = (Action)Delegate.CreateDelegate(typeof(Action), _durationLimits, type.GetMethod("BuildPreview", flags)!);

        // Original and a translation shifted by 40 ms, so no start time matches exactly.
        var original = new Subtitle();
        _translation = new Subtitle();
        for (var i = 0; i < Lines; i++)
        {
            original.Paragraphs.Add(new Paragraph("Original line " + i, i * 2500, i * 2500 + 2000));
            _translation.Paragraphs.Add(new Paragraph("Translated line " + i, i * 2500 + 40, i * 2500 + 2040));
        }

        var ttsType = typeof(Nikse.SubtitleEdit.Features.Video.TextToSpeech.TextToSpeechViewModel);
        var tts = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(ttsType);
        ttsType.GetField("_originalSubtitle", flags)!.SetValue(tts, original);
        _spokenText = (Func<Paragraph, string?>)Delegate.CreateDelegate(typeof(Func<Paragraph, string?>), tts, ttsType.GetMethod("GetSpokenTextInVideo", flags)!);
    }

    [Benchmark]
    public int ApplyDurationLimitsPreview()
    {
        _buildPreview();
        return _durationLimits.AllSubtitlesFixed.Count;
    }

    /// <summary>Every 5th translated line looked up, as a dub's per-line clone pass does.</summary>
    [Benchmark]
    public int TtsSpokenTextInVideo()
    {
        var total = 0;
        for (var i = 0; i < _translation.Paragraphs.Count; i += 5)
        {
            total += _spokenText(_translation.Paragraphs[i])?.Length ?? 0;
        }

        return total;
    }
}

/// <summary>
/// Ripple delete and the column shift/rotate commands map every selected row to its position:
/// the old <c>column.IndexOf</c> per row against <c>MainViewModel.IndexLookup</c> (reached by
/// reflection so the class still compiles against the commit before the change).
/// </summary>
[MemoryDiagnoser]
public class Round31RowIndexBenchmarks
{
    private List<SubtitleLineViewModel> _column = null!;
    private List<SubtitleLineViewModel> _selected = null!;
    private Func<List<SubtitleLineViewModel>, Func<SubtitleLineViewModel, int>>? _indexLookup;

    [Params(1000, 10000)]
    public int Selected { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var srt = new SubRip();
        _column = new List<SubtitleLineViewModel>(10000);
        for (var i = 0; i < 10000; i++)
        {
            _column.Add(new SubtitleLineViewModel(new Paragraph("Line " + i, i * 2000, i * 2000 + 1500), srt));
        }

        _selected = _column.Skip(10000 - Selected).ToList();
        var method = typeof(MainViewModel).GetMethod("IndexLookup", BindingFlags.NonPublic | BindingFlags.Static);
        if (method != null)
        {
            _indexLookup = (Func<List<SubtitleLineViewModel>, Func<SubtitleLineViewModel, int>>)Delegate.CreateDelegate(
                typeof(Func<List<SubtitleLineViewModel>, Func<SubtitleLineViewModel, int>>), method);
        }
    }

    [Benchmark(Baseline = true)]
    public int IndexOfPerRow() => _selected
        .Select(x => _column.IndexOf(x))
        .Where(i => i >= 0 && i < _column.Count)
        .Distinct()
        .OrderBy(i => i)
        .ToList().Count;

    [Benchmark]
    public int IndexLookup() => _selected
        .Select(_indexLookup!(_column))
        .Where(i => i >= 0 && i < _column.Count)
        .Distinct()
        .OrderBy(i => i)
        .ToList().Count;
}

/// <summary>
/// Merge lines with same text preview (run against the commit before the change and after):
/// 6000 lines in groups of three identical lines, so 2000 merge items.
/// </summary>
[MemoryDiagnoser]
public class Round31MergeSameTextBenchmarks
{
    private Action _updatePreview = null!;
    private object _vm = null!;
    private System.Collections.ICollection _mergeItems = null!;

    [Params(6000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var type = typeof(Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameText.MergeSameTextViewModel);

        var lines = new List<SubtitleLineViewModel>(Lines);
        var srt = new SubRip();
        for (var i = 0; i < Lines; i++)
        {
            var line = new SubtitleLineViewModel(new Paragraph("Same text " + i / 3, i * 1000, i * 1000 + 900), srt) { Number = i + 1 };
            lines.Add(line);
        }

        // The constructor builds an Avalonia TableView; the preview only needs these fields.
        _vm = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);
        _mergeItems = new System.Collections.ObjectModel.ObservableCollection<Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameText.MergeDisplayItem>();
        type.GetField("_mergeItems", flags)!.SetValue(_vm, _mergeItems);
        type.GetField("_mergeSubtitles", flags)!.SetValue(_vm, new System.Collections.ObjectModel.ObservableCollection<SubtitleLineViewModel>());
        type.GetField("_maxMsOrFramesBetweenLines", flags)!.SetValue(_vm, 250);
        type.GetField("_subtitles", flags)!.SetValue(_vm, lines);
        _updatePreview = (Action)Delegate.CreateDelegate(typeof(Action), _vm, type.GetMethod("UpdatePreview", flags)!);
    }

    [Benchmark]
    public int MergeSameTextPreview()
    {
        _updatePreview();
        return _mergeItems.Count;
    }
}
