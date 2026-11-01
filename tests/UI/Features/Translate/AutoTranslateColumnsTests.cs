using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Translate;

/// <summary>
/// The lines show source and translation side by side, under the names of their languages.
/// </summary>
public class AutoTranslateColumnsTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [AvaloniaFact]
    public void Columns_AreNumberSourceAndTranslation_NamedAfterTheLanguages()
    {
        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello there.", 0, 1000));
        vm.Initialize(subtitle);
        var window = new AutoTranslateWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        // From the lists: the combo boxes clear a value that is not one of their items
        var source = vm.SourceLanguages.Last();
        var target = vm.TargetLanguages.Last();
        vm.SelectedSourceLanguage = source;
        vm.SelectedTargetLanguage = target;
        Dispatcher.UIThread.RunJobs();

        var columns = vm.RowGrid!.Columns;
        Assert.Equal(3, columns.Count);
        Assert.Equal(source.Name, columns[1].Header);
        Assert.Equal(target.Name, columns[2].Header);
        window.Close();
    }

    [Theory]
    [InlineData(4.1, "00:04")]
    [InlineData(754, "12:34")]
    [InlineData(3723, "1:02:03")]
    public void ShowTime_IsShownToTheSecond(double seconds, string expected)
    {
        Assert.Equal(expected, AutoTranslateWindow.FormatShortTime(TimeSpan.FromSeconds(seconds)));
    }
}
