using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;

/// <summary>
/// Go to line number, in the editor look (see <see cref="EditorChrome"/>): the field with the
/// number of lines, the line it points at where there is a preview, Cancel and OK at the bottom.
/// </summary>
public class GoToLineNumberWindow : Window
{
    public GoToLineNumberWindow(GoToLineNumberViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.GoToLineNumber;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        vm.UpDown = new NumericUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            Width = 150,
            VerticalContentAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.LineNumber))
            {
                Mode = BindingMode.TwoWay,
            },
            Minimum = 1,
            Increment = 1,          // Only step in whole numbers
            FormatString = "F0",    // Show 0 decimal places
        };
        vm.UpDown.Classes.Add("field");
        AutomationProperties.SetName(vm.UpDown, Se.Language.General.GoToLineNumber);
        vm.UpDown.KeyDown += (sender, args) => vm.OnKeyDown(args);

        // A section header over the field, like Go to video position
        vm.UpDown.HorizontalAlignment = HorizontalAlignment.Left;
        var fieldRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                vm.UpDown,
                new TextBlock
                {
                    FontSize = EditorChrome.FieldFontSize,
                    Foreground = EditorChrome.DimTextBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(vm.TotalText)),
                },
            },
        };

        var preview = new Border
        {
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 8),
            Height = 76,
            Child = new StackPanel
            {
                Spacing = 3,
                Children =
                {
                    new TextBlock
                    {
                        FontSize = EditorChrome.ScaledFont(11),
                        Foreground = EditorChrome.DimTextBrush,
                        [!TextBlock.TextProperty] = new Binding(nameof(vm.PreviewTime)),
                    },
                    new TextBlock
                    {
                        FontSize = EditorChrome.FieldFontSize,
                        MaxLines = 2,
                        TextWrapping = TextWrapping.Wrap,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        [!TextBlock.TextProperty] = new Binding(nameof(vm.PreviewText)),
                    },
                },
            },
            [!IsVisibleProperty] = new Binding(nameof(vm.IsPreviewVisible)),
        };

        var row = EditorChrome.MakeEditorSection(Se.Language.General.GoToLineNumber, new StackPanel { Spacing = 10, Children = { fieldRow, preview } }, first: true);
        row.Width = 380;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(row, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, () => { vm.Activated(); });
        KeyDown += (_, args) => vm.OnKeyDown(args);
    }
}
