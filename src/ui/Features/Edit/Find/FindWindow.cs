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

public class FindWindow : Window
{
    public FindWindow(FindViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Find;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var textBoxFind = new AutoCompleteBox
        {
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 200,
            Margin = new Thickness(0, 0, 0, 3),
            PlaceholderText = Se.Language.Edit.Find.SearchTextWatermark,
            ItemsSource = vm.SearchHistory,
            [!AutoCompleteBox.TextProperty] = new Binding(nameof(vm.SearchText)),
            MinimumPrefixLength = 0,
        };
        textBoxFind.KeyDown += vm.FindTextBoxKeyDown;

        // SE4-style "most recent find text" dropdown: the AutoCompleteBox only reveals history
        // while typing a matching prefix, so recent searches were invisible until this button.
        var historyFlyout = new MenuFlyout();
        var buttonHistory = UiUtil.MakeButton(null, IconNames.History);
        buttonHistory.Margin = new Thickness(3, 0, 0, 3);
        buttonHistory.Flyout = historyFlyout;
        ToolTip.SetTip(buttonHistory, Se.Language.General.ShowHistory);

        // The items must exist BEFORE the flyout opens: items added from the Opening
        // event come too late for the popup's initial measure, so the menu displayed
        // as an empty sliver on Windows. Build eagerly and rebuild on history changes.
        void RebuildHistoryMenu()
        {
            historyFlyout.Items.Clear();
            foreach (var text in vm.SearchHistory)
            {
                historyFlyout.Items.Add(new MenuItem
                {
                    // TextBlock header: a plain string header would eat '_' as an access-key marker.
                    Header = new TextBlock { Text = text },
                    Command = vm.ShowHistoryCommand,
                    CommandParameter = text,
                });
            }

            buttonHistory.IsVisible = vm.SearchHistory.Count > 0;
        }

        vm.SearchHistory.CollectionChanged += (_, _) => RebuildHistoryMenu();
        RebuildHistoryMenu();

        var panelFind = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            VerticalAlignment = VerticalAlignment.Center,
        };
        panelFind.Add(textBoxFind, 0, 0);
        panelFind.Add(buttonHistory, 0, 1);

        var checkBoxWholeWord = new CheckBox
        {
            Content = Se.Language.Edit.Find.WholeWord,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 3),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.WholeWord)) { Mode = BindingMode.TwoWay }
        };

        var valueConverter = new FindModeValueConverter();

        var radioButtonCaseSensitive = new RadioButton
        {
            Content = Se.Language.Edit.Find.CaseSensitive,
            VerticalAlignment = VerticalAlignment.Center,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.FindMode))
            {
                Converter = valueConverter,
                ConverterParameter = FindService.FindMode.CaseSensitive,
                Mode = BindingMode.TwoWay
            }
        };

        var radioButtonCaseInsensitive = new RadioButton
        {
            Content = Se.Language.Edit.Find.CaseInsensitive,
            VerticalAlignment = VerticalAlignment.Center,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.FindMode))
            {
                Converter = valueConverter,
                ConverterParameter = FindService.FindMode.CaseInsensitive,
                Mode = BindingMode.TwoWay
            }
        };

        var radioButtonRegularExpression = new RadioButton
        {
            Content = Se.Language.General.RegularExpression,
            VerticalAlignment = VerticalAlignment.Center,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.FindMode))
            {
                Converter = valueConverter,
                ConverterParameter = FindService.FindMode.RegularExpression,
                Mode = BindingMode.TwoWay
            }
        };

        var panelFindTypes = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 0,
            Children =
            {
                radioButtonCaseSensitive,
                radioButtonCaseInsensitive,
                radioButtonRegularExpression
            }
        };

        var buttonFindPrevious = UiUtil.MakeButton(Se.Language.Edit.Find.FindPrevious, vm.FindPreviousCommand)
            .WithLeftAlignment()
            .WithMinWidth(150)
            .WithMargin(0, 0, 0, 10);
        var buttonFindNext = UiUtil.MakeButton(Se.Language.Edit.Find.FindNext, vm.FindNextCommand)
            .WithLeftAlignment()
            .WithMinWidth(150)
            .WithMargin(0, 0, 0, 10);
        var buttonCount = UiUtil.MakeButton(Se.Language.General.Count, vm.CountCommand)
            .WithLeftAlignment()
            .WithMinWidth(150)
            .WithMargin(0, 0, 0, 10);

        var textBlockCountResult = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.CountResult)) { Mode = BindingMode.OneWay },
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0)
        };

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                buttonFindNext,
                buttonFindPrevious,
                buttonCount,
                textBlockCountResult
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelFind, 0, 0);
        grid.Add(checkBoxWholeWord, 1, 0);
        grid.Add(panelFindTypes, 2, 0);
        grid.Add(panelButtons, 0, 1, 3, 1);

        Content = grid;

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
}
