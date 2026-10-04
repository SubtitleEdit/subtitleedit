using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.ExportPac;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Batch convert writes the same files as the main window's Save/Export: the PAC code page, the
/// Cavena 890 header fields and the DVB Teletext page come from settings instead of hard-coded
/// defaults or leftovers from the last main-window export, the binary formats the main window
/// exports are selectable, and "Force CR+LF on save" is honoured.
/// </summary>
public class BatchConverterExportParityTests
{
    public BatchConverterExportParityTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private const string InputSrt = "1\n00:00:01,000 --> 00:00:03,000\nΓειά σου κόσμε.\n\n2\n00:00:04,000 --> 00:00:06,000\nSecond line.\n";

    private static async Task<byte[]> ConvertAsync(BatchConvertConfig config, string extension)
    {
        var dir = Directory.CreateTempSubdirectory("se-batch-parity-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "movie.srt");
            await File.WriteAllTextAsync(inputFile, InputSrt, TestContext.Current.CancellationToken);

            config.SaveInSourceFolder = true;
            config.Overwrite = true;
            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(config);

            var item = new BatchConvertItem(inputFile, 1, new SubRip().Name, LoadInput(inputFile));
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var outputFile = Path.Combine(dir.FullName, "movie" + extension);
            Assert.True(File.Exists(outputFile), $"no {extension} written, status: {item.Status}");
            return await File.ReadAllBytesAsync(outputFile, TestContext.Current.CancellationToken);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    private static Subtitle LoadInput(string fileName)
    {
        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, InputSrt.SplitToLines(), fileName);
        return subtitle;
    }

    private static byte[] SaveDirect(IBinaryPersistableSubtitle format, string fileName)
    {
        using var ms = new MemoryStream();
        format.Save(fileName, ms, LoadInput(fileName), true);
        return ms.ToArray();
    }

    [Fact]
    public async Task Pac_UsesConfiguredCodePage()
    {
        var bytes = await ConvertAsync(new BatchConvertConfig
        {
            TargetFormatName = BatchConverter.FormatPac,
            PacCodePage = Pac.CodePageGreek,
        }, ".pac");

        var expected = SaveDirect(new Pac { CodePage = Pac.CodePageGreek }, "movie.pac");
        var latin = SaveDirect(new Pac { CodePage = Pac.CodePageLatin }, "movie.pac");
        Assert.Equal(expected, bytes);
        Assert.NotEqual(latin, bytes);
    }

    [Theory]
    [InlineData("PAC Unicode (UniPac)", ".fpc")]
    [InlineData(CheetahCaption.NameOfFormat, ".cap")]
    [InlineData(CheetahCaptionOld.NameOfFormat, ".cap")]
    [InlineData(CapMakerPlus.NameOfFormat, ".cap")]
    public async Task NewBinaryTargets_WriteBinaryFile(string formatName, string extension)
    {
        var bytes = await ConvertAsync(new BatchConvertConfig { TargetFormatName = formatName }, extension);
        Assert.True(bytes.Length > 0);
        Assert.NotEqual("Not supported!", Encoding.ASCII.GetString(bytes).Trim());
    }

    [Fact]
    public async Task Cavena890_UsesBatchSettingsAndRestoresMainWindowValues()
    {
        var ss = Configuration.Settings.SubtitleSettings;
        var oldTitle = ss.CurrentCavena89Title;
        var oldTranslator = ss.CurrentCavena890Translator;
        try
        {
            ss.CurrentCavena89Title = "MAIN WINDOW TITLE";
            ss.CurrentCavena890Translator = "MAIN TRANSLATOR";

            var bytes = await ConvertAsync(new BatchConvertConfig
            {
                TargetFormatName = BatchConverter.FormatCavena890,
                Cavena890TranslatedTitle = "BATCH TITLE",
                Cavena890Translator = string.Empty,
            }, ".890");

            var ascii = Encoding.ASCII.GetString(bytes);
            Assert.Contains("BATCH TITLE", ascii);
            Assert.DoesNotContain("MAIN TRANSLATOR", ascii);
            Assert.Equal("MAIN WINDOW TITLE", ss.CurrentCavena89Title);
            Assert.Equal("MAIN TRANSLATOR", ss.CurrentCavena890Translator);
        }
        finally
        {
            ss.CurrentCavena89Title = oldTitle;
            ss.CurrentCavena890Translator = oldTranslator;
        }
    }

    [Fact]
    public async Task DvbTeletext_UsesConfiguredPage()
    {
        var bytes = await ConvertAsync(new BatchConvertConfig
        {
            TargetFormatName = DvbTeletext.NameOfFormat,
            DvbTeletextPageNumber = 777,
            DvbTeletextLanguageCode = "dan",
        }, ".dvbttx");

        var expected = SaveDirect(new DvbTeletext { PageNumber = 777, LanguageCode = "dan" }, "movie.dvbttx");
        var defaults = SaveDirect(new DvbTeletext(), "movie.dvbttx");
        Assert.Equal(expected, bytes);
        Assert.NotEqual(defaults, bytes);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TextFormat_HonoursForceCrLf(bool forceCrLf)
    {
        var bytes = await ConvertAsync(new BatchConvertConfig
        {
            TargetFormatName = new SubRip().Name,
            TargetEncoding = TextEncoding.Utf8WithoutBom,
            ForceCrLf = forceCrLf,
        }, ".srt");

        var text = Encoding.UTF8.GetString(bytes);
        var loneLf = text.Replace("\r\n", string.Empty).Contains('\n');
        if (forceCrLf)
        {
            Assert.Contains("\r\n", text);
            Assert.False(loneLf, "LF without CR in output");
        }
        else
        {
            Assert.Equal(Environment.NewLine == "\n", loneLf);
        }
    }

    [Fact]
    public void ExportPac_RemembersLastCodePage()
    {
        using var _ = new SettingsScope("File.ExportPacCodePage", "File.ExportPacSecondaryCodePage");
        Se.Settings.File.ExportPacCodePage = Pac.CodePageLatin;
        Se.Settings.File.ExportPacSecondaryCodePage = -1;

        var vm = new ExportPacViewModel();
        Assert.Equal("Latin", vm.SelectedPacCodePage);
        vm.SelectedPacCodePage = "Greek";
        vm.SelectedSecondaryPacCodePage = "Cyrillic";
        vm.OkCommand.Execute(null);

        Assert.Equal(Pac.CodePageGreek, Se.Settings.File.ExportPacCodePage);
        Assert.Equal(Pac.CodePageCyrillic, Se.Settings.File.ExportPacSecondaryCodePage);

        var reopened = new ExportPacViewModel();
        Assert.Equal("Greek", reopened.SelectedPacCodePage);
        Assert.Equal("Cyrillic", reopened.SelectedSecondaryPacCodePage);
    }
}
