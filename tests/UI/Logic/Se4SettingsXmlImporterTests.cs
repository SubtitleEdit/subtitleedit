using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Se4Setup;

namespace UITests.Logic;

/// <summary>
/// The Settings import dialog only took an SE 5 Settings.json, which a user coming from SE 4 does
/// not have (#14309). These cover the SE 4 Settings.xml path: the sections it finds decide which
/// categories the dialog offers, and each category is mapped field by field - anything SE 4 has no
/// counterpart for has to keep its current value rather than fall back to an SE 5 default.
/// </summary>
public class Se4SettingsXmlImporterTests : IDisposable
{
    private readonly SeGeneral _originalGeneral = Se.Settings.General;
    private readonly SeWaveform _originalWaveform = Se.Settings.Waveform;
    private readonly SeAppearance _originalAppearance = Se.Settings.Appearance;
    private readonly SeAutoTranslate _originalAutoTranslate = Se.Settings.AutoTranslate;

    public Se4SettingsXmlImporterTests()
    {
        // Fresh sections so a mapped value is provably the import's doing, and so the real
        // settings of whoever runs the suite are never touched.
        Se.Settings.General = new SeGeneral();
        Se.Settings.Waveform = new SeWaveform();
        Se.Settings.Appearance = new SeAppearance();
        Se.Settings.AutoTranslate = new SeAutoTranslate();
    }

    public void Dispose()
    {
        Se.Settings.General = _originalGeneral;
        Se.Settings.Waveform = _originalWaveform;
        Se.Settings.Appearance = _originalAppearance;
        Se.Settings.AutoTranslate = _originalAutoTranslate;
    }

    private static Se4SettingsXmlImporter.Se4SettingsFile Parse(string xml)
    {
        var file = Se4SettingsXmlImporter.Parse(xml);
        Assert.NotNull(file);
        return file!;
    }

    [Fact]
    public void LooksLikeXml_AcceptsAnSe4FileAndRejectsJson()
    {
        Assert.True(Se4SettingsXmlImporter.LooksLikeXml("<?xml version=\"1.0\"?><Settings />"));
        Assert.True(Se4SettingsXmlImporter.LooksLikeXml("﻿\r\n  <Settings />"));
        Assert.False(Se4SettingsXmlImporter.LooksLikeXml("{ \"General\": {} }"));
    }

    [Fact]
    public void Parse_RejectsAnythingThatIsNotAnSe4SettingsFile()
    {
        Assert.Null(Se4SettingsXmlImporter.Parse("<Settings><General>"));
        Assert.Null(Se4SettingsXmlImporter.Parse("<MultipleSearchAndReplaceList><Item /></MultipleSearchAndReplaceList>"));

        // Right root, but nothing the dialog can offer.
        Assert.Null(Se4SettingsXmlImporter.Parse("<Settings><SsaStyle><FontName>Arial</FontName></SsaStyle></Settings>"));
    }

    [Fact]
    public void Parse_ReportsWhichCategoriesTheFileCanFill()
    {
        var file = Parse("<Settings><General><MaxNumberOfLines>2</MaxNumberOfLines></General></Settings>");

        Assert.True(file.HasRules);
        Assert.True(file.HasAppearance);
        Assert.True(file.HasSyntaxColoring);
        Assert.False(file.HasWaveform);
        Assert.False(file.HasAutoTranslate);
        Assert.False(file.HasShortcuts);
    }

    // An empty <Tools /> is not an auto-translate section - offering the checkbox for it would let
    // the user "import" nothing at all.
    [Fact]
    public void Parse_TreatsAnEmptySectionAsAbsent()
    {
        var file = Parse("<Settings><General><MaxNumberOfLines>2</MaxNumberOfLines></General><Tools /></Settings>");

        Assert.False(file.HasAutoTranslate);
    }

