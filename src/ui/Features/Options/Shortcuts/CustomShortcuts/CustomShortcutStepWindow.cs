using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.CustomShortcuts;

public class CustomShortcutStepWindow : Window
{
    public CustomShortcutStepWindow(CustomShortcutStepViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        var language = Se.Language.Options.Shortcuts;
        Title = language.CustomShortcutStep;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        const double inputWidth = InputWidth;

        var labelType = UiUtil.MakeLabel(Se.Language.General.Type);
        var comboBoxType = UiUtil.MakeComboBox(vm.StepTypes, vm, nameof(vm.SelectedStepType));
        comboBoxType.Width = inputWidth; // same right edge as the inputs below
        var panelType = MakeFormGrid((labelType, comboBoxType));

        // Run command: a searchable list of every shortcut command.
        // Search box with icon and a match-count badge, like the Shortcuts window.
        var textBoxSearch = new TextBox
        {
            PlaceholderText = Se.Language.General.Search,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.CommandSearchText)) { Mode = BindingMode.TwoWay },
        }.WithSearchAndClearIcons().WithAccessibleName(Se.Language.General.Search);
        var accentColor = UiUtil.GetAccentBrush() is ISolidColorBrush accentSolid ? accentSolid.Color : Colors.DodgerBlue;
        var badgeCount = new Border
        {
            Background = new SolidColorBrush(accentColor, 0.18),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(8, 3, 8, 2),
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding(nameof(vm.FilteredCommands) + ".Count") { Mode = BindingMode.OneWay },
                FontSize = UiUtil.ScaledFontSize(10),
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(accentColor),
            },
        };
        var panelSearch = new Grid
        {
            Width = inputWidth,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        panelSearch.Add(textBoxSearch, 0, 0);
        panelSearch.Add(badgeCount, 0, 1);
        var listBoxCommands = new ListBox
        {
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.FilteredCommands)) { Mode = BindingMode.OneWay },
            [!Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedCommand)) { Mode = BindingMode.TwoWay },
            Width = inputWidth,
            Height = 300,
            ItemTemplate = new FuncDataTemplate<CustomShortcutCommandItem>((item, _) =>
            {
                var name = new TextBlock { Text = item?.DisplayName, TextTrimming = TextTrimming.CharacterEllipsis };
                var group = new TextBlock { Text = item?.GroupName, Opacity = 0.6, FontSize = 11, TextTrimming = TextTrimming.CharacterEllipsis };
                return new StackPanel { Children = { name, group } };
            }),
        };
        listBoxCommands.DoubleTapped += (_, _) => vm.CommandListDoubleTapped();
        var labelSearch = UiUtil.MakeLabel(Se.Language.General.Search);
        var panelRunCommand = MakeFormGrid(
            (labelSearch, panelSearch),
            (null, UiUtil.MakeBorderForControlNoPadding(listBoxCommands)));
        panelRunCommand.Bind(IsVisibleProperty, new Binding(nameof(vm.IsRunCommand)));

        // Insert text
        var labelText = UiUtil.MakeLabel(Se.Language.General.Text);
        var textBoxText = new TextBox
        {
            Width = inputWidth,
            Height = 70,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            [!TextBox.TextProperty] = new Binding(nameof(vm.Text)) { Mode = BindingMode.TwoWay },
        }.WithAccessibleName(Se.Language.General.Text);
        var labelPosition = UiUtil.MakeLabel(Se.Language.General.Position);
        var comboBoxPosition = UiUtil.MakeComboBox(vm.Positions, vm, nameof(vm.SelectedPosition));
        comboBoxPosition.Width = 250;
        var panelInsertText = MakeFormGrid(
            (labelText, textBoxText),
            (labelPosition, comboBoxPosition));
        panelInsertText.Bind(IsVisibleProperty, new Binding(nameof(vm.IsInsertText)));

        // Find and replace
        var labelFind = UiUtil.MakeLabel(Se.Language.General.Find);
        var textBoxFind = UiUtil.MakeTextBox(inputWidth, vm, nameof(vm.Find));
        var labelReplace = UiUtil.MakeLabel(Se.Language.General.ReplaceWith);
        var textBoxReplace = UiUtil.MakeTextBox(inputWidth, vm, nameof(vm.ReplaceWith));
        var checkBoxRegex = UiUtil.MakeCheckBox(Se.Language.General.RegularExpression, vm, nameof(vm.UseRegex));
        var checkBoxCaseSensitive = UiUtil.MakeCheckBox(Se.Language.General.CaseSensitive, vm, nameof(vm.CaseSensitive));
        var panelReplace = MakeFormGrid(
            (labelFind, textBoxFind),
            (labelReplace, textBoxReplace),
            (null, UiUtil.MakeHorizontalPanel(checkBoxRegex, checkBoxCaseSensitive)));
        panelReplace.Bind(IsVisibleProperty, new Binding(nameof(vm.IsReplace)));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 12,
        };

        var panels = new Panel { Children = { panelRunCommand, panelInsertText, panelReplace } };

        // Every row uses the same label column and input width, so left and right edges line up.
        grid.Add(panelType, 0);
        grid.Add(panels, 1);
        grid.Add(buttonPanel, 2);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, comboBoxType); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private const double LabelWidth = 110;
    private const double InputWidth = 420;

    /// <summary>
    /// Label + input rows with fixed column widths, so every section of the dialog (type, run
    /// command, insert text, find and replace) lines up left and right.
    /// </summary>
    private static Grid MakeFormGrid(params (Control? Label, Control Input)[] rows)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(LabelWidth) },
                new ColumnDefinition { Width = new GridLength(InputWidth) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        for (var i = 0; i < rows.Length; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            if (rows[i].Label is { } label)
            {
                label.VerticalAlignment = VerticalAlignment.Top;
                label.Margin = new Thickness(0, 6, 0, 0);
                grid.Add(label, i);
            }

            rows[i].Input.HorizontalAlignment = HorizontalAlignment.Left;
            grid.Add(rows[i].Input, i, 1);
        }

        return grid;
    }
}
