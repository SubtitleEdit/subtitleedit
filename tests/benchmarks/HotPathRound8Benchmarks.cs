using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 8 of the hot-path hunt: vectorized character scanning and no-match fast paths,
/// the technique from PR #13561. Every case runs over a realistic mixed subtitle corpus
/// (plain lines, HTML-tagged lines, ASSA-tagged lines, multi-line dialog).
///
/// DriftControl* are deliberately untouched by this round - if they move, the run pair is
/// invalid (see the benchmark-verification notes).
/// </summary>
[MemoryDiagnoser]
public class HotPathRound8Benchmarks
{
    private string[] _lines = null!;
    private string[] _plainLines = null!;
    private string _bigText = null!;
    private Subtitle _detectSubtitle = null!;
    private string _cjkText = null!;
    private string _json = null!;
    private TextToSingleLineConverter _singleLineConverter = null!;
    private ISpellCheckManager _spellCheckManager = null!;

    private static readonly string[] Corpus =
    {
        "It was the best of times, it was the worst of times.",
        "<i>Are you coming with us?</i>",
        "- No.\r\n- Then stay here and wait.",
        "{\\an8}Somewhere in Denmark",
        "<font color=\"#ff0000\">Look out!</font>",
        "I told you already, this is not going\r\nto work out the way you think it will.",
        "Hello.",
        "{\\i1}Whispering{\\i0} in the dark...",
        "<b>WARNING:</b> contains flashing images",
        "Well... I don't know, Mr. Anderson.",
        "The quick brown fox jumps over the lazy dog.",
        "<i>- What is it?\r\n- Nothing.</i>",
    };

    [GlobalSetup]
    public void Setup()
    {
        _lines = new string[600];
        for (var i = 0; i < _lines.Length; i++)
        {
            _lines[i] = Corpus[i % Corpus.Length];
        }

        _plainLines = new string[600];
        for (var i = 0; i < _plainLines.Length; i++)
        {
            _plainLines[i] = Corpus[(i * 5) % Corpus.Length].Replace("<", string.Empty).Replace(">", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty);
        }

        var sb = new StringBuilder();
        for (var i = 0; i < 800; i++)
        {
            sb.AppendLine(Corpus[i % Corpus.Length]);
        }

        _bigText = sb.ToString();

        _detectSubtitle = new Subtitle();
        for (var i = 0; i < 800; i++)
        {
            _detectSubtitle.Paragraphs.Add(new Paragraph(Corpus[i % Corpus.Length], i * 2000, i * 2000 + 1800));
        }

        var cjk = new StringBuilder();
        for (var i = 0; i < 800; i++)
        {
            cjk.AppendLine("我們明天早上再談這件事吧。");
            cjk.AppendLine("你確定這是正確的做法嗎？");
        }

        _cjkText = cjk.ToString();

        var json = new StringBuilder();
        json.Append("{\"translations\":[");
        for (var i = 0; i < 400; i++)
        {
            if (i > 0)
            {
                json.Append(',');
            }

            json.Append("{\"index\":").Append(i).Append(",\"text\":\"").Append("It was the best of times, it was the worst of times.").Append("\",\"score\":0.9812}");
        }

        json.Append("]}");
        _json = json.ToString();

        _singleLineConverter = new TextToSingleLineConverter();
        _spellCheckManager = new NoopSpellCheckManager();
    }

    // 1. Subtitle grid rows and the edit box tokenize on every repaint / keystroke.
    [Benchmark]
    public int TokenizeSyntax()
    {
        var sum = 0;
        foreach (var line in _lines)
        {
            sum += SubtitleSyntaxTokenizer.Tokenize(line).Count;
        }

        return sum;
    }

    // 2. RemoveHtmlTags has 340+ call sites; character-count and CPS run it per repaint.
    [Benchmark]
    public int RemoveHtmlTags()
    {
        var sum = 0;
        foreach (var line in _lines)
        {
            sum += HtmlUtil.RemoveHtmlTags(line, true).Length;
        }

        return sum;
    }

    // 3. Language auto detect probes 40+ word lists; each probe built its pattern string.
    [Benchmark]
    public string AutoDetectLanguage()
    {
        return LanguageAutoDetect.AutoDetectGoogleLanguage(_detectSubtitle, 500);
    }

    // 4. Script detection over a whole file - reached when word detection finds nothing.
    [Benchmark]
    public string EncodingViaLetter()
    {
        return LanguageAutoDetect.GetEncodingViaLetter(_cjkText);
    }