    [Fact]
    public void ApplyRules_MapsTheSe4RuleValues()
    {
        var file = Parse(
            "<Settings><General>" +
            "<SubtitleLineMaximumLength>37</SubtitleLineMaximumLength>" +
            "<MaxNumberOfLines>3</MaxNumberOfLines>" +
            "<MergeLinesShorterThan>34</MergeLinesShorterThan>" +
            "<SubtitleMinimumDisplayMilliseconds>1200</SubtitleMinimumDisplayMilliseconds>" +
            "<SubtitleMaximumDisplayMilliseconds>7000</SubtitleMaximumDisplayMilliseconds>" +
            "<MinimumMillisecondsBetweenLines>84</MinimumMillisecondsBetweenLines>" +
            "<SubtitleMaximumCharactersPerSeconds>17.5</SubtitleMaximumCharactersPerSeconds>" +
            "<SubtitleMaximumWordsPerMinute>190</SubtitleMaximumWordsPerMinute>" +
            "<DialogStyle>DashSecondLineWithoutSpace</DialogStyle>" +
            "<DefaultFrameRate>25</DefaultFrameRate>" +
            "</General></Settings>");

        var frames = Se.Settings.General.MinimumBetweenLines.Frames;
        Se4SettingsXmlImporter.ApplyRules(file);

        var g = Se.Settings.General;
        Assert.Equal(37, g.SubtitleLineMaximumLength);
        Assert.Equal(3, g.MaxNumberOfLines);
        Assert.Equal(34, g.UnbreakLinesShorterThan);
        Assert.Equal(1200, g.SubtitleMinimumDisplayMilliseconds);
        Assert.Equal(7000, g.SubtitleMaximumDisplayMilliseconds);
        Assert.Equal(84, g.MinimumBetweenLines.Milliseconds);
        Assert.Equal(17.5, g.SubtitleMaximumCharactersPerSeconds);
        Assert.Equal(190, g.SubtitleMaximumWordsPerMinute);
        Assert.Equal("DashSecondLineWithoutSpace", g.DialogStyle);
        Assert.Equal(25, g.DefaultFrameRate);

        // SE 4 has no frame count for the minimum gap, so SE 5's must survive the import.
        Assert.Equal(frames, g.MinimumBetweenLines.Frames);
    }

    // SeGeneral.ToProfile does Enum.Parse on these, so a value SE 5 no longer knows would throw
    // every time the rules profile is read - keep the current one instead.
    [Fact]
    public void ApplyRules_IgnoresADialogStyleSe5DoesNotKnow()
    {
        var file = Parse("<Settings><General><DialogStyle>SomethingElse</DialogStyle></General></Settings>");
        var before = Se.Settings.General.DialogStyle;

        Se4SettingsXmlImporter.ApplyRules(file);

        Assert.Equal(before, Se.Settings.General.DialogStyle);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("23976")] // SE 4 could read back a locale-mangled "23,976" as this
    [InlineData("not a number")]
    public void ApplyRules_IgnoresAnOutOfRangeFrameRate(string value)
    {
        var file = Parse($"<Settings><General><DefaultFrameRate>{value}</DefaultFrameRate></General></Settings>");
        var before = Se.Settings.General.DefaultFrameRate;

        Se4SettingsXmlImporter.ApplyRules(file);

        Assert.Equal(before, Se.Settings.General.DefaultFrameRate);
    }

    [Fact]
    public void ApplySyntaxColoring_MapsTheToolsTogglesAndTheErrorColor()
    {
        var file = Parse(
            "<Settings>" +
            "<General><SubtitleLineMaximumPixelWidth>640</SubtitleLineMaximumPixelWidth>" +
            "<MeasureFontName>Verdana</MeasureFontName><MeasureFontSize>30</MeasureFontSize></General>" +
            "<Tools>" +
            "<ListViewSyntaxColorDurationSmall>True</ListViewSyntaxColorDurationSmall>" +
            "<ListViewSyntaxColorLongLines>False</ListViewSyntaxColorLongLines>" +
            "<ListViewSyntaxColorGap>True</ListViewSyntaxColorGap>" +
            // System.Drawing's Color.ToArgb for the SE 4 default (255, 180, 150).
            "<ListViewSyntaxErrorColor>-19306</ListViewSyntaxErrorColor>" +
            "</Tools></Settings>");

        Se4SettingsXmlImporter.ApplySyntaxColoring(file);

        var g = Se.Settings.General;
        Assert.True(g.ColorDurationTooShort);
        Assert.False(g.ColorTextTooLong);
        Assert.True(g.ColorGapTooShort);
        Assert.Equal("#FFFFB496", g.ErrorColor.ToUpperInvariant());
        Assert.Equal(640, g.ColorTextTooWidePixels);
        Assert.Equal("Verdana", g.ColorTextTooWideFontName);
        Assert.Equal(30, g.ColorTextTooWideFontSize);
    }

