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
        vm.Window = this;
        DataContext = vm;

        var labelName = UiUtil.MakeLabel(Se.Language.General.Name);
        var textBoxName = UiUtil.MakeTextBox(300, vm, nameof(vm.Name));

        var labelUrl = UiUtil.MakeLabel(Se.Language.General.Url);
        var textBoxUrl = UiUtil.MakeTextBox(300, vm, nameof(vm.Url));

        // The URL is a template, which is not obvious from an empty text box.
        var labelHint = UiUtil.MakeLabel(Se.Language.Options.Shortcuts.SearchViaUrlHint);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelName, 0);
        grid.Add(textBoxName, 0, 1);
        grid.Add(labelUrl, 1);
        grid.Add(textBoxUrl, 1, 1);
        grid.Add(labelHint, 2, 1);
        grid.Add(buttonPanel, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { textBoxName.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
