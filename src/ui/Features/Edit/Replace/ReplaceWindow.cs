using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Edit.Replace;

/// <summary>
/// Replace, in the editor look (see <see cref="EditorChrome"/>): what to find, what to replace it
/// with, how to match and where on the left; the actions with the result line in a column to the
/// right. Non-modal.
/// </summary>
public class ReplaceWindow : Window
{
    public ReplaceWindow(ReplaceViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.RemoveAccessKey(Se.Language.General.Replace);
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
        FindWindow.MakeHistoryButtonCompact(buttonHistory);
        var panelSearch = FindWindowParts.MakeSearchPanel(textBoxFind, buttonHistory);

        var findRows = new StackPanel { Spacing = 10 };
        findRows.Children.Add(panelSearch);
        findRows.Children.Add(EditorChrome.MakeToggleRow(nameof(vm.WholeWord), Se.Language.Edit.Find.WholeWord));

        var textBoxReplace = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 180,
            PlaceholderText = Se.Language.Edit.Find.ReplaceTextWatermark,
            [!TextBox.TextProperty] = new Binding(nameof(vm.ReplaceText)) { Mode = BindingMode.TwoWay }
        };
        textBoxReplace.Classes.Add("text");
        textBoxReplace.KeyDown += vm.ReplaceTextBoxKeyDown;

        var comboBoxScope = UiUtil.MakeComboBox(vm.Scopes, vm, nameof(vm.SelectedScope)).WithAccessibleName(Se.Language.Edit.Find.ReplaceIn);
        comboBoxScope.HorizontalAlignment = HorizontalAlignment.Stretch;

        // Only relevant while an editable original text column is on screen, so the whole block
        // stays hidden the rest of the time instead of offering choices with no effect.
        var sectionScope = EditorChrome.MakeEditorSection(Se.Language.Edit.Find.ReplaceIn, comboBoxScope);
        sectionScope.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsScopeVisible)) { Source = vm });

        var sections = new StackPanel { MinWidth = 280 };
        sections.Children.Add(EditorChrome.MakeEditorSection(UiUtil.RemoveAccessKey(Se.Language.General.Find), findRows, first: true));
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.ReplaceWith, textBoxReplace));
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Match, FindWindow.MakeFindModeRadioButtons()));
        sections.Children.Add(sectionScope);

        var buttonFindNext = EditorChrome.StyleActionColumnButton(UiUtil.MakeButton(Se.Language.Edit.Find.FindNext, vm.FindNextCommand)
            .WithIconLeft(IconNames.ChevronRight));
        var buttonReplace = EditorChrome.StyleActionColumnButton(UiUtil.MakeButton(Se.Language.Edit.Find.ReplaceAndFindNext, vm.ReplaceCommand)
            .WithIconLeft(IconNames.FindReplace));
        buttonReplace.Classes.Add("primary");
        var buttonReplaceAll = EditorChrome.StyleActionColumnButton(UiUtil.MakeButton(Se.Language.Edit.Find.ReplaceAll, vm.ReplaceAllCommand)
            .WithIconLeft(IconNames.FileReplaceOutline));
        var buttonCount = EditorChrome.StyleActionColumnButton(UiUtil.MakeButton(Se.Language.General.Count, vm.CountCommand)
            .WithIconLeft(IconNames.Counter));
        var panelResult = FindWindowParts.MakeResultPanel(nameof(vm.CountResult), nameof(vm.ResultIcon));
        panelResult.Margin = new Thickness(2, 4, 0, 0);

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            MinWidth = 190,
            Children =
            {
                buttonFindNext,
                buttonReplace,
                buttonReplaceAll,
                buttonCount,
                panelResult,
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

        RegexContextFlyout.Attach(textBoxReplace, vm, () => vm.FindMode == FindService.FindMode.RegularExpression, isReplaceBox: true);

        Opened += delegate
        {
            // The AutoCompleteBox's inner TextBox only exists after the template is applied.
            var innerTextBox = textBoxFind.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
            if (innerTextBox != null)
            {
                RegexContextFlyout.Attach(innerTextBox, vm, () => vm.FindMode == FindService.FindMode.RegularExpression);
            }

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (vm.FocusReplaceOnOpen)
                {
                    textBoxReplace.Focus();
                }
                else
                {
                    textBoxFind.GetVisualDescendants()
                               .OfType<TextBox>()
                               .FirstOrDefault()?
                               .Focus();
                }
            });
        };
        AddHandler(KeyDownEvent, vm.OnKeyDown, RoutingStrategies.Tunnel);
        Closing += (_, _) => vm.SaveSettings();
    }
}
