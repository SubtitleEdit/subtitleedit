using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// With "no line break after" enabled, <see cref="Utilities.AutoBreakLine(string, string)"/> asks
/// <c>CanBreak</c> about every space in the line, and CanBreak walks the whole per-language list.
/// The multi-word phrase check (issue #9631) added a second walk plus a substring per candidate,
/// even though only 2 of the 33 shipped lists contain a multi-word entry at all.
///
/// The list sizes mirror the shipped files: 19 items for English, 117 for Greek (the largest).
/// </summary>
[MemoryDiagnoser]
public class NoBreakAfterBenchmarks
{
    private const string Long = "It was the best of times, it was the worst of times, it was the age of wisdom, " +
                                "it was the age of foolishness, it was the epoch of belief.";

    private string _dictionaryFolder = string.Empty;
    private string _oldDataDirectory = string.Empty;
    private bool _oldUseNoLineBreakAfter;

    /// <summary>Entries in the language's NoBreakAfterList.</summary>
    [Params(19, 117)]
    public int ListItems { get; set; }

    /// <summary>Whether the list contains a multi-word phrase (only bg and mk do).</summary>
    [Params(false, true)]
    public bool MultiWord { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _oldDataDirectory = Configuration.DataDirectory;
        _oldUseNoLineBreakAfter = Configuration.Settings.Tools.UseNoLineBreakAfter;
        _dictionaryFolder = Path.Combine(Path.GetTempPath(), "seNoBreakAfterBench_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_dictionaryFolder, "Dictionaries"));

        var items = new List<string>();
        for (var i = 0; i < ListItems; i++)
        {
            items.Add("wrd" + i + ".");
        }

        if (MultiWord)
        {
            items[items.Count - 1] = "SORT OF";
        }

        var xml = "<NoBreakAfterList>" + Environment.NewLine +
                  string.Join(Environment.NewLine, items.Select(p => "  <Item>" + p + "</Item>")) +
                  Environment.NewLine + "</NoBreakAfterList>";
        File.WriteAllText(Path.Combine(_dictionaryFolder, "Dictionaries", "zz_NoBreakAfterList.xml"), xml);

        Configuration.DataDirectory = _dictionaryFolder;
        Configuration.Settings.Tools.UseNoLineBreakAfter = true;
        Utilities.ResetNoBreakAfterList();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        Configuration.DataDirectory = _oldDataDirectory;
        Configuration.Settings.Tools.UseNoLineBreakAfter = _oldUseNoLineBreakAfter;
        Utilities.ResetNoBreakAfterList();
        try
        {
            Directory.Delete(_dictionaryFolder, true);
        }
        catch
        {
            // best effort
        }
    }

    [Benchmark]
    public string AutoBreakLine() => Utilities.AutoBreakLinePrivate(Long, 43, 0, "zz", false);
}
