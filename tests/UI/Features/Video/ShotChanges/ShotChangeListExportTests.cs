using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Video.ShotChanges;

namespace UITests.Features.Video.ShotChanges;

public class ShotChangeListExportTests : IDisposable
{
    private readonly string _outputFileName = Path.Combine(Path.GetTempPath(), $"se-shot-change-export-{Guid.NewGuid():N}.shotchanges");

    public void Dispose()
    {
        if (File.Exists(_outputFileName))
        {
            File.Delete(_outputFileName);
        }
    }

    private sealed class SaveFileHelper : StubFileHelper
    {
        private readonly string _result;

        public SaveFileHelper(string result)
        {
            _result = result;
        }

        public string? SuggestedFileName { get; private set; }
        public IReadOnlyList<(string Name, string Extension)>? FileTypes { get; private set; }

        public override Task<string> PickSaveFile(Visual sender, IReadOnlyList<(string Name, string Extension)> fileTypes, string suggestedFileName, string title)
        {
            FileTypes = fileTypes;
            SuggestedFileName = suggestedFileName;
            return Task.FromResult(_result);
        }
    }

    [AvaloniaFact]
    public async Task Export_WritesSecondsPerLineLikeTheShotChangesFiles()
    {
        var fileHelper = new SaveFileHelper(_outputFileName);
        var vm = new ShotChangeListViewModel(fileHelper);
        var videoFileName = Path.Combine(Path.GetTempPath(), "My.Movie.mkv");
        vm.Initialize(new List<double> { 1.5, 12.04, 3600.25 }, videoFileName);
        var window = new ShotChangeListWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        await vm.ExportCommand.ExecuteAsync(null);

        Assert.Equal("1.5\n12.04\n3600.25", File.ReadAllText(_outputFileName).Replace("\r\n", "\n"));
        Assert.Equal(Path.Combine(Path.GetTempPath(), "My.Movie"), fileHelper.SuggestedFileName);
        Assert.Equal(".shotchanges", fileHelper.FileTypes![0].Extension);

        window.Close();
    }

    [AvaloniaFact]
    public async Task Export_Cancelled_WritesNothing()
    {
        var vm = new ShotChangeListViewModel(new SaveFileHelper(string.Empty));
        vm.Initialize(new List<double> { 1.5 });
        var window = new ShotChangeListWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        await vm.ExportCommand.ExecuteAsync(null);

        Assert.False(File.Exists(_outputFileName));

        window.Close();
    }
}
