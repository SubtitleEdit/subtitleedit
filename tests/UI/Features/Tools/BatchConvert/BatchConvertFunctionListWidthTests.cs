using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// The function list was a fixed 360 px, which fits the English names but cut Russian, Bulgarian
/// and French ones off with an ellipsis (#15901). It now widens to the longest name.
/// </summary>
public class BatchConvertFunctionListWidthTests
{
    [AvaloniaFact]
    public void LongTranslatedFunctionName_IsNotTrimmed()
    {
        // The first row, so it is realized without scrolling. Bulgarian "Adjust image brightness/alpha/color".
        const string longName = "Регулиране на яркостта/алфа/цвята на изображението";
        var original = Se.Language.General.DeleteLines;
        Se.Language.General.DeleteLines = longName;
        try
        {
            var services = new ServiceCollection();
            services.AddSubtitleEditServices();
            var vm = services.BuildServiceProvider().GetRequiredService<BatchConvertViewModel>();
            var window = new BatchConvertWindow(vm) { Width = 1024, Height = 740 };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var cell = window.GetVisualDescendants()
                .OfType<TextBlock>()
                .First(t => t.Text == longName && t.FindAncestorOfType<TableView>() != null);
            var probe = new TextBlock { Text = longName, FontSize = cell.FontSize, FontFamily = cell.FontFamily };
            probe.Measure(Size.Infinity);

            Assert.True(probe.DesiredSize.Width <= cell.Bounds.Width,
                $"needs {probe.DesiredSize.Width:0} px, cell is {cell.Bounds.Width:0} px");

            window.Close();
        }
        finally
        {
            Se.Language.General.DeleteLines = original;
        }
    }
}
