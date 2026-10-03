using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Regression tests for issue #12839: the ASSA settings window in batch convert always showed
/// the default styles, and edits typed into the source view were discarded on OK.
/// </summary>
public class BatchConvertAssaViewModelTests
{
    private const string CustomStyleName = "BatchStyle42";

    [AvaloniaFact]
    public void SavedHeader_IsShownInSourceView()
    {
        var header = Se.Settings.Tools.BatchConvert.AssaHeader;
        var footer = Se.Settings.Tools.BatchConvert.AssaFooter;
        try
        {
            Se.Settings.Tools.BatchConvert.AssaHeader = MakeHeaderWithStyle(CustomStyleName);
            Se.Settings.Tools.BatchConvert.AssaFooter = string.Empty;

            var viewModel = MakeViewModel();

            Assert.Contains(CustomStyleName, viewModel.Text);
        }
        finally
        {
            Se.Settings.Tools.BatchConvert.AssaHeader = header;
            Se.Settings.Tools.BatchConvert.AssaFooter = footer;
        }
    }

    [AvaloniaFact]
    public void Ok_KeepsEditsMadeInSourceView()
    {
        var header = Se.Settings.Tools.BatchConvert.AssaHeader;
        var footer = Se.Settings.Tools.BatchConvert.AssaFooter;
        try
        {
            Se.Settings.Tools.BatchConvert.AssaHeader = string.Empty;
            Se.Settings.Tools.BatchConvert.AssaFooter = string.Empty;

            var viewModel = MakeViewModel();
            viewModel.Text = viewModel.Text.Replace("Style: Default,", $"Style: {CustomStyleName},");

            viewModel.OkCommand.Execute(null);

            Assert.Contains(CustomStyleName, Se.Settings.Tools.BatchConvert.AssaHeader);
        }
        finally
        {
            Se.Settings.Tools.BatchConvert.AssaHeader = header;
            Se.Settings.Tools.BatchConvert.AssaFooter = footer;
        }
    }

    /// <summary>
    /// Picking a font from "Collected fonts" in the styles dialog embeds it in the dialog's
    /// subtitle footer. Batch convert only took the header back, so the font was dropped.
    /// </summary>
    [AvaloniaFact]
    public async Task EditStyles_Ok_KeepsFontsEmbeddedInTheDialog()
    {
        var header = Se.Settings.Tools.BatchConvert.AssaHeader;
        var footer = Se.Settings.Tools.BatchConvert.AssaFooter;
        try
        {
            Se.Settings.Tools.BatchConvert.AssaHeader = string.Empty;
            Se.Settings.Tools.BatchConvert.AssaFooter = string.Empty;

            var services = new ServiceCollection();
            services.AddSubtitleEditServices();
            using var provider = services.BuildServiceProvider();
            const string embeddedFonts = "[Fonts]\nfontname: test_0.ttf\n!!!!";
            var windowService = new OkStylesDialogWindowService(provider, vm =>
            {
                vm.ResultSubtitle.Footer = embeddedFonts;
                vm.OkCommand.Execute(null);
            });
            var viewModel = new BatchConvertAssaViewModel(windowService) { Window = new Avalonia.Controls.Window() };

            await viewModel.EditStylesCommand.ExecuteAsync(null);
            viewModel.OkCommand.Execute(null);

            Assert.Equal(embeddedFonts, Se.Settings.Tools.BatchConvert.AssaFooter);
        }
        finally
        {
            Se.Settings.Tools.BatchConvert.AssaHeader = header;
            Se.Settings.Tools.BatchConvert.AssaFooter = footer;
        }
    }

    // Opens no window: builds the styles dialog's view model, lets the test act in it, returns it
    private sealed class OkStylesDialogWindowService(IServiceProvider provider, Action<AssaStylesViewModel> act) : IWindowService
    {
        public Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
            Avalonia.Controls.Window owner,
            Action<TViewModel>? configureViewModel = null,
            Action<TWindow>? configureWindow = null)
            where TWindow : Avalonia.Controls.Window where TViewModel : class
        {
            var vm = provider.GetRequiredService<TViewModel>();
            configureViewModel?.Invoke(vm);
            act((vm as AssaStylesViewModel)!);
            return Task.FromResult(vm);
        }

