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
        }.WithAccessibleName(Se.Language.General.Find); // AutoCompleteBox has no watermark-derived name (#12087)
        textBoxFind.KeyDown += vm.FindTextBoxKeyDown;

        var buttonHistory = FindWindowParts.MakeHistoryButton(vm.SearchHistory, vm.ShowHistoryCommand);
        var panelFind = FindWindowParts.MakeSearchPanel(textBoxFind, buttonHistory);

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
            Content = Se.Language.General.CaseSensitive,
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
            Content = Se.Language.General.CaseInsensitive,
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
            .WithIconLeft(IconNames.ChevronLeft)
            .WithLeftAlignment()
            .WithMinWidth(150)
            .WithMargin(0, 0, 0, 10);
        var buttonFindNext = UiUtil.MakeButton(Se.Language.Edit.Find.FindNext, vm.FindNextCommand)
            .WithIconLeft(IconNames.ChevronRight)
            .WithLeftAlignment()
            .WithMinWidth(150)
            .WithMargin(0, 0, 0, 10);
        var buttonCount = UiUtil.MakeButton(Se.Language.General.Count, vm.CountCommand)
            .WithIconLeft(IconNames.Counter)
            .WithLeftAlignment()
            .WithMinWidth(150)
            .WithMargin(0, 0, 0, 10);

        var panelResult = FindWindowParts.MakeResultPanel(nameof(vm.CountResult), nameof(vm.ResultIcon));

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
                panelResult
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
