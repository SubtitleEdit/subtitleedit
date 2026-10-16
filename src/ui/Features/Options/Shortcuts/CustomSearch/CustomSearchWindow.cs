using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        const double labelWidth = 80;

        var textBoxName = EditorChrome.MakeTextField(nameof(vm.Name), Se.Language.General.Name, 300);
        var textBoxUrl = EditorChrome.MakeTextField(nameof(vm.Url), Se.Language.General.Url, 380);

        // The URL is a template, which is not obvious from an empty text box.
        var labelHint = EditorChrome.MakeHint(Se.Language.Options.Shortcuts.SearchViaUrlHint);
        labelHint.MaxWidth = 380;
        labelHint.HorizontalAlignment = HorizontalAlignment.Left;

        var rows = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.Name, textBoxName, labelWidth, spacing: 6),
                EditorChrome.MakeLabeledRow(Se.Language.General.Url, textBoxUrl, labelWidth, spacing: 6),
                EditorChrome.MakeLabeledRow(string.Empty, labelHint, labelWidth, spacing: 6),
            },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

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

        grid.Add(EditorChrome.MakePanel(EditorChrome.MakeEditorSection(Se.Language.Options.Shortcuts.SearchVia, rows, first: true)), 0);
        grid.Add(buttonPanel, 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxName); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
