using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Edit;
using Nikse.SubtitleEdit.Features.Video;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.CustomSearch;

public class CustomSearchWindow : Window
{
    public CustomSearchWindow(CustomSearchViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 500;
        CanResize = false;
        Title = Se.Language.Options.Shortcuts.SearchVia;
        EditDialogChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        const double labelWidth = 80;

        var textBoxName = VideoSubDialogChrome.MakeTextBox(nameof(vm.Name), Se.Language.General.Name, 300);
        var textBoxUrl = VideoSubDialogChrome.MakeTextBox(nameof(vm.Url), Se.Language.General.Url, 380);

        // The URL is a template, which is not obvious from an empty text box.
        var labelHint = VideoSubDialogChrome.MakeHintText(Se.Language.Options.Shortcuts.SearchViaUrlHint);
        labelHint.MaxWidth = 380;
        labelHint.HorizontalAlignment = HorizontalAlignment.Left;

        var rows = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                VideoSubDialogChrome.MakeRow(Se.Language.General.Name, textBoxName, labelWidth),
                VideoSubDialogChrome.MakeRow(Se.Language.General.Url, textBoxUrl, labelWidth),
                VideoSubDialogChrome.MakeRow(string.Empty, labelHint, labelWidth),
            },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = VideoSubDialogChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(VideoSubDialogChrome.MakePanel(EditorChrome.MakeEditorSection(Se.Language.Options.Shortcuts.SearchVia, rows, first: true)), 0);
        grid.Add(buttonPanel, 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxName); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
