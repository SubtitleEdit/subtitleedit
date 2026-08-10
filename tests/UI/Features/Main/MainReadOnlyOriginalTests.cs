using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// An original subtitle that does not line up 1:1 with the working subtitle is opened as a read-only
/// reference: it is shown side by side, but it is never counted as changed and never written back.
/// Before this, the mismatching lines were dropped and the truncated result was saved over the user's
/// file (issue #13449).
/// </summary>
public class MainReadOnlyOriginalTests
{
    [AvaloniaFact]
    public void ReadOnlyOriginal_ChangedText_IsNotReportedAsChanges()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var line = AddLine(vm, "Translated text", "Reference text");
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = true;
            SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());
            SetPrivateField(vm, "_changeSubtitleHashOriginal", vm.GetFastHashOriginal());

            line.OriginalText = "Changed reference text";

            Assert.False(vm.HasChanges());
            Assert.False(vm.CanEditOriginal);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void EditableOriginal_ChangedText_IsStillReportedAsChanges()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var line = AddLine(vm, "Translated text", "Original text");
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = false;
            SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());
            SetPrivateField(vm, "_changeSubtitleHashOriginal", vm.GetFastHashOriginal());

            line.OriginalText = "Changed original text";

            Assert.True(vm.HasChanges());
            Assert.True(vm.CanEditOriginal);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public async Task ReadOnlyOriginal_SaveSubtitleOriginal_LeavesTheFileUntouched()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = Path.Combine(Path.GetTempPath(), $"se-readonly-original-{Guid.NewGuid():N}.srt");
        const string onDisk = "1\r\n00:00:00,000 --> 00:00:02,000\r\nReference line one\r\n\r\n" +
                              "2\r\n00:00:02,000 --> 00:00:04,000\r\nReference line two\r\n\r\n";
        await File.WriteAllTextAsync(fileName, onDisk);

        try
        {
            AddLine(vm, "Translated text", "Reference line one");
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = true;
            SetPrivateField(vm, "_subtitleFileNameOriginal", fileName);

            var saved = await InvokeSaveSubtitleOriginal(vm);

            Assert.False(saved);
            Assert.Equal(onDisk, await File.ReadAllTextAsync(fileName));
        }
        finally
        {
            CloseWindow(window, vm);
            File.Delete(fileName);
        }
    }

    [AvaloniaFact]
    public void ImportAsReadOnlyReference_KeepsEveryOriginalLineInMemory()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            // Working subtitle: two lines. Reference: the same two plus a line the translation
            // never got - the case from issue #13449.
            AddLine(vm, "Translated one", string.Empty, 0, 2000);
            AddLine(vm, "Translated two", string.Empty, 4000, 6000);

            var reference = new Subtitle();
            reference.Paragraphs.Add(new Paragraph("Reference one", 0, 2000));
            reference.Paragraphs.Add(new Paragraph("Reference only - no translation", 2000, 4000));
            reference.Paragraphs.Add(new Paragraph("Reference two", 4000, 6000));

            var projection = new Subtitle();
            projection.Paragraphs.Add(new Paragraph("Reference one", 0, 2000));
            projection.Paragraphs.Add(new Paragraph("Reference two", 4000, 6000));

            InvokeImportOriginalSubtitle(vm, "reference.srt", reference, projection, isReadOnly: true);

            Assert.True(vm.IsOriginalReadOnly);
            Assert.False(vm.CanEditOriginal);

            // The rows show the time-matched projection...
            Assert.Equal("Reference one", vm.Subtitles[0].OriginalText);
            Assert.Equal("Reference two", vm.Subtitles[1].OriginalText);

            // ...while the reference itself keeps all three lines, so nothing is lost.
            var subtitleOriginal = (Subtitle)GetField("_subtitleOriginal").GetValue(vm)!;
            Assert.Equal(3, subtitleOriginal.Paragraphs.Count);
            Assert.Equal("Reference only - no translation", subtitleOriginal.Paragraphs[1].Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
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

    private static SubtitleLineViewModel AddLine(
        MainViewModel vm, string text, string originalText, int startMs = 0, int endMs = 2000)
    {
        var line = new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            OriginalText = originalText,
        };

        vm.Subtitles.Add(line);
        return line;
    }

    private static void CloseWindow(Window window, MainViewModel vm)
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
    }

    private static async Task<bool> InvokeSaveSubtitleOriginal(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "SaveSubtitleOriginal", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("SaveSubtitleOriginal not found");

        return await (Task<bool>)method.Invoke(vm, new object[] { true })!;
    }

    private static void InvokeImportOriginalSubtitle(
        MainViewModel vm, string fileName, Subtitle subtitle, Subtitle displayOriginal, bool isReadOnly)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ImportOriginalSubtitle not found");

        method.Invoke(vm, new object?[] { 0, fileName, subtitle, displayOriginal, isReadOnly });
    }

    private static void SetPrivateField(MainViewModel vm, string name, object value)
    {
        GetField(name).SetValue(vm, value);
    }

    private static FieldInfo GetField(string name)
    {
        return typeof(MainViewModel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
               ?? throw new InvalidOperationException($"Field not found: {name}");
    }
}
