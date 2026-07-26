using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
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
