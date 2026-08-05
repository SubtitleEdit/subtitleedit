using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.SourceView;

public class SourceViewWindow : Window
{
    /// <summary>Set once the discard prompt was answered, so the second Close() is not questioned.</summary>
    private bool _allowClose;

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
        if (vm.SourceViewTextBox.TextControl is SyntaxTextView textEditor)
        {
            var textBoxContextFlyout = new MenuFlyout
            {
                Placement = PlacementMode.Pointer,
                Items =
                {
                    new MenuItem { Header = Se.Language.General.Cut, Command = vm.CutCommand },
                    new MenuItem { Header = Se.Language.General.Copy, Command = vm.CopyCommand },
                    new MenuItem { Header = Se.Language.General.Paste, Command = vm.PasteCommand },
                    new MenuItem { Header = Se.Language.General.SelectAll, Command = vm.SelectAllCommand },
                    new Separator(),
                    new MenuItem { Header = Se.Language.General.Undo, Command = vm.UndoCommand },
                    new MenuItem { Header = Se.Language.General.Redo, Command = vm.RedoCommand },
                    new Separator(),

                    // The line commands live in the editor's key handling; the menu is what makes
                    // them findable at all.
                    new MenuItem { Header = Se.Language.SourceView.MoveLineUp, Command = vm.MoveLineUpCommand },
                    new MenuItem { Header = Se.Language.SourceView.MoveLineDown, Command = vm.MoveLineDownCommand },
                    new MenuItem { Header = Se.Language.SourceView.DuplicateLine, Command = vm.DuplicateLineCommand },
                    new MenuItem { Header = Se.Language.SourceView.DeleteLine, Command = vm.DeleteLineCommand },
                    new Separator(),
                    new MenuItem { Header = Se.Language.General.Find, Command = vm.ShowFindCommand },
                    new MenuItem { Header = Se.Language.General.Replace, Command = vm.ShowReplaceCommand },
                    new MenuItem { Header = Se.Language.General.GoToLineNumber, Command = vm.GoToLineCommand },
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

        var findBar = MakeFindBar(vm);
        var bottomPanel = MakeBottomPanel(vm);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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
        grid.Add(findBar, 1);
        grid.Add(bottomPanel, 2);

        Content = grid;

        Opened += delegate { Avalonia.Threading.Dispatcher.UIThread.Post(vm.FocusEditor); };
        AddHandler(KeyDownEvent, vm.OnKeyDown, RoutingStrategies.Tunnel);

        Closing += async (_, e) =>
        {
            if (!_allowClose && vm.NeedsDiscardConfirmation)
            {
                // Cancel first, synchronously - the close cannot be stopped after the await.
                e.Cancel = true;
                if (await vm.ConfirmDiscardAsync())
                {
                    _allowClose = true;
                    Close();
                }

                return;
            }

            UiUtil.SaveWindowPosition(this);
        };

        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    /// <summary>
    /// The search bar sits between the editor and the buttons and is hidden until Ctrl+F / Ctrl+H.
    /// A dialog on top of a dialog would be in the way of the text it is searching.
    /// </summary>
    private static Border MakeFindBar(SourceViewViewModel vm)
    {
        var searchTextBox = new TextBox
        {
            Width = 260,
            PlaceholderText = Se.Language.Edit.Find.SearchTextWatermark,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SearchText)) { Mode = BindingMode.TwoWay },
        };
        searchTextBox.TextChanged += vm.SearchTextChanged;
        vm.SearchTextBox = searchTextBox;

        var replaceTextBox = new TextBox
        {
            Width = 260,
            PlaceholderText = Se.Language.Edit.Find.ReplaceTextWatermark,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.ReplaceText)) { Mode = BindingMode.TwoWay },
            [!IsVisibleProperty] = new Binding(nameof(vm.IsReplaceVisible)),
        };
        vm.ReplaceTextBox = replaceTextBox;

        var buttonFindPrevious = UiUtil.MakeButton(Se.Language.Edit.Find.FindPrevious, vm.FindPreviousCommand);
        var buttonFindNext = UiUtil.MakeButton(Se.Language.Edit.Find.FindNext, vm.FindNextCommand);

        var buttonReplace = UiUtil.MakeButton(Se.Language.General.Replace, vm.ReplaceCommand);
        buttonReplace.Bind(IsVisibleProperty, new Binding(nameof(vm.IsReplaceVisible)));

        var buttonReplaceAll = UiUtil.MakeButton(Se.Language.Edit.Find.ReplaceAll, vm.ReplaceAllCommand);
        buttonReplaceAll.Bind(IsVisibleProperty, new Binding(nameof(vm.IsReplaceVisible)));

        // The options sit shoulder to shoulder after the buttons, so they need their own gap.
        var optionMargin = new Thickness(10, 0, 0, 0);
        var checkBoxMatchCase = UiUtil.MakeCheckBox(Se.Language.General.CaseSensitive, vm, nameof(vm.MatchCase));
        checkBoxMatchCase.Margin = optionMargin;
        var checkBoxWholeWord = UiUtil.MakeCheckBox(Se.Language.Edit.Find.WholeWord, vm, nameof(vm.WholeWord));
        checkBoxWholeWord.Margin = optionMargin;
        var checkBoxRegex = UiUtil.MakeCheckBox(Se.Language.General.RegularExpression, vm, nameof(vm.UseRegularExpression));
        checkBoxRegex.Margin = optionMargin;

        var labelStatus = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.FindStatus));
        labelStatus.Margin = optionMargin;

        var buttonClose = UiUtil.MakeButton(vm.CloseFindBarCommand, IconNames.Close, Se.Language.SourceView.CloseSearch);
        buttonClose.HorizontalAlignment = HorizontalAlignment.Right;

        var panel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            LineSpacing = 6,
            Children =
            {
                searchTextBox,
                buttonFindPrevious,
                buttonFindNext,
                replaceTextBox,
                buttonReplace,
                buttonReplaceAll,
                checkBoxMatchCase,
                checkBoxWholeWord,
                checkBoxRegex,
                labelStatus,
            },
        };

        var innerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowDefinitions = { new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) } },
        };
        innerGrid.Add(panel, 0, 0);
        innerGrid.Add(buttonClose, 0, 1);

        var border = new Border
        {
            Child = innerGrid,
            Margin = new Thickness(0, 6, 10, 0),
            Padding = new Thickness(6, 4),
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetTextColor(0.3d),
            CornerRadius = new CornerRadius(UiUtil.CornerRadius),
            [!IsVisibleProperty] = new Binding(nameof(vm.IsFindBarVisible)),
        };

        return border;
    }

    private static Grid MakeBottomPanel(SourceViewViewModel vm)
    {
        var labelCursorPosition = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.LineAndColumnInfo));
        var labelSelection = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.SelectionInfo));

        // Red only when the source does not parse - the count is ordinary information otherwise.
        var labelValidation = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.ValidationInfo));
        labelValidation.Bind(ForegroundProperty, new Binding(nameof(vm.IsValidationError))
        {
            Converter = new BooleanToBrushConverter
            {
                TrueBrush = new SolidColorBrush(Color.FromRgb(0xD3, 0x2F, 0x2F)),
                FalseBrush = UiUtil.GetTextColor(0.7d),
            },
        });

        var statusPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 15,
            Children = { labelCursorPosition, labelSelection, labelValidation },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowDefinitions = { new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) } },
        };

        grid.Add(statusPanel, 0, 0);
        grid.Add(buttonPanel, 0, 1);

        return grid;
    }
}
