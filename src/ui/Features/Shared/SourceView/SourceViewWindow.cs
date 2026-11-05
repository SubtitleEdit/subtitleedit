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

/// <summary>
/// Source view, in the editor look (see <see cref="EditorChrome"/>): the syntax editor in a
/// field frame, the find bar as a strip below it, cursor/validation status in the button bar.
/// </summary>
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
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

        Avalonia.Automation.AutomationProperties.SetName(vm.SourceViewTextBox.TextControl, Se.Language.General.Text);

        // The editor in a field frame; its syntax colors stay as they are (theme-stable pastels)
        var contentFrame = EditorChrome.MakeListFrame(vm.SourceViewTextBox.ContentControl);
        contentFrame.Margin = new Thickness(14, 14, 14, 12);

        var findBar = MakeFindBar(vm);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        grid.Add(contentFrame, 0);
        grid.Add(findBar, 1);

        Content = EditorChrome.MakeRoot(grid, MakeBottomPanel(vm));

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
            Width = 240,
            PlaceholderText = Se.Language.Edit.Find.SearchTextWatermark,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SearchText)) { Mode = BindingMode.TwoWay },
        };
        searchTextBox.Classes.Add("text");
        Avalonia.Automation.AutomationProperties.SetName(searchTextBox, Se.Language.General.Find);
        searchTextBox.TextChanged += vm.SearchTextChanged;
        vm.SearchTextBox = searchTextBox;

        var replaceTextBox = new TextBox
        {
            Width = 240,
            PlaceholderText = Se.Language.Edit.Find.ReplaceTextWatermark,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.ReplaceText)) { Mode = BindingMode.TwoWay },
            [!IsVisibleProperty] = new Binding(nameof(vm.IsReplaceVisible)),
        };
        replaceTextBox.Classes.Add("text");
        Avalonia.Automation.AutomationProperties.SetName(replaceTextBox, Se.Language.General.ReplaceWith);
        vm.ReplaceTextBox = replaceTextBox;

        var buttonFindPrevious = EditorChrome.MakeTextToolButton("fa-solid fa-chevron-up", Se.Language.Edit.Find.FindPrevious, vm.FindPreviousCommand);
        var buttonFindNext = EditorChrome.MakeTextToolButton("fa-solid fa-chevron-down", Se.Language.Edit.Find.FindNext, vm.FindNextCommand);

        var buttonReplace = EditorChrome.MakeTextToolButton("fa-solid fa-right-left", Se.Language.General.Replace, vm.ReplaceCommand);
        buttonReplace.Bind(IsVisibleProperty, new Binding(nameof(vm.IsReplaceVisible)));

        var buttonReplaceAll = EditorChrome.MakeTextToolButton("fa-solid fa-repeat", Se.Language.Edit.Find.ReplaceAll, vm.ReplaceAllCommand);
        buttonReplaceAll.Bind(IsVisibleProperty, new Binding(nameof(vm.IsReplaceVisible)));

        // The options sit shoulder to shoulder after the buttons, so they need their own gap.
        var optionMargin = new Thickness(10, 0, 0, 0);
        var checkBoxMatchCase = UiUtil.MakeCheckBox(Se.Language.General.CaseSensitive, vm, nameof(vm.MatchCase));
        checkBoxMatchCase.Margin = optionMargin;
        var checkBoxWholeWord = UiUtil.MakeCheckBox(Se.Language.Edit.Find.WholeWord, vm, nameof(vm.WholeWord));
        checkBoxWholeWord.Margin = optionMargin;
        var checkBoxRegex = UiUtil.MakeCheckBox(Se.Language.General.RegularExpression, vm, nameof(vm.UseRegularExpression));
        checkBoxRegex.Margin = optionMargin;

        var labelStatus = EditorChrome.MakeBoundHint(nameof(vm.FindStatus));
        labelStatus.Margin = optionMargin;

        var buttonClose = EditorChrome.CreateToolButton("fa-solid fa-xmark", Se.Language.SourceView.CloseSearch, vm.CloseFindBarCommand, 28);
        buttonClose.HorizontalAlignment = HorizontalAlignment.Right;
        buttonClose.VerticalAlignment = VerticalAlignment.Top;

        var panel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            LineSpacing = 6,
            ItemSpacing = 6,
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
            Padding = new Thickness(14, 8, 10, 8),
            Background = EditorChrome.StatusBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            BorderBrush = EditorChrome.LineBrush,
            [!IsVisibleProperty] = new Binding(nameof(vm.IsFindBarVisible)),
        };

        return border;
    }

    private static Border MakeBottomPanel(SourceViewViewModel vm)
    {
        var labelCursorPosition = EditorChrome.MakeBoundHint(nameof(vm.LineAndColumnInfo));
        var labelSelection = EditorChrome.MakeBoundHint(nameof(vm.SelectionInfo));

        // Red only when the source does not parse - the count is ordinary information otherwise.
        var labelValidation = EditorChrome.MakeBoundHint(nameof(vm.ValidationInfo));
        labelValidation.Bind(TextBlock.ForegroundProperty, new Binding(nameof(vm.IsValidationError))
        {
            Converter = new BooleanToBrushConverter
            {
                TrueBrush = new SolidColorBrush(Color.FromRgb(0xD3, 0x2F, 0x2F)),
                FalseBrush = EditorChrome.DimTextBrush,
            },
        });

        var statusPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 15,
            Children = { labelCursorPosition, labelSelection, labelValidation },
        };

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return EditorChrome.MakeButtonBar(statusPanel, buttonCancel, buttonOk);
    }
}
