using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr.CrispEmbedSettings;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Video.VideoOcr;

/// <summary>
/// The Video OCR window builds its settings column in code, so the CrispEmbed gear button that
/// replaced the two small download icon buttons is only proven to exist once the window is
/// constructed. These tests assert it reaches the logical tree, follows the engine selection,
/// and that the CrispEmbed dialog it opens lists every downloadable model.
/// </summary>
public class VideoOcrCrispEmbedSettingsButtonTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: an unclosed window would
    // outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private VideoOcrWindow BuildWindow(out VideoOcrViewModel viewModel)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        viewModel = provider.GetRequiredService<VideoOcrViewModel>();
        var window = new VideoOcrWindow(viewModel);
        _windows.Add(window);
        return window;
    }

    private static Button? FindButton(Control root, string automationName)
    {
        return root.GetLogicalDescendants().OfType<Button>()
            .FirstOrDefault(b => AutomationProperties.GetName(b) == automationName);
    }

    private static string SettingsButtonName =>
        $"{CrispEmbedEngine.StaticName} - {Se.Language.General.Settings}";

    [AvaloniaFact]
    public void Window_HasCrispEmbedSettingsButton_HiddenUntilCrispEmbedIsSelected()
    {
        if (!CrispEmbedEngine.CanBeDownloaded())
        {
            return;
        }

        var window = BuildWindow(out var viewModel);
        try
        {
            viewModel.SelectedEngine = viewModel.Engines.First(p => p.EngineType != OcrEngineType.CrispEmbed);

            var button = FindButton(window, SettingsButtonName);
            Assert.NotNull(button);
            Assert.False(button!.IsVisible);

            viewModel.SelectedEngine = viewModel.Engines.First(p => p.EngineType == OcrEngineType.CrispEmbed);

            Assert.True(button.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// The gear button replaced the model download and engine re-download icon buttons - having
    /// both would put three download routes next to each other again.
    /// </summary>
    [AvaloniaFact]
    public void Window_HasNoCrispEmbedDownloadIconButtons()
    {
        if (!CrispEmbedEngine.CanBeDownloaded())
        {
            return;
        }

        var window = BuildWindow(out var viewModel);
        try
        {
            viewModel.SelectedEngine = viewModel.Engines.First(p => p.EngineType == OcrEngineType.CrispEmbed);

            Assert.Null(FindButton(window,
                string.Format(Se.Language.General.ReDownloadX, CrispEmbedEngine.StaticName)));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SettingsWindow_ListsEveryBackendModel_WithADownloadButton()
    {
        var viewModel = new CrispEmbedSettingsViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        viewModel.Initialize();
        var window = new CrispEmbedSettingsWindow(viewModel);
        _windows.Add(window);
        try
        {
            var expected = CrispEmbedEngine.GetBackends().Sum(b => b.Models.Count);

            Assert.Equal(expected, viewModel.Models.Count);
            Assert.All(viewModel.Models, m =>
            {
                Assert.False(string.IsNullOrEmpty(m.BackendName));
                Assert.False(string.IsNullOrEmpty(m.ModelName));
                Assert.False(string.IsNullOrEmpty(m.StatusLabel));
                Assert.False(string.IsNullOrEmpty(m.DownloadButtonText));
                Assert.NotNull(m.DownloadCommand);
            });

            // Engine status and the install folder are the reason the dialog exists - a silent
            // binding failure would leave them blank.
            Assert.False(string.IsNullOrEmpty(viewModel.EngineLabel));
            Assert.False(string.IsNullOrEmpty(viewModel.EngineDownloadButtonText));
            Assert.False(string.IsNullOrEmpty(viewModel.InstallFolder));

            window.Show();

            // InitializeWindow posts its clamp-to-working-area callback from Opened; flush it
            // while the window is still alive so it cannot run against a disposed platform
            // implementation during session teardown.
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            Assert.NotNull(window.Content);
        }
        finally
        {
            window.Close();
        }
    }
}