    [Fact]
    public void ApplyWaveform_MapsColorsAndToggles()
    {
        var file = Parse(
            "<Settings><VideoControls>" +
            "<WaveformColor>-16711936</WaveformColor>" +      // Lime
            "<WaveformBackgroundColor>-16777216</WaveformBackgroundColor>" + // Black
            "<WaveformDrawGrid>True</WaveformDrawGrid>" +
            "<WaveformTextSize>14</WaveformTextSize>" +
            "<WaveformTextBold>True</WaveformTextBold>" +
            "</VideoControls></Settings>");

        var shotChangeColor = Se.Settings.Waveform.WaveformShotChangeColor;
        Se4SettingsXmlImporter.ApplyWaveform(file);

        var w = Se.Settings.Waveform;
        Assert.Equal("#FF00FF00", w.WaveformColor.ToUpperInvariant());
        Assert.Equal("#FF000000", w.WaveformBackgroundColor.ToUpperInvariant());
        Assert.True(w.DrawGridLines);
        Assert.Equal(14, w.WaveformTextFontSize);
        Assert.True(w.WaveformTextFontBold);

        // SE 4 has no shot-change color; SE 5's must not be reset by the import.
        Assert.Equal(shotChangeColor, w.WaveformShotChangeColor);
    }

    [Fact]
    public void ApplyAppearance_MapsThemeFontsAndToolbarButtons()
    {
        var file = Parse(
            "<Settings><General>" +
            "<UseDarkTheme>True</UseDarkTheme>" +
            "<SubtitleFontName>Consolas</SubtitleFontName>" +
            "<SubtitleTextBoxFontSize>13</SubtitleTextBoxFontSize>" +
            "<SubtitleListViewFontSize>11</SubtitleListViewFontSize>" +
            "<ShowToolbarNew>False</ShowToolbarNew>" +
            "<ShowToolbarVisualSync>True</ShowToolbarVisualSync>" +
            "</General></Settings>");

        Se4SettingsXmlImporter.ApplyAppearance(file);

        var a = Se.Settings.Appearance;
        Assert.Equal(UiTheme.ThemeNameDark, a.Theme);
        Assert.Equal("Consolas", a.SubtitleTextBoxAndGridFontName);
        Assert.Equal(13, a.SubtitleTextBoxFontSize);
        Assert.Equal(11, a.SubtitleGridFontSize);
        Assert.False(a.ToolbarShowFileNew);
        Assert.True(a.ToolbarShowVisualSync);
    }

    [Fact]
    public void ApplyAppearance_UsesTheLightThemeWhenSe4WasNotDark()
    {
        var file = Parse("<Settings><General><UseDarkTheme>False</UseDarkTheme></General></Settings>");

        Se4SettingsXmlImporter.ApplyAppearance(file);

        Assert.Equal(UiTheme.ThemeNameLight, Se.Settings.Appearance.Theme);
    }

    // SE 4 prefixed several of these with "AutoTranslate" and SE 5 does not, so the renamed ones
    // are the ones worth pinning down.
    [Fact]
    public void ApplyAutoTranslate_MapsKeysUrlsAndModels()
    {
        var file = Parse(
            "<Settings><Tools>" +
            "<AutoTranslateDeepLApiKey>deepl-key</AutoTranslateDeepLApiKey>" +
            "<AutoTranslateDeepLUrl>https://api.deepl.com/</AutoTranslateDeepLUrl>" +
            "<AutoTranslateLibreUrl>http://localhost:5000/</AutoTranslateLibreUrl>" +
            "<OllamaApiUrl>http://localhost:11434/api/generate</OllamaApiUrl>" +
            "<ChatGptApiKey>chatgpt-key</ChatGptApiKey>" +
            "<GeminiProApiKey>gemini-key</GeminiProApiKey>" +
            "<AutoTranslateDelaySeconds>3</AutoTranslateDelaySeconds>" +
            "</Tools></Settings>");

        Se4SettingsXmlImporter.ApplyAutoTranslate(file);

        var t = Se.Settings.AutoTranslate;
        Assert.Equal("deepl-key", t.DeepLApiKey);
        Assert.Equal("https://api.deepl.com/", t.DeepLUrl);
        Assert.Equal("http://localhost:5000/", t.LibreTranslateUrl);
        Assert.Equal("http://localhost:11434/api/generate", t.OllamaUrl);
        Assert.Equal("chatgpt-key", t.ChatGptApiKey);
        Assert.Equal("gemini-key", t.GeminiProApiKey);
        Assert.Equal(3, t.RequestDelaySeconds);
    }

    // An empty element in SE 4's file means "not set"; writing it through would wipe a key the
    // user has in SE 5.
    [Fact]
    public void ApplyAutoTranslate_KeepsTheCurrentValueForAnEmptySe4Value()
    {
        Se.Settings.AutoTranslate.ChatGptApiKey = "keep-me";
        var file = Parse("<Settings><Tools><ChatGptApiKey></ChatGptApiKey></Tools></Settings>");

        Se4SettingsXmlImporter.ApplyAutoTranslate(file);

        Assert.Equal("keep-me", Se.Settings.AutoTranslate.ChatGptApiKey);
    }
}
