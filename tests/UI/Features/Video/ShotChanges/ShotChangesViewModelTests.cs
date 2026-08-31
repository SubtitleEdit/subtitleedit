using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Video.ShotChanges;

namespace UITests.Features.Video.ShotChanges;

public class ShotChangesViewModelTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly List<string> _files = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();

        foreach (var file in _files.Where(File.Exists))
        {
            File.Delete(file);
        }
    }

    private ShotChangesViewModel ShowWindow()
    {
        var vm = new ShotChangesViewModel();
        var window = new ShotChangesWindow(vm);
        _windows.Add(window);
        window.Show();
        vm.Initialize(string.Empty);
        Dispatcher.UIThread.RunJobs();
        return vm;
    }

    private string WriteTempFile(string extension, string content)
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + extension);
        File.WriteAllText(fileName, content);
        _files.Add(fileName);
        return fileName;
    }

    [AvaloniaFact]
    public void SecondsWithDecimalCommaAreImported()
    {
        var vm = ShowWindow();
        vm.TimeCodeSeconds = true;
        vm.ImportText = "1,5" + Environment.NewLine + "2,25";

        vm.OkCommand.Execute(null);

        Assert.Equal(new[] { 1.5, 2.25 }, vm.FfmpegLines.Select(p => p.Seconds));
    }

    [AvaloniaFact]
    public void MatroskaChapterFileKeepsSubSecondPrecision()
    {
        // 00:00:12.480 - the old writer emitted frames in the last field (":12"), which the
        // hours:minutes:seconds:milliseconds parser then read back as 12 milliseconds.
        var fileName = WriteTempFile(".xml",
            "<Chapters><EditionEntry><ChapterAtom>" +
            "<ChapterTimeStart>00:00:12.480000000</ChapterTimeStart>" +
            "</ChapterAtom></EditionEntry></Chapters>");

        var vm = ShowWindow();
        vm.LoadTextFile(fileName).GetAwaiter().GetResult();
        Dispatcher.UIThread.RunJobs();

        vm.OkCommand.Execute(null);

        var item = Assert.Single(vm.FfmpegLines);
        Assert.Equal(12.48, item.Seconds, 3);
    }

    [AvaloniaFact]
    public void JsonShotChangesFileIsImported()
    {
        var fileName = WriteTempFile(".json",
            "[{\"frame_time\": 1.5}, {\"frame_time\": 12.48}]");

        var vm = ShowWindow();
        vm.LoadTextFile(fileName).GetAwaiter().GetResult();
        Dispatcher.UIThread.RunJobs();

        vm.OkCommand.Execute(null);

        Assert.Equal(new[] { 1.5, 12.48 }, vm.FfmpegLines.Select(p => p.Seconds));
    }
}