    // 5. Every auto-translate / TTS engine response is parsed with this.
    [Benchmark]
    public int JsonParse()
    {
        return new SeJsonParser().GetAllTagsByNameAsStrings(_json, "text").Count;
    }

    // 6. Character count runs per grid row repaint, per keystroke and per waveform frame.
    [Benchmark]
    public decimal CountCharacters()
    {
        var calc = new CalcAll();
        decimal sum = 0;
        foreach (var line in _lines)
        {
            sum += calc.CountCharacters(line, false);
        }

        return sum;
    }

    // 7. The edit box re-scans its text for misspellings on every keystroke.
    [Benchmark]
    public int MisspelledWords()
    {
        var sum = 0;
        foreach (var line in _lines)
        {
            sum += SpellCheckWordScanner.GetMisspelledWords(line, _spellCheckManager).Count;
        }

        return sum;
    }

    // 8. Grid text column converter - once per visible row per repaint.
    [Benchmark]
    public int SingleLineConvert()
    {
        var sum = 0;
        foreach (var line in _lines)
        {
            sum += ((string)_singleLineConverter.Convert(line, typeof(string), null, CultureInfo.InvariantCulture)).Length;
        }

        return sum;
    }

    // 9. Constructed per paragraph by several fix-common-errors rules and by name casing.
    [Benchmark]
    public int StrippableTexts()
    {
        var sum = 0;
        foreach (var line in _lines)
        {
            sum += new StrippableText(line).StrippedText.Length;
        }

        return sum;
    }

    // 10. JSON subtitle writers and several web payloads escape every line with this.
    [Benchmark]
    public int EncodeJson()
    {
        var sum = 0;
        foreach (var line in _lines)
        {
            sum += Json.EncodeJsonText(line).Length;
        }

        return sum;
    }

    // 11. "Remove unneeded spaces" runs per paragraph in fix common errors and batch convert.
    [Benchmark]
    public int RemoveUnneededSpaces()
    {
        var sum = 0;
        foreach (var line in _lines)
        {
            sum += Utilities.RemoveUnneededSpaces(line, "en").Length;
        }

        return sum;
    }

    // --- drift controls: untouched by this round ---

    [Benchmark]
    public int DriftControlAutoBreak()
    {
        var sum = 0;
        foreach (var line in _plainLines)
        {
            sum += Utilities.AutoBreakLine(line).Length;
        }

        return sum;
    }

    [Benchmark]
    public int DriftControlRemoveOpenCloseTags()
    {
        var sum = 0;
        foreach (var line in _lines)
        {
            sum += HtmlUtil.RemoveOpenCloseTags(line, "font", "i", "b", "u").Length;
        }

        return sum;
    }
}

/// <summary>
/// Spell check manager that says every word is correct, so the benchmark measures
/// <see cref="SpellCheckWordScanner"/>'s own scanning and not a dictionary lookup.
/// </summary>
internal sealed class NoopSpellCheckManager : ISpellCheckManager
{
    public event SpellCheckManager.SpellCheckWordChangedHandler? OnWordChanged { add { } remove { } }

    public bool IsWordCorrect(SpellCheckWord word, string allText) => true;

    public List<SpellCheckResult> CheckSpelling(ObservableCollection<SubtitleLineViewModel> subtitles, SpellCheckResult? startFrom = null, int? stopBeforeLineIndex = null) => new();

    public int NoOfChangedWords { get; set; }
    public int NoOfSkippedWords { get; set; }
    public int NoOfCorrectWords { get; set; }
    public int NoOfNames { get; set; }
    public int NoOfAddedWords { get; set; }
    public IWordSpellChecker? WordSpellChecker { get; set; }

    public void AddIgnoreWord(string word) { }
    public void ChangeWord(string fromWord, string toWord, SpellCheckWord spellCheckWord, SubtitleLineViewModel p) { }
    public void ChangeAllWord(string fromWord, string toWord, SpellCheckWord spellCheckWord, SubtitleLineViewModel p) { }
    public void AddToNames(string currentWord) { }
    public void AdToUserDictionary(string currentWord) { }
    public void RemoveIgnoreWord(string word) { }
    public void RemoveChangeAllWord(string fromWord) { }
    public void RemoveFromNames(string word) { }
    public void RemoveFromUserDictionary(string word) { }
    public List<SpellCheckDictionaryDisplay> GetDictionaryLanguages(string dictionaryFolder) => new();

    public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;
    public bool IsWordCorrect(string word) => true;
    public List<string> GetSuggestions(string word) => new();
}
