using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

/// <summary>
/// Shows what post-processing found in a fresh transcription (issue #13973):
/// a header with the verdict, one summary card per issue type (click to
/// filter), and the affected lines in a table.
/// </summary>
public class SpeechToTextQualityReportWindow : Window
{
    private readonly SpeechToTextQualityReportViewModel _vm;

    public SpeechToTextQualityReportWindow(SpeechToTextQualityReportViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AudioToText.QualityReportTitle;
        Width = 960;
        Height = 640;
        MinWidth = 720;
        MinHeight = 460;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Video.AudioToText;

        // Header: title + one-line verdict
        var labelTitle = new TextBlock
        {
            Text = l.QualityReportTitle,
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };
        var labelSummary = new TextBlock
        {
            FontSize = 14,
            Opacity = 0.75,
            Margin = new Thickness(0, 4, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Summary)),
        };
        var header = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 0, 0, 12),
            Children = { labelTitle, labelSummary },
        };

        // Summary cards - one per issue type, acting as filter buttons
        var cards = new ItemsControl
        {
            ItemsSource = vm.Cards,
            Margin = new Thickness(0, 0, 0, 12),
            ItemsPanel = new FuncTemplate<Panel?>(() => new WrapPanel { Orientation = Orientation.Horizontal, ItemSpacing = 8, LineSpacing = 8 }),
            ItemTemplate = new FuncDataTemplate<QualityReportCard>((card, _) => MakeCard(card)),
        };

        // Issue table
        var table = TableViewExtras.MakeTableView(alwaysSelected: false, multiSelect: false);
        table[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.Items));
        table[!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedItem)) { Mode = BindingMode.TwoWay };
        table.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(QualityReportDisplayItem.Number)),
            Width = new GridLength(60),
        });
        table.Columns.Add(new SeTableViewColumn
        {
            Header = l.QualityReportIssue,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<QualityReportDisplayItem>((item, _) =>
            {
                var dot = new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = 9,
                    Height = 9,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!Avalonia.Controls.Shapes.Shape.FillProperty] = new Binding(nameof(QualityReportDisplayItem.Brush)),
                };
                var text = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(QualityReportDisplayItem.Category)),
                };
                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Margin = new Thickness(6, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Children = { dot, text },
                };
            }),
            Width = new GridLength(170),
        });
        table.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(QualityReportDisplayItem.Start)),
            Width = new GridLength(105),
        });
        table.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Hide,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(QualityReportDisplayItem.End)),
            Width = new GridLength(105),
        });
        table.Columns.Add(new SeTableViewColumn
        {
            Header = l.QualityReportDetail,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(QualityReportDisplayItem.Detail)),
            Width = new GridLength(120),
        });
        table.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(QualityReportDisplayItem.Text)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        AutomationProperties.SetName(table, l.QualityReportTitle);
        var tableBorder = UiUtil.MakeBorderForControlNoPadding(table);

        // Footer: tip, "do not show again", OK
        var labelTip = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.7,
            Margin = new Thickness(0, 10, 0, 10),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Tip)),
        };

        var checkDoNotShowAgain = UiUtil.MakeCheckBox(l.QualityReportDoNotShowAgain, vm, nameof(vm.DoNotShowAgain));
        checkDoNotShowAgain.VerticalAlignment = VerticalAlignment.Center;
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var footer = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        footer.Add(checkDoNotShowAgain, 0, 0);
        footer.Add(UiUtil.MakeButtonBar(buttonOk), 0, 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
        };
        grid.Add(header, 0, 0);
        grid.Add(cards, 1, 0);
        grid.Add(tableBorder, 2, 0);
        grid.Add(labelTip, 3, 0);
        grid.Add(footer, 4, 0);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private Control MakeCard(QualityReportCard? card)
    {
        if (card == null)
        {
            return new Border();
        }

        var accent = new Border
        {
            Width = 5,
            CornerRadius = new CornerRadius(3),
            Background = card.Brush,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        var count = new TextBlock
        {
            Text = card.Count.ToString(),
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = card.Brush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var label = new TextBlock
        {
            Text = card.Label,
            FontSize = 12,
            Opacity = 0.85,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var texts = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { count, label },
        };
        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { accent, texts },
        };

        // A bordered button rather than a ToggleButton: the Fluent checked state
        // floods the card with the accent color and drowns the colored count.
        var border = new Border
        {
            Child = content,
            Padding = new Thickness(12, 8),
            MinWidth = 120,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(UiUtil.CornerRadius + 2),
            [!Border.BorderBrushProperty] = new Binding(nameof(QualityReportCard.BorderBrush)),
        };

        var button = new Button
        {
            Content = border,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(UiUtil.CornerRadius + 2),
            Command = _vm.SetFilterCommand,
            CommandParameter = card,
            Opacity = card.Count == 0 && card.Type != null ? 0.5 : 1.0,
        };
        AutomationProperties.SetName(button, $"{card.Label}: {card.Count}");
        if (Se.Settings.Appearance.ShowHints && !string.IsNullOrEmpty(card.Hint))
        {
            ToolTip.SetTip(button, card.Hint);
        }

        return button;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
