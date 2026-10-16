using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Collections;

namespace Nikse.SubtitleEdit.Features.Tools.BridgeGaps;

/// <summary>
/// Bridge gaps, in the editor look (see <see cref="EditorChrome"/>): the limits on top, the
/// preview of the bridged gaps below them, the count and Cancel/OK in the button bar.
/// </summary>
public class BridgeGapsWindow : Window
{
    public BridgeGapsWindow(BridgeGapsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.BridgeGaps);
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 900;
        MinHeight = 500;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        var numericUpDownBridgeGapSmallerThan = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(1, 10000, vm.BridgeGapsSmallerThanMsOrFrames, 90, vm, nameof(vm.BridgeGapsSmallerThanMsOrFrames)), vm.BridgeGapsSmallerThanLabel, 90, center: true);
        numericUpDownBridgeGapSmallerThan.ValueChanged += vm.ValueChanged;
        var buttonCalculateBridgeGapSmallerThan = EditorChrome.CreateToolButton("fa-solid fa-calculator",
                Se.Language.Tools.BridgeGaps.CalculateBridgeGapsSmallerThanDotDotDot, vm.CalculateBridgeGapsSmallerThanMsCommand, 28)
            .WithBindIsVisible(vm, nameof(vm.IsMsMode));

        var numericUpDownMinGap = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(0, 1000, vm.MinGapMsOrFrames, 90, vm, nameof(vm.MinGapMsOrFrames)), vm.MinGapLabel, 90, center: true);
        numericUpDownMinGap.ValueChanged += vm.ValueChanged;
        var buttonCalculateMinGap = EditorChrome.CreateToolButton("fa-solid fa-calculator",
                Se.Language.Options.Settings.MinGapCalculateDotDotDot, vm.CalculateMinGapMsCommand, 28)
            .WithBindIsVisible(vm, nameof(vm.IsMsMode));

        var numericUpDownPercentForLeft = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(0, 100, Se.Settings.Tools.BridgeGaps.PercentForLeft, 90, vm, nameof(vm.PercentForLeft)), Se.Language.Tools.BridgeGaps.PercentFoPrevious, 90, center: true);
        numericUpDownPercentForLeft.ValueChanged += vm.ValueChanged;

        var panelControls = new WrapPanel { Orientation = Orientation.Horizontal, ItemSpacing = 28, LineSpacing = 8 };
        panelControls.Children.Add(EditorChrome.MakeLabeledField(vm.BridgeGapsSmallerThanLabel, numericUpDownBridgeGapSmallerThan, buttonCalculateBridgeGapSmallerThan));
        panelControls.Children.Add(EditorChrome.MakeLabeledField(vm.MinGapLabel, numericUpDownMinGap, buttonCalculateMinGap));
        panelControls.Children.Add(EditorChrome.MakeLabeledField(Se.Language.Tools.BridgeGaps.PercentFoPrevious, numericUpDownPercentForLeft));
        var optionsSection = EditorChrome.MakeEditorSection(Se.Language.General.Options, panelControls, first: true);

        var listSection = EditorChrome.MakeFillSection(EditorChrome.MakeSectionHeader(Se.Language.General.Preview), EditorChrome.MakeListFrame(MakeSubtitleView(vm)));

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // Bridge gap smaller than
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Subtitle view
            },
        };
        sections.Add(optionsSection, 0);
        sections.Add(listSection, 1);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(EditorChrome.MakeBoundHint(nameof(vm.StatusText), 12), buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, numericUpDownBridgeGapSmallerThan); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Control MakeSubtitleView(BridgeGapsViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        // No header-click sorting (the DataGrid's CanUserSortColumns is not carried
        // over): gap-change previews in subtitle order.
        var dataGridSubtitle = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridSubtitle.Width = double.NaN;
        dataGridSubtitle.Height = double.NaN;
        dataGridSubtitle.DataContext = vm;
        dataGridSubtitle.ItemsSource = vm.Subtitles;
        dataGridSubtitle.Columns.AddRange(new[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(BridgeGapDisplayItem.Number)),
                Width = new GridLength(60), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Show,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(BridgeGapDisplayItem.StartTime)) { Converter = fullTimeConverter },
                Width = new GridLength(115),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Duration,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(BridgeGapDisplayItem.Duration)) { Converter = shortTimeConverter },
                Width = new GridLength(90),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Text,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(BridgeGapDisplayItem.Text)),
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.Tools.BridgeGaps.GapChange,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(BridgeGapDisplayItem.InfoText)),
                Width = new GridLength(120),
            },
        });

        dataGridSubtitle.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGridSubtitle.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                if (target == null)
                {
                    return;
                }

                dataGridSubtitle.SelectedItem = target;
                dataGridSubtitle.ScrollIntoView(target);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        return dataGridSubtitle;
    }
}
