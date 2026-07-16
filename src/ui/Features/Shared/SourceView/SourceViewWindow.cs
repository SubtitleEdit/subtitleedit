using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using AvaloniaEdit;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.SourceView;

public class SourceViewWindow : Window
{
    public SourceViewWindow(SourceViewViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        Width = 1200;
        Height = 800;
        MinWidth = 700;
        MinHeight = 400;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        // Add a context menu for the advanced text editor.
        if (vm.SourceViewTextBox.TextControl is TextEditor textEditor)
        {
            var textBoxContextFlyout = new MenuFlyout
            {
                Placement = PlacementMode.Pointer,
                Items =
                {
                    new MenuItem { Header = Se.Language.General.Cut, Command = vm.CutCommand },
                    new MenuItem { Header = Se.Language.General.Copy, Command = vm.CopyCommand },
                    new MenuItem { Header = Se.Language.General.Paste, Command = vm.PasteCommand },
                },
            };
            textEditor.ContextFlyout = textBoxContextFlyout;
            UiUtil.AttachMacContextFlyoutHandler(textEditor);
        }

        var contentBorder = new Border
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            Child = vm.SourceViewTextBox.ContentControl,
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var labelCursorPosition = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.LineAndColumnInfo)).WithAlignmentTop();

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(contentBorder, 0);
        grid.Add(labelCursorPosition, 1);
        grid.Add(buttonPanel, 1);

        Content = grid;

        Opened += delegate { Avalonia.Threading.Dispatcher.UIThread.Post(vm.FocusEditor); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