        public T ShowWindow<T>(Avalonia.Controls.Window owner, Action<T>? configure = null) where T : Avalonia.Controls.Window
            => throw new NotSupportedException();

        public TViewModel ShowWindow<T, TViewModel>(Avalonia.Controls.Window owner, Action<T, TViewModel>? configure = null)
            where T : Avalonia.Controls.Window where TViewModel : class
            => throw new NotSupportedException();

        public TViewModel ShowIndependentWindow<T, TViewModel>(Action<T, TViewModel>? configure = null)
            where T : Avalonia.Controls.Window where TViewModel : class
            => throw new NotSupportedException();

        public Task<T> ShowDialogAsync<T>(Avalonia.Controls.Window owner, Action<T>? configure = null) where T : Avalonia.Controls.Window
            => throw new NotSupportedException();

        public Task<TViewModel> ShowWithOwnerHiddenAsync<TWindow, TViewModel>(
            Avalonia.Controls.Window owner,
            IReadOnlyList<Avalonia.Controls.Window?> companions,
            Action<TViewModel>? configureViewModel = null)
            where TWindow : Avalonia.Controls.Window where TViewModel : class
            => throw new NotSupportedException();
    }

    /// <summary>
    /// SE 4 applied the pasted header by default; SE 5 defaults to "Use source styles", which
    /// silently ignored the header for .ass input (Reddit report). Editing the header means it
    /// should be used, so the edit unticks the checkbox - and ticking it back sticks.
    /// </summary>
    [AvaloniaFact]
    public void EditingHeader_UnticksUseSourceStyles_UntilNextEdit()
    {
        var header = Se.Settings.Tools.BatchConvert.AssaHeader;
        var footer = Se.Settings.Tools.BatchConvert.AssaFooter;
        var useSource = Se.Settings.Tools.BatchConvert.AssaUseSourceStylesIfPossible;
        try
        {
            Se.Settings.Tools.BatchConvert.AssaHeader = string.Empty;
            Se.Settings.Tools.BatchConvert.AssaFooter = string.Empty;
            Se.Settings.Tools.BatchConvert.AssaUseSourceStylesIfPossible = true;

            var viewModel = MakeViewModel();
            Assert.True(viewModel.UseSourceStylesIfPossible);
            Assert.False(viewModel.KeepSourceEmbeddedFontsIsEnabled);

            // Re-setting the same text (as the source editor does when it is created) is no edit
            viewModel.Text = viewModel.Text.Replace("\r\n", "\n");
            Assert.True(viewModel.UseSourceStylesIfPossible);

            viewModel.Text = viewModel.Text.Replace("Style: Default,Arial", "Style: Default,Trebuchet MS");
            Assert.False(viewModel.UseSourceStylesIfPossible);
            Assert.True(viewModel.KeepSourceEmbeddedFontsIsEnabled);

            viewModel.UseSourceStylesIfPossible = true;
            viewModel.Text = viewModel.Text + "\n";
            Assert.True(viewModel.UseSourceStylesIfPossible);
        }
        finally
        {
            Se.Settings.Tools.BatchConvert.AssaHeader = header;
            Se.Settings.Tools.BatchConvert.AssaFooter = footer;
            Se.Settings.Tools.BatchConvert.AssaUseSourceStylesIfPossible = useSource;
        }
    }

    private static BatchConvertAssaViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<BatchConvertAssaViewModel>();
    }

    private static string MakeHeaderWithStyle(string styleName)
    {
        var format = new AdvancedSubStationAlpha();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Sample subtitle", 0, 2000));
        var text = format.ToText(subtitle, string.Empty);
        format.LoadSubtitle(subtitle, text.SplitToLines(), string.Empty);
        return subtitle.Header.Replace("Style: Default,", $"Style: {styleName},");
    }
}
