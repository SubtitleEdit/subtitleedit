using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.FormatProperties.ItunesTimedTextProperties;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Files;

/// <summary>
/// iTunes Timed Text export preset (#16257): the properties window's default color, body
/// defaults, template-file header source and named presets.
/// </summary>
public class ItunesTimedTextPropertiesTests
{
    private const string Template = """
        <?xml version="1.0" encoding="UTF-8"?>
        <tt xmlns="http://www.w3.org/ns/ttml" xmlns:tts="http://www.w3.org/ns/ttml#styling" xmlns:ttp="http://www.w3.org/ns/ttml#parameter" xml:lang="de" ttp:timeBase="smpte" ttp:frameRate="25" ttp:frameRateMultiplier="1 1" ttp:dropMode="nonDrop">
          <head>
            <styling>
              <style tts:fontSize="100%" tts:color="#EBEBEB" tts:fontStyle="normal" tts:fontWeight="normal" tts:fontFamily="sansSerif" xml:id="plain"/>
              <style tts:fontSize="100%" tts:color="#EBEBEB" tts:fontStyle="italic" tts:fontWeight="normal" tts:fontFamily="sansSerif" xml:id="slanted"/>
            </styling>
            <layout>
              <region xml:id="top" tts:origin="0% 0%" tts:extent="100% 15%" tts:textAlign="center" tts:displayAlign="before"/>
              <region xml:id="bottom" tts:origin="0% 85%" tts:extent="100% 15%" tts:textAlign="center" tts:displayAlign="after"/>
            </layout>
          </head>
          <body style="plain" region="bottom"><div/></body>
        </tt>
        """;

    private static ItunesTimedTextPropertiesViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ItunesTimedTextPropertiesViewModel>();
    }

    private static void ResetSettings()
    {
        Se.Settings.Formats = new SeFormats();
        var ss = Configuration.Settings.SubtitleSettings;
        ss.TimedTextItunesDefaultColor = "white";
        ss.TimedTextItunesDefaultStyle = string.Empty;
        ss.TimedTextItunesDefaultRegion = string.Empty;
        ss.TimedTextItunesHeaderTemplate = string.Empty;
        ss.TimedTextItunesLanguage = string.Empty;
    }

    [AvaloniaFact]
    public void Window_OpensWithTheBuiltInHeader()
    {
        ResetSettings();
        var vm = MakeViewModel();
        var subtitle = new Subtitle();
        vm.Initialize(subtitle);
        var window = new ItunesTimedTextPropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.IsCurrentFileMode);
            Assert.False(vm.IsTemplateMode);
            Assert.Equal(["[N/A]", "normal", "bold", "italic"], vm.DefaultStyles);
            Assert.Equal(["[N/A]", "top", "bottom"], vm.DefaultRegions);
            Assert.Equal(Colors.White, vm.DefaultColor);
            Assert.Empty(vm.PresetNames);
        }
        finally
        {
            window.Close();
            ResetSettings();
        }
    }

    [AvaloniaFact]
    public void Ok_WritesColorAndBodyDefaultsIntoHeaderAndSettings()
    {
        ResetSettings();
        var vm = MakeViewModel();
        var subtitle = new Subtitle();
        vm.Initialize(subtitle);
        var window = new ItunesTimedTextPropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            vm.SelectedDefaultStyle = "normal";
            vm.SelectedDefaultRegion = "bottom";
            vm.DefaultColor = Color.FromRgb(0xEB, 0xEB, 0xEB);
            vm.SelectedLanguage = "de";
            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.OkPressed);
            Assert.Contains("style=\"normal\"", subtitle.Header);
            Assert.Contains("region=\"bottom\"", subtitle.Header);
            Assert.Equal(3, subtitle.Header.Split("tts:color=\"#EBEBEB\"").Length - 1);
            Assert.DoesNotContain("tts:color=\"white\"", subtitle.Header);

            var formats = Se.Settings.Formats;
            Assert.Equal("#EBEBEB", formats.TimedTextItunesDefaultColor);
            Assert.Equal("normal", formats.TimedTextItunesDefaultStyle);
            Assert.Equal("bottom", formats.TimedTextItunesDefaultRegion);
            Assert.Equal("de", formats.TimedTextItunesLanguage);
            Assert.Equal(SeItunesTimedTextPreset.HeaderSourceCurrentFile, formats.TimedTextItunesHeaderSource);

            // the libse mirror is current, so the next SRT -> ITT save already uses it
            Assert.Equal("#EBEBEB", Configuration.Settings.SubtitleSettings.TimedTextItunesDefaultColor);
            var raw = new Subtitle { Paragraphs = { new Paragraph("Hi", 0, 1000) } }.ToText(new ItunesTimedText());
            Assert.Contains("<body style=\"normal\" region=\"bottom\">", raw);
            Assert.Contains("tts:color=\"#EBEBEB\"", raw);
        }
        finally
        {
            window.Close();
            ResetSettings();
        }
    }

    [AvaloniaFact]
    public void TemplateFile_ListsItsStylesAndFeedsTheWriter()
    {
        ResetSettings();
        var templateFile = Path.Combine(Path.GetTempPath(), "se-itt-template-" + Guid.NewGuid().ToString("N") + ".itt");
        File.WriteAllText(templateFile, Template);
        var vm = MakeViewModel();
        var subtitle = new Subtitle { Header = new ItunesTimedText().ToText(new Subtitle(), "tt") };
        var headerBefore = subtitle.Header;
        vm.Initialize(subtitle);
        var window = new ItunesTimedTextPropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            vm.SelectedHeaderSource = vm.HeaderSources[1];
            Assert.True(vm.IsTemplateMode);
            Assert.Equal(Se.Language.File.PropertyTemplateFileNotFound, vm.TemplateSummary);

            vm.TemplateFile = templateFile;
            Assert.Equal(["[N/A]", "plain", "slanted"], vm.DefaultStyles);
            Assert.Equal(["[N/A]", "top", "bottom"], vm.DefaultRegions);
            Assert.Contains("de", vm.TemplateSummary);
            Assert.Contains("plain, slanted", vm.TemplateSummary);

            Assert.Equal("[N/A]", vm.SelectedDefaultStyle);
            vm.SelectedDefaultStyle = "slanted";
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("slanted", vm.SelectedDefaultStyle);
            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            // the current file's header is not touched - the template is the header now
            Assert.Equal(headerBefore, subtitle.Header);
            var formats = Se.Settings.Formats;
            Assert.Equal(SeItunesTimedTextPreset.HeaderSourceTemplateFile, formats.TimedTextItunesHeaderSource);
            Assert.Equal(templateFile, formats.TimedTextItunesTemplateFile);
            Assert.Equal("slanted", formats.TimedTextItunesDefaultStyle);
            Assert.Contains("#EBEBEB", Configuration.Settings.SubtitleSettings.TimedTextItunesHeaderTemplate);

            var raw = new Subtitle { Paragraphs = { new Paragraph("Hi", 0, 1000) } }.ToText(new ItunesTimedText());
            Assert.Contains("tts:color=\"#EBEBEB\"", raw);
            Assert.Contains("<body style=\"slanted\" region=\"bottom\">", raw);
            Assert.DoesNotContain("tts:color=\"white\"", raw);

            // back to the current file: its own styles again
            vm.SelectedHeaderSource = vm.HeaderSources[0];
            Assert.Equal(["[N/A]", "normal", "bold", "italic"], vm.DefaultStyles);
        }
        finally
        {
            window.Close();
            ResetSettings();
            File.Delete(templateFile);
        }
    }

    [AvaloniaFact]
    public void SelectingAPreset_AppliesItsValues()
    {
        ResetSettings();
        Se.Settings.Formats.TimedTextItunesPresets.Add(new SeItunesTimedTextPreset
        {
            Name = "Broadcast",
            Language = "fr",
            DefaultColor = "#EBEBEB",
            DefaultStyle = "italic",
            DefaultRegion = "top",
            TimeCodeFormat = "hh:mm:ss.ms",
            StyleAttribute = "style",
            TopOrigin = "0% 5%",
        });
        var vm = MakeViewModel();
        vm.Initialize(new Subtitle());
        var window = new ItunesTimedTextPropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(["Broadcast"], vm.PresetNames);
            Assert.Null(vm.SelectedPresetName);

            vm.SelectedPresetName = "Broadcast";

            Assert.Equal("fr", vm.SelectedLanguage);
            Assert.Equal(Color.FromRgb(0xEB, 0xEB, 0xEB), vm.DefaultColor);
            Assert.Equal("italic", vm.SelectedDefaultStyle);
            Assert.Equal("top", vm.SelectedDefaultRegion);
            Assert.Equal("hh:mm:ss.ms", vm.SelectedTimeCodeFormat);
            Assert.Equal("style", vm.SelectedStyleAttribute);
            Assert.Equal("0% 5%", vm.TopOrigin);
            Assert.True(vm.IsCurrentFileMode);

            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Broadcast", Se.Settings.Formats.TimedTextItunesSelectedPreset);
        }
        finally
        {
            window.Close();
            ResetSettings();
        }
    }
}
