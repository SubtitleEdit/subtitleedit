using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Edit.Find;

/// <summary>
/// Find, in the editor look (see <see cref="EditorChrome"/>): the search text and how to match on
/// the left, the find actions with the result line in a column to the right. Non-modal.
/// </summary>
public class FindWindow : Window
{
    public FindWindow(FindViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.RemoveAccessKey(Se.Language.General.Find);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var textBoxFind = new AutoCompleteBox
        {
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 200,
            PlaceholderText = Se.Language.Edit.Find.SearchTextWatermark,
            ItemsSource = vm.SearchHistory,
            [!AutoCompleteBox.TextProperty] = new Binding(nameof(vm.SearchText)),
            MinimumPrefixLength = 0,
        }.WithAccessibleName(Se.Language.General.Find); // AutoCompleteBox has no watermark-derived name (#12087)
        textBoxFind.KeyDown += vm.FindTextBoxKeyDown;
        FindWindowParts.SetHistoryItemTemplate(textBoxFind, vm.RemoveHistoryItemCommand);

        var buttonHistory = FindWindowParts.MakeHistoryButton(vm.SearchHistory, vm.ShowHistoryCommand, vm.RemoveHistoryItemCommand, vm.ClearHistoryCommand);
        MakeHistoryButtonCompact(buttonHistory);
        var panelFind = FindWindowParts.MakeSearchPanel(textBoxFind, buttonHistory);

        var findRows = new StackPanel { Spacing = 10 };
        findRows.Children.Add(panelFind);
        findRows.Children.Add(EditorChrome.MakeToggleRow(nameof(vm.WholeWord), Se.Language.Edit.Find.WholeWord));

        var sections = new StackPanel { MinWidth = 280 };
        sections.Children.Add(EditorChrome.MakeEditorSection(UiUtil.RemoveAccessKey(Se.Language.General.Find), findRows, first: true));
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Match, MakeFindModeRadioButtons()));

        var buttonFindNext = EditorChrome.StyleActionColumnButton(UiUtil.MakeButton(Se.Language.Edit.Find.FindNext, vm.FindNextCommand)
            .WithIconLeft(IconNames.ChevronRight));
        buttonFindNext.Classes.Add("primary");
        var buttonFindPrevious = EditorChrome.StyleActionColumnButton(UiUtil.MakeButton(Se.Language.Edit.Find.FindPrevious, vm.FindPreviousCommand)
            .WithIconLeft(IconNames.ChevronLeft));
        var buttonCount = EditorChrome.StyleActionColumnButton(UiUtil.MakeButton(Se.Language.General.Count, vm.CountCommand)
            .WithIconLeft(IconNames.Counter));

        var panelResult = FindWindowParts.MakeResultPanel(nameof(vm.CountResult), nameof(vm.ResultIcon));
        panelResult.Margin = new Thickness(2, 4, 0, 0);

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            MinWidth = 160,
            Children =
            {
                buttonFindNext,
                buttonFindPrevious,
                buttonCount,
                panelResult
            }
        };

        Content = EditorChrome.MakeSidePanelRoot(sections, EditorChrome.MakeActionColumn(panelButtons));

        vm.FocusSearchBox = () => Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            textBoxFind.GetVisualDescendants()
                       .OfType<TextBox>()
                       .FirstOrDefault()?
                       .Focus();
        });

        Opened += delegate
        {
            vm.FocusSearchBox();

            // The AutoCompleteBox's inner TextBox only exists after the template is applied.
            var innerTextBox = textBoxFind.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
            if (innerTextBox != null)
            {
                RegexContextFlyout.Attach(innerTextBox, vm, () => vm.FindMode == FindService.FindMode.RegularExpression);
            }
        };
        AddHandler(KeyDownEvent, vm.OnKeyDown, RoutingStrategies.Tunnel);
        Closing += (_, _) => vm.SaveSettings();
    }

    /// <summary>
    /// The search history button as a flat icon button next to the search box.
    /// </summary>
    internal static void MakeHistoryButtonCompact(Button buttonHistory)
    {
        buttonHistory.Classes.Add("tool");
        buttonHistory.Width = 28;
        buttonHistory.Height = 28;
        buttonHistory.Margin = new Thickness(4, 0, 0, 0);
    }

    /// <summary>
    /// Case sensitive / case insensitive / regular expression (bound to FindMode of the window's
    /// view model) - radio buttons in one parent, so they form one group.
    /// </summary>
    internal static StackPanel MakeFindModeRadioButtons()
    {
        var valueConverter = new FindModeValueConverter();
        RadioButton Make(string text, FindService.FindMode mode) => new()
        {
            Content = text,
            FontSize = 12.5,
            VerticalAlignment = VerticalAlignment.Center,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(FindViewModel.FindMode))
            {
                Converter = valueConverter,
                ConverterParameter = mode,
                Mode = BindingMode.TwoWay
            }
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            Children =
            {
                Make(Se.Language.General.CaseSensitive, FindService.FindMode.CaseSensitive),
                Make(Se.Language.General.CaseInsensitive, FindService.FindMode.CaseInsensitive),
                Make(Se.Language.General.RegularExpression, FindService.FindMode.RegularExpression),
            }
        };
    }
}
