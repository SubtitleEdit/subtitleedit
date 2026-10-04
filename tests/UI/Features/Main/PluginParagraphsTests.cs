using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Plugins;

namespace UITests.Features.Main;

/// <summary>
/// A binary format such as EBU STL has no text form (Ebu.ToText returns "Not supported!"), so
/// plugins could only see - and return - SubRip, which drops the GSI header and the teletext rows.
/// The request now carries the header and the paragraphs, and a response with paragraphs is
/// applied without a re-parse.
/// </summary>
public class PluginParagraphsTests : IDisposable
{
    private static readonly string GsiHeader = new Ebu.EbuGeneralSubtitleInformation().ToString();

    // Selecting EBU STL forces frame mode through the session-only override (and flips libse's
    // time format); left behind, every later test that parses or compares times runs in frames.
    private readonly SettingsScope _settings = new("General.UseFrameMode");

    public void Dispose()
    {
        _settings.Dispose();
    }

    [AvaloniaFact]
    public void BuildPluginRequest_BinaryFormat_SendsHeaderAndParagraphs()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            vm.SelectedSubtitleFormat = vm.SubtitleFormats.First(f => f is Ebu);
            GetWorkingSubtitle(vm).Header = GsiHeader;
            AddLine(vm, "Hallo", 36_000_000, 36_002_000, "22");

            var request = (PluginRequest)typeof(MainViewModel)
                .GetMethod("BuildPluginRequest", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(vm, new object[] { CreatePlugin(), new List<int>() })!;

            Assert.Equal(string.Empty, request.Subtitle.Native);
            Assert.Equal(GsiHeader, request.Subtitle.Header);
            var paragraph = Assert.Single(request.Subtitle.Paragraphs!);
            Assert.Equal("Hallo", paragraph.Text);
            Assert.Equal("22", paragraph.MarginV);
            Assert.Equal(36_000_000, paragraph.StartMs);
            Assert.NotNull(request.Rules);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public async Task ApplyPluginSubtitle_Paragraphs_KeepRowsAndReplaceHeader()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            vm.SelectedSubtitleFormat = vm.SubtitleFormats.First(f => f is Ebu);
            GetWorkingSubtitle(vm).Header = GsiHeader;
            AddLine(vm, "Hallo", 36_000_000, 36_002_000, "23");

            var newHeader = "850" + GsiHeader.Substring(3);
            var response = new PluginResponse
            {
                Status = PluginConstants.StatusOk,
                Subtitle = new PluginSubtitle
                {
                    Header = newHeader,
                    Paragraphs = new List<PluginParagraph>
                    {
                        new() { StartMs = 36_000_000, EndMs = 36_000_200, Text = string.Empty, MarginV = "22" },
                        new() { StartMs = 36_001_000, EndMs = 36_003_000, Text = "Hallo", MarginV = "22" },
                    },
                },
            };

            var applied = await (Task<bool>)typeof(MainViewModel)
                .GetMethod("ApplyPluginSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(vm, new object[] { CreatePlugin(), response })!;

            Assert.True(applied);
            Assert.IsType<Ebu>(vm.SelectedSubtitleFormat);
            Assert.Equal(new[] { string.Empty, "Hallo" }, vm.Subtitles.Select(p => p.Text));
            Assert.Equal(new[] { "22", "22" }, vm.Subtitles.Select(p => p.MarginV));
            Assert.Equal(newHeader, vm.GetUpdateSubtitle().Header);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static InstalledPlugin CreatePlugin() => new()
    {
        Manifest = new PluginManifest { Name = "Test plugin" },
        FolderPath = string.Empty,
        ManifestPath = string.Empty,
    };

    private static Subtitle GetWorkingSubtitle(MainViewModel vm)
    {
        return (Subtitle)typeof(MainViewModel)
            .GetField("_subtitle", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(vm)!;
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    private static void AddLine(MainViewModel vm, string text, int startMs, int endMs, string marginV)
    {
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, startMs, endMs) { MarginV = marginV }, null!)
        {
            Number = vm.Subtitles.Count + 1,
        });
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        Dispatcher.UIThread.RunJobs();
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
