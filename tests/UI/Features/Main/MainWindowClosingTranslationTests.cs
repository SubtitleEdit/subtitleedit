using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

public class MainWindowClosingTranslationTests
{
    [AvaloniaFact]
    public void OnlyOriginalChanged_FirstClosePromptIsForOriginal()
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

        var vm = (MainViewModel)view.DataContext!;
        var line = new SubtitleLineViewModel(new Paragraph("Main text", 0, 2000), null!)
        {
            OriginalText = "Original text",
        };
        vm.Subtitles.Add(line);
        vm.ShowColumnOriginalText = true;
        var mainHash = vm.GetFastHash();
        SetPrivateField(vm, "_changeSubtitleHash", mainHash);
        SetPrivateField(vm, "_changeSubtitleHashOriginal", vm.GetFastHashOriginal());
        line.OriginalText = "Changed original text";

        Assert.True(vm.HasChanges());
        Assert.Equal(mainHash, vm.GetFastHash());

        try
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();

            var messageBox = Assert.Single(
                window.OwnedWindows.OfType<MessageBox>(),
                p => p.Title == Se.Language.General.SaveChangesTitle);
            var messages = messageBox.GetVisualDescendants()
                .OfType<TextBlock>()
                .Select(p => p.Text)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();
            var expected = string.Format(
                Se.Language.General.SaveChangesToXOriginal,
                Se.Language.General.Untitled);

            Assert.Contains(expected, messages);
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
        }
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
