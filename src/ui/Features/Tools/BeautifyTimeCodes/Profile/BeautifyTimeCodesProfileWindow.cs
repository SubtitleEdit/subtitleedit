using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Config.Language.Tools;
using Optris.Icons.Avalonia;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes.Profile;

public class BeautifyTimeCodesProfileWindow : Window
{
    private readonly LanguageBeautifyTimeCodesProfile _l;
    private readonly BeautifyTimeCodesProfileViewModel _vm;

    private CuesPreviewControl _previewIn = null!;
    private CuesPreviewControl _previewOut = null!;
    private CuesPreviewControl _previewConnectedIn = null!;
    private CuesPreviewControl _previewConnectedOut = null!;
    private CuesPreviewControl _previewChainGeneral = null!;
    private CuesPreviewControl _previewChainInOnShot = null!;
    private CuesPreviewControl _previewChainOutOnShot = null!;

    public BeautifyTimeCodesProfileWindow(BeautifyTimeCodesProfileViewModel vm)
    {
        _vm = vm;
        _l = Se.Language.Tools.BeautifyTimeCodesProfile;

        UiUtil.InitializeWindow(this, GetType().Name);
        Title = _l.Title;
        CanResize = true;
        Width = 1100;
        Height = 760;
        MinWidth = 980;
        MinHeight = 640;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Margin = UiUtil.MakeWindowMargin(),
        };

        grid.Add(BuildGeneralSection(), 0, 0, 1, 2);
        grid.Add(BuildInCuesGroup(), 1, 0);
        grid.Add(BuildOutCuesGroup(), 1, 1);

