using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Main;

/// <summary>
/// File > Import > Time codes with the original's non-matching lines shown: the display-only
/// reference rows are not lines of the working subtitle. They used to count as lines (a matching
/// file got the "different number of lines" prompt) and took imported times by grid position, so
/// every working line after one got the time code meant for the line before it.
/// </summary>
public class ImportTimeCodesReferenceRowsTests
{
    [AvaloniaFact]
    public async Task ReferenceRows_AreSkipped()
    {
        var fileName = Path.Combine(Path.GetTempPath(), $"se-import-time-codes-{Guid.NewGuid()}.srt");
        File.WriteAllText(fileName,
            "1\r\n00:00:10,000 --> 00:00:11,000\r\nA\r\n\r\n" +
            "2\r\n00:00:20,000 --> 00:00:21,000\r\nB\r\n\r\n" +
            "3\r\n00:00:30,000 --> 00:00:31,000\r\nC\r\n");

        var (window, vm) = CreateMainViewModel(new PickFileHelper(fileName));
        try
        {
            var one = AddLine(vm, "One", 0, 1000);
            var two = AddLine(vm, "Two", 2000, 3000);
            var reference = AddLine(vm, string.Empty, 4000, 5000);
            reference.IsReferenceOnly = true;
            var three = AddLine(vm, "Three", 6000, 7000);

            var import = vm.ImportTimeCodesCommand.ExecuteAsync(null);
            for (var i = 0; i < 200 && !import.IsCompleted; i++)
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(10);
            }

            Assert.True(import.IsCompleted, "Import time codes did not finish (a prompt was shown?)");
            Assert.Equal(10_000, one.StartTime.TotalMilliseconds);
            Assert.Equal(20_000, two.StartTime.TotalMilliseconds);
            Assert.Equal(30_000, three.StartTime.TotalMilliseconds);
            Assert.Equal(31_000, three.EndTime.TotalMilliseconds);
            Assert.Equal(4_000, reference.StartTime.TotalMilliseconds);
        }
        finally
        {
            foreach (var ownedWindow in window.OwnedWindows.ToArray())
            {
                ownedWindow.Close();
            }

            window.Closing -= vm.OnClosing;
            if (window.IsVisible)
            {
                window.Close();
            }

            File.Delete(fileName);
        }
    }

    private sealed class PickFileHelper : StubFileHelper
    {
        private readonly string _fileName;

        public PickFileHelper(string fileName)
        {
            _fileName = fileName;
        }

        public override Task<string> PickOpenSubtitleFile(Visual sender, string title, bool includeVideoFiles = true, string? lastOpenedFilePath = null, bool includeSpreadsheets = false)
            => Task.FromResult(_fileName);
    }

    private static SubtitleLineViewModel AddLine(MainViewModel vm, string text, int startMs, int endMs)
    {
        var line = new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            Number = vm.Subtitles.Count + 1,
        };

        vm.Subtitles.Add(line);
        return line;
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel(IFileHelper fileHelper)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        services.AddSingleton(fileHelper);
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }
}
