using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Files.ManualChosenEncoding;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Features.Shared.FindText;
using Nikse.SubtitleEdit.Features.Shared.PickFontName;
using Nikse.SubtitleEdit.Features.Shared.PickLanguage;
using Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace UITests.Logic;

/// <summary>
/// Search/filter boxes get a magnifier icon and an "x" clear button (follow-up to #14893).
/// </summary>
public partial class SearchAndClearIconsTests
{
    private sealed partial class SearchViewModel : ObservableObject
    {
        [ObservableProperty] private string _searchText = string.Empty;
    }

    private static Button ClearButton(TextBox textBox) => Assert.IsType<Button>(textBox.InnerRightContent);

    [AvaloniaFact]
    public void AddsSearchIcon_AndAnAccessibleClearButton_HiddenWhileEmpty()
    {
        var textBox = new TextBox().WithSearchAndClearIcons();

        var icon = Assert.IsType<Icon>(textBox.InnerLeftContent);
        Assert.Equal(IconNames.Find, icon.Value);
        var clearButton = ClearButton(textBox);
        Assert.Equal(Se.Language.General.Clear, AutomationProperties.GetName(clearButton));
        Assert.False(clearButton.Focusable);
        Assert.False(clearButton.IsVisible);

        textBox.Text = "abc";
        Assert.True(clearButton.IsVisible);
    }

    [AvaloniaFact]
    public void ClearButton_ClearsText_PushesItToTheBinding_AndRaisesTextChanged()
    {
        var vm = new SearchViewModel();
        var textBox = UiUtil.MakeTextBox(200, vm, nameof(vm.SearchText)).WithSearchAndClearIcons();
        var window = new Window { DataContext = vm, Content = textBox };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var textChangedCount = 0;
            textBox.TextChanged += (_, _) => textChangedCount++;

            textBox.Text = "font";
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("font", vm.SearchText);

            ClearButton(textBox).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(string.Empty, vm.SearchText);
            Assert.True(string.IsNullOrEmpty(textBox.Text));
            // Filters that listen to TextChanged (not the binding) re-run too.
            Assert.Equal(2, textChangedCount);
            Assert.False(ClearButton(textBox).IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    public static TheoryData<Type, Type> WindowsWithSearchBoxes => new()
    {
        { typeof(FixCommonErrorsWindow), typeof(FixCommonErrorsViewModel) },
        { typeof(PickLanguageWindow), typeof(PickLanguageViewModel) },
        { typeof(PickFontNameWindow), typeof(PickFontNameViewModel) },
        { typeof(PickSubtitleFormatWindow), typeof(PickSubtitleFormatViewModel) },
        { typeof(ManualChosenEncodingWindow), typeof(ManualChosenEncodingViewModel) },
        { typeof(FindRuleWindow), typeof(FindRuleViewModel) },
        { typeof(FindTextWindow), typeof(FindTextViewModel) },
        { typeof(ShortcutsWindow), typeof(ShortcutsViewModel) },
        { typeof(BatchConvertWindow), typeof(BatchConvertViewModel) },
        { typeof(VoiceManagerWindow), typeof(VoiceManagerViewModel) },
        { typeof(SettingsWindow), typeof(SettingsViewModel) },
    };

    [AvaloniaTheory]
    [MemberData(nameof(WindowsWithSearchBoxes))]
    public void Window_SearchBox_HasTheClearButton(Type windowType, Type viewModelType)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService(viewModelType);
        var window = (Window)Activator.CreateInstance(windowType, vm)!;
        try
        {
            var decorated = window.GetLogicalDescendants().OfType<TextBox>()
                .Where(t => t.InnerLeftContent is Icon { Value: IconNames.Find } && t.InnerRightContent is Button)
                .ToList();
            Assert.Single(decorated);
        }
        finally
        {
            window.Close();
        }
    }
}