        var bottomGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 10,
        };
        bottomGrid.Add(BuildConnectedGroup(), 0, 0);
        bottomGrid.Add(BuildChainingGroup(), 0, 1);
        grid.Add(bottomGrid, 2, 0, 1, 2);

        var presetButton = BuildPresetButton();
        var ok = UiUtil.MakeButtonOk(vm.OkCommand);
        var cancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(ok, cancel);
        buttonBar.Children.Insert(0, presetButton);

        var outerGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
        };
        outerGrid.Add(new ScrollViewer { Content = grid, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }, 0, 0);
        outerGrid.Add(buttonBar, 1, 0);
        Content = outerGrid;

        Activated += delegate { ok.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);

        // Push initial values into the preview controls once they're attached
        Loaded += (_, _) => RefreshAllPreviews();
        vm.PropertyChanged += (_, _) => RefreshAllPreviews();
    }

    private void RefreshAllPreviews()
    {
        if (_previewIn == null)
        {
            return;
        }

        var fr = _vm.FrameRate;

        _previewIn.FrameRate = fr;
        _previewIn.RightGap = _vm.InCuesGap;
        _previewIn.LeftGap = 0;
        _previewIn.LeftGreenZone = _vm.InCuesLeftGreenZone;
        _previewIn.LeftRedZone = _vm.InCuesLeftRedZone;
        _previewIn.RightRedZone = _vm.InCuesRightRedZone;
        _previewIn.RightGreenZone = _vm.InCuesRightGreenZone;
        _previewIn.PreviewText = _l.SubtitlePreviewText;

        _previewOut.FrameRate = fr;
        _previewOut.LeftGap = _vm.OutCuesGap;
        _previewOut.RightGap = 0;
        _previewOut.LeftGreenZone = _vm.OutCuesLeftGreenZone;
        _previewOut.LeftRedZone = _vm.OutCuesLeftRedZone;
        _previewOut.RightRedZone = _vm.OutCuesRightRedZone;
        _previewOut.RightGreenZone = _vm.OutCuesRightGreenZone;
        _previewOut.PreviewText = _l.SubtitlePreviewText;

        _previewConnectedIn.FrameRate = fr;
        _previewConnectedIn.LeftGap = _vm.ConnectedInCueClosestLeftGap;
        _previewConnectedIn.RightGap = _vm.ConnectedInCueClosestRightGap;
        _previewConnectedIn.LeftGreenZone = _vm.ConnectedLeftGreenZone;
        _previewConnectedIn.LeftRedZone = _vm.ConnectedLeftRedZone;
        _previewConnectedIn.RightRedZone = _vm.ConnectedRightRedZone;
        _previewConnectedIn.RightGreenZone = _vm.ConnectedRightGreenZone;
        _previewConnectedIn.PreviewText = _l.SubtitlePreviewText;

        _previewConnectedOut.FrameRate = fr;
        _previewConnectedOut.LeftGap = _vm.ConnectedOutCueClosestLeftGap;
        _previewConnectedOut.RightGap = _vm.ConnectedOutCueClosestRightGap;
        _previewConnectedOut.LeftGreenZone = _vm.ConnectedLeftGreenZone;
        _previewConnectedOut.LeftRedZone = _vm.ConnectedLeftRedZone;
        _previewConnectedOut.RightRedZone = _vm.ConnectedRightRedZone;
        _previewConnectedOut.RightGreenZone = _vm.ConnectedRightGreenZone;
        _previewConnectedOut.PreviewText = _l.SubtitlePreviewText;

        _previewChainGeneral.FrameRate = fr;
        _previewChainGeneral.LeftGap = 0;
        _previewChainGeneral.RightGap = 0;
        _previewChainGeneral.LeftGreenZone = _vm.ChainingGeneralUseZones ? _vm.ChainingGeneralLeftGreenZone : 0;
        _previewChainGeneral.LeftRedZone = _vm.ChainingGeneralUseZones ? _vm.ChainingGeneralLeftRedZone : 0;
        _previewChainGeneral.RightRedZone = 0;
        _previewChainGeneral.RightGreenZone = 0;
        _previewChainGeneral.PreviewText = _l.SubtitlePreviewText;

        _previewChainInOnShot.FrameRate = fr;
        _previewChainInOnShot.LeftGap = 0;
        _previewChainInOnShot.RightGap = _vm.InCuesGap;
        _previewChainInOnShot.LeftGreenZone = _vm.ChainingInOnShotUseZones ? _vm.ChainingInOnShotLeftGreenZone : 0;
        _previewChainInOnShot.LeftRedZone = _vm.ChainingInOnShotUseZones ? _vm.ChainingInOnShotLeftRedZone : 0;
        _previewChainInOnShot.RightRedZone = 0;
        _previewChainInOnShot.RightGreenZone = 0;
        _previewChainInOnShot.PreviewText = _l.SubtitlePreviewText;

        _previewChainOutOnShot.FrameRate = fr;
        _previewChainOutOnShot.LeftGap = _vm.OutCuesGap;
        _previewChainOutOnShot.RightGap = 0;
        _previewChainOutOnShot.LeftGreenZone = 0;
        _previewChainOutOnShot.LeftRedZone = 0;
        _previewChainOutOnShot.RightRedZone = _vm.ChainingOutOnShotUseZones ? _vm.ChainingOutOnShotRightRedZone : 0;
        _previewChainOutOnShot.RightGreenZone = _vm.ChainingOutOnShotUseZones ? _vm.ChainingOutOnShotRightGreenZone : 0;
        _previewChainOutOnShot.PreviewText = _l.SubtitlePreviewText;
    }

    private Control BuildPresetButton()
    {
        return new SplitButton
        {
            Content = _l.LoadPreset,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 20, 0),
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new Avalonia.Controls.MenuItem { Header = _l.PresetDefault, Command = _vm.LoadPresetDefaultCommand },
                    new Avalonia.Controls.MenuItem { Header = _l.PresetNetflix, Command = _vm.LoadPresetNetflixCommand },
                    new Avalonia.Controls.MenuItem { Header = _l.PresetSdi, Command = _vm.LoadPresetSdiCommand },
                },
            },
        };
    }

    private Control BuildGeneralSection()
    {
        var labelGap = UiUtil.MakeLabel(_l.Gap + ":");
        var nudGap = MakeFrameNud(nameof(_vm.Gap));
        var suffix = UiUtil.MakeLabel(_l.GapSuffix);

        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { labelGap, nudGap, suffix, MakeHintIcon(_l.HintGap) },
        };

        return MakeGroupBox(_l.General, content);
    }

    private Control BuildInCuesGroup()
    {
        _previewIn = new CuesPreviewControl();

        var rowGap = MakeRowLabelNudSuffix(_l.Gap, nameof(_vm.InCuesGap), _l.Frames);
        var rowZones = MakeZonesRow(
            nameof(_vm.InCuesLeftGreenZone), nameof(_vm.InCuesLeftRedZone),
            nameof(_vm.InCuesRightRedZone), nameof(_vm.InCuesRightGreenZone));

        var stack = new StackPanel
        {
            Spacing = 6,
            Children = { _previewIn, rowGap, rowZones },
        };

        return MakeGroupBox(_l.InCues, stack);
    }

    private Control BuildOutCuesGroup()
    {
        _previewOut = new CuesPreviewControl();

        var rowGap = MakeRowLabelNudSuffix(_l.Gap, nameof(_vm.OutCuesGap), _l.Frames);
        var rowZones = MakeZonesRow(
            nameof(_vm.OutCuesLeftGreenZone), nameof(_vm.OutCuesLeftRedZone),
            nameof(_vm.OutCuesRightRedZone), nameof(_vm.OutCuesRightGreenZone));

        var stack = new StackPanel
        {
            Spacing = 6,
            Children = { _previewOut, rowGap, rowZones },
        };

        return MakeGroupBox(_l.OutCues, stack);
    }

    private Control BuildConnectedGroup()
    {
        _previewConnectedIn = new CuesPreviewControl();
        _previewConnectedOut = new CuesPreviewControl { IsVisible = false };

        var previewHost = new Grid();
        previewHost.Children.Add(_previewConnectedIn);
        previewHost.Children.Add(_previewConnectedOut);

        var tabs = new TabControl
        {
            [!SelectingItemsControl.SelectedIndexProperty] = new Binding(nameof(_vm.SelectedConnectedTabIndex)) { Source = _vm, Mode = BindingMode.TwoWay },
        };
        tabs.Items.Add(new TabItem { Header = MakeTabHeader(_l.InCueClosest) });
        tabs.Items.Add(new TabItem { Header = MakeTabHeader(_l.OutCueClosest) });
        tabs.SelectionChanged += (_, _) =>
        {
            _previewConnectedIn.IsVisible = _vm.SelectedConnectedTabIndex == 0;
            _previewConnectedOut.IsVisible = _vm.SelectedConnectedTabIndex == 1;
        };

        // Gap row — two NUDs depending on selected tab
        var labelGap = UiUtil.MakeLabel(_l.Gap + ":");
        var nudGapInLeft = MakeFrameNud(nameof(_vm.ConnectedInCueClosestLeftGap));
        var nudGapInRight = MakeFrameNud(nameof(_vm.ConnectedInCueClosestRightGap));
        var nudGapOutLeft = MakeFrameNud(nameof(_vm.ConnectedOutCueClosestLeftGap));
        var nudGapOutRight = MakeFrameNud(nameof(_vm.ConnectedOutCueClosestRightGap));

        var conv = new IndexEqualsConverter();
        nudGapInLeft.Bind(IsVisibleProperty, new Binding(nameof(_vm.SelectedConnectedTabIndex)) { Source = _vm, Converter = conv, ConverterParameter = 0 });
        nudGapInRight.Bind(IsVisibleProperty, new Binding(nameof(_vm.SelectedConnectedTabIndex)) { Source = _vm, Converter = conv, ConverterParameter = 0 });
        nudGapOutLeft.Bind(IsVisibleProperty, new Binding(nameof(_vm.SelectedConnectedTabIndex)) { Source = _vm, Converter = conv, ConverterParameter = 1 });
        nudGapOutRight.Bind(IsVisibleProperty, new Binding(nameof(_vm.SelectedConnectedTabIndex)) { Source = _vm, Converter = conv, ConverterParameter = 1 });

        var rowGap = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { labelGap, nudGapInLeft, nudGapInRight, nudGapOutLeft, nudGapOutRight, MakeHintIcon(_l.HintConnected) },
        };

        var rowZones = MakeZonesRow(
            nameof(_vm.ConnectedLeftGreenZone), nameof(_vm.ConnectedLeftRedZone),
            nameof(_vm.ConnectedRightRedZone), nameof(_vm.ConnectedRightGreenZone));

        var rowTreatConnected = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                UiUtil.MakeLabel(_l.TreadAsConnected),
                MakeMsNud(nameof(_vm.ConnectedTreatConnectedMs)),
                UiUtil.MakeLabel(_l.Milliseconds),
            }
        };

        var stack = new StackPanel
        {
            Spacing = 6,
            Children = { previewHost, tabs, rowGap, rowZones, rowTreatConnected },
        };

        return MakeGroupBox(_l.ConnectedSubtitles, stack);
    }

    private Control BuildChainingGroup()
    {
        _previewChainGeneral = new CuesPreviewControl();
        _previewChainInOnShot = new CuesPreviewControl { IsVisible = false };
        _previewChainOutOnShot = new CuesPreviewControl { IsVisible = false };

        var previewHost = new Grid();
        previewHost.Children.Add(_previewChainGeneral);
        previewHost.Children.Add(_previewChainInOnShot);
        previewHost.Children.Add(_previewChainOutOnShot);

        var tabs = new TabControl
        {
            [!SelectingItemsControl.SelectedIndexProperty] = new Binding(nameof(_vm.SelectedChainingTabIndex)) { Source = _vm, Mode = BindingMode.TwoWay },
        };
        tabs.Items.Add(new TabItem { Header = MakeTabHeader(_l.General) });
        tabs.Items.Add(new TabItem { Header = MakeTabHeader(_l.InCueOnShot) });
        tabs.Items.Add(new TabItem { Header = MakeTabHeader(_l.OutCueOnShot) });
        tabs.SelectionChanged += (_, _) =>
        {
            _previewChainGeneral.IsVisible = _vm.SelectedChainingTabIndex == 0;
            _previewChainInOnShot.IsVisible = _vm.SelectedChainingTabIndex == 1;
            _previewChainOutOnShot.IsVisible = _vm.SelectedChainingTabIndex == 2;
        };

        var conv = new IndexEqualsConverter();

        var generalPanel = BuildChainingSubPanel(
            useZonesPath: nameof(_vm.ChainingGeneralUseZones),
            useMaxGapPath: nameof(_vm.ChainingGeneralUseMaxGap),
            maxGapPath: nameof(_vm.ChainingGeneralMaxGapMs),
            leftGreenPath: nameof(_vm.ChainingGeneralLeftGreenZone),
            leftRedPath: nameof(_vm.ChainingGeneralLeftRedZone),
            rightRedPath: null,
            rightGreenPath: null,
            checkGeneralPath: null,
            includeShotChangeBehavior: true);
        generalPanel.Bind(IsVisibleProperty, new Binding(nameof(_vm.SelectedChainingTabIndex)) { Source = _vm, Converter = conv, ConverterParameter = 0 });

        var inPanel = BuildChainingSubPanel(
            useZonesPath: nameof(_vm.ChainingInOnShotUseZones),
            useMaxGapPath: nameof(_vm.ChainingInOnShotUseMaxGap),
            maxGapPath: nameof(_vm.ChainingInOnShotMaxGapMs),
            leftGreenPath: nameof(_vm.ChainingInOnShotLeftGreenZone),
            leftRedPath: nameof(_vm.ChainingInOnShotLeftRedZone),
            rightRedPath: null,
            rightGreenPath: null,
            checkGeneralPath: nameof(_vm.ChainingInOnShotCheckGeneral),
            includeShotChangeBehavior: false);
        inPanel.Bind(IsVisibleProperty, new Binding(nameof(_vm.SelectedChainingTabIndex)) { Source = _vm, Converter = conv, ConverterParameter = 1 });

        var outPanel = BuildChainingSubPanel(
            useZonesPath: nameof(_vm.ChainingOutOnShotUseZones),
            useMaxGapPath: nameof(_vm.ChainingOutOnShotUseMaxGap),
            maxGapPath: nameof(_vm.ChainingOutOnShotMaxGapMs),
            leftGreenPath: null,
            leftRedPath: null,
            rightRedPath: nameof(_vm.ChainingOutOnShotRightRedZone),
            rightGreenPath: nameof(_vm.ChainingOutOnShotRightGreenZone),
            checkGeneralPath: nameof(_vm.ChainingOutOnShotCheckGeneral),
            includeShotChangeBehavior: false);
        outPanel.Bind(IsVisibleProperty, new Binding(nameof(_vm.SelectedChainingTabIndex)) { Source = _vm, Converter = conv, ConverterParameter = 2 });

        var hostPanel = new Grid();
        hostPanel.Children.Add(generalPanel);
        hostPanel.Children.Add(inPanel);
        hostPanel.Children.Add(outPanel);

        var stack = new StackPanel
        {
            Spacing = 6,
            Children = { previewHost, tabs, hostPanel },
        };

        return MakeGroupBox(_l.Chaining, stack);
    }

    private Control BuildChainingSubPanel(
        string useZonesPath, string useMaxGapPath, string maxGapPath,
        string? leftGreenPath, string? leftRedPath,
        string? rightRedPath, string? rightGreenPath,
        string? checkGeneralPath, bool includeShotChangeBehavior)
    {
        var radioMax = new RadioButton
        {
            GroupName = "chaining_" + useZonesPath,
            Content = _l.MaxGap,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(useMaxGapPath) { Source = _vm, Mode = BindingMode.TwoWay },
        };
        var nudMax = MakeMsNud(maxGapPath);
        nudMax.Bind(IsEnabledProperty, new Binding(useMaxGapPath) { Source = _vm });
        var maxRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { radioMax, nudMax, UiUtil.MakeLabel(_l.Milliseconds) },
        };

        var radioZones = new RadioButton
        {
            GroupName = "chaining_" + useZonesPath,
            Content = _l.Zones,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(useZonesPath) { Source = _vm, Mode = BindingMode.TwoWay },
        };

        var nudFields = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        if (leftGreenPath != null) nudFields.Children.Add(MakeFrameNud(leftGreenPath));
        if (leftRedPath != null) nudFields.Children.Add(MakeFrameNud(leftRedPath));
        if (rightRedPath != null) nudFields.Children.Add(MakeFrameNud(rightRedPath));
        if (rightGreenPath != null) nudFields.Children.Add(MakeFrameNud(rightGreenPath));
        nudFields.Bind(IsEnabledProperty, new Binding(useZonesPath) { Source = _vm });
        var zonesRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { radioZones, nudFields },
        };

        var stack = new StackPanel { Spacing = 6 };
        stack.Children.Add(maxRow);
        stack.Children.Add(zonesRow);

        if (includeShotChangeBehavior)
        {
            var labelBehavior = UiUtil.MakeLabel(_l.ShotChangeBehavior);
            var combo = new ComboBox
            {
                Width = 220,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.ShotChangeBehaviors)) { Source = _vm },
                [!SelectingItemsControl.SelectedIndexProperty] = new Binding(nameof(_vm.SelectedChainingShotChangeBehaviorIndex)) { Source = _vm, Mode = BindingMode.TwoWay },
            };
            var behaviorRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children = { labelBehavior, combo, MakeHintIcon(_l.HintChaining) },
            };
            stack.Children.Add(behaviorRow);
        }

        if (checkGeneralPath != null)
        {
            var cb = new CheckBox
            {
                Content = _l.CheckGeneral,
                [!ToggleButton.IsCheckedProperty] = new Binding(checkGeneralPath) { Source = _vm, Mode = BindingMode.TwoWay },
            };
            stack.Children.Add(cb);
        }

        return stack;
    }

    private Control MakeRowLabelNudSuffix(string label, string bindingPath, string suffix)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                UiUtil.MakeLabel(label + ":"),
                MakeFrameNud(bindingPath),
                UiUtil.MakeLabel(suffix),
            },
        };
    }

    private Control MakeZonesRow(string leftGreenPath, string leftRedPath, string rightRedPath, string rightGreenPath)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                UiUtil.MakeLabel(_l.Zones + ":"),
                MakeZoneNud(leftGreenPath, isGreen: true),
                MakeZoneNud(leftRedPath, isGreen: false),
                MakeZoneNud(rightRedPath, isGreen: false),
                MakeZoneNud(rightGreenPath, isGreen: true),
                MakeHintIcon(_l.HintZones),
            },
        };
    }

    private static TextBlock MakeTabHeader(string text) => new()
    {
        Text = text,
        FontSize = 12,
    };

    private NumericUpDown MakeFrameNud(string bindingPath)
    {
        return new NumericUpDown
        {
            Width = 100,
            Minimum = 0,
            Maximum = 100,
            Increment = 1,
            FormatString = "F0",
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(bindingPath) { Source = _vm, Mode = BindingMode.TwoWay, Converter = new IntDecimalConverter() },
        };
    }

    private NumericUpDown MakeMsNud(string bindingPath)
    {
        return new NumericUpDown
        {
            Width = 130,
            Minimum = 0,
            Maximum = 5000,
            Increment = 10,
            FormatString = "F0",
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(bindingPath) { Source = _vm, Mode = BindingMode.TwoWay, Converter = new IntDecimalConverter() },
        };
    }

    private NumericUpDown MakeZoneNud(string bindingPath, bool isGreen)
    {
        var nud = MakeFrameNud(bindingPath);
        nud.Background = new SolidColorBrush(isGreen ? Color.FromArgb(80, 0, 160, 0) : Color.FromArgb(80, 200, 30, 30));
        nud.Width = 95;
        return nud;
    }

    private Control MakeHintIcon(string tip)
    {
        var icon = new ContentControl
        {
            Width = 16,
            Height = 16,
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.7,
            [ToolTip.TipProperty] = tip,
        };
        Attached.SetIcon(icon, IconNames.Information);
        return icon;
    }

    private static Control MakeGroupBox(string title, Control content)
    {
        var header = new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 6),
        };
        var inner = new StackPanel
        {
            Spacing = 4,
            Children = { header, content },
        };
        return UiUtil.MakeBorderForControl(inner);
    }

    private sealed class IndexEqualsConverter : IValueConverter
    {
        public object? Convert(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int i && parameter is int p) return i == p;
            if (value is int i2 && int.TryParse(parameter?.ToString(), out var pi)) return i2 == pi;
            return false;
        }

        public object? ConvertBack(object? value, System.Type targetType, object? parameter, CultureInfo culture) => null;
    }

    private sealed class IntDecimalConverter : IValueConverter
    {
        public object? Convert(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            return value is int i ? (decimal?)i : null;
        }

        public object? ConvertBack(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            return value is decimal d ? (int)d : 0;
        }
    }
}
