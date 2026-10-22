using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Config.Language.Tools;
using System;
using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes.Profile;

/// <summary>
/// The beautify time codes profile, in the editor look (see <see cref="EditorChrome"/>): the base
/// gap on top, in/out cues and connected/chaining side by side in sections (each with its cue
/// preview), the presets plus Cancel/OK in the button bar. The sections scroll on a short screen.
/// The in/out and general/in/out pages of the connected and chaining sections are segmented
/// buttons bound to the same view model indices the tab controls used.
/// </summary>
public class BeautifyTimeCodesProfileWindow : Window
{
    private const double LabelWidth = 80;

    private static readonly IBrush GreenZoneBrush = new SolidColorBrush(Color.FromRgb(0, 160, 0));
    private static readonly IBrush RedZoneBrush = new SolidColorBrush(Color.FromRgb(200, 30, 30));

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
        Width = 1080;
        Height = 640;
        MinWidth = 960;
        MinHeight = 520;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        var sections = new StackPanel();
        sections.Children.Add(BuildGeneralSection());
        sections.Children.Add(SideBySide(BuildInCuesGroup(), BuildOutCuesGroup()));
        sections.Children.Add(SideBySide(BuildConnectedGroup(), BuildChainingGroup()));

        // Keeps the buttons reachable when the window is clamped to a small working area.
        var scrollViewer = new ScrollViewer
        {
            Content = sections,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var presetButton = BuildPresetButton();
        var ok = UiUtil.MakeButtonOk(vm.OkCommand);
        ok.Classes.Add("primary");
        var cancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(scrollViewer, EditorChrome.MakeDockedButtonBar(presetButton, cancel, ok));

        UiUtil.FocusOnFirstActivation(this, ok);
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
        var flyout = new MenuFlyout();
        flyout.Opening += (_, _) => FillPresetMenu(flyout);
        FillPresetMenu(flyout);

        return new SeSplitButton
        {
            Content = _l.LoadPreset,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Flyout = flyout,
        };
    }

    // Rebuilt on every open: saved profiles can be added or deleted while the window is up (#11541).
    private void FillPresetMenu(MenuFlyout flyout)
    {
        flyout.Items.Clear();
        flyout.Items.Add(new Avalonia.Controls.MenuItem { Header = _l.PresetDefault, Command = _vm.LoadPresetDefaultCommand });
        flyout.Items.Add(new Avalonia.Controls.MenuItem { Header = _l.PresetNetflix, Command = _vm.LoadPresetNetflixCommand });
        flyout.Items.Add(new Avalonia.Controls.MenuItem { Header = _l.PresetSdi, Command = _vm.LoadPresetSdiCommand });

        var customProfiles = _vm.CustomProfiles.ToList();
        if (customProfiles.Count > 0)
        {
            flyout.Items.Add(new Separator());
            foreach (var customProfile in customProfiles)
            {
                var item = new Avalonia.Controls.MenuItem { Header = customProfile.Name };
                item.Click += (_, _) => _vm.LoadCustomProfile(customProfile);
                flyout.Items.Add(item);
            }
        }

        flyout.Items.Add(new Separator());
        flyout.Items.Add(new Avalonia.Controls.MenuItem { Header = _l.SaveAsProfile, Command = _vm.SaveAsProfileCommand });

        if (customProfiles.Count > 0)
        {
            var deleteMenu = new Avalonia.Controls.MenuItem { Header = _l.DeleteProfile };
            foreach (var customProfile in customProfiles)
            {
                var item = new Avalonia.Controls.MenuItem { Header = customProfile.Name };
                item.Click += async (_, _) => await _vm.DeleteCustomProfile(customProfile);
                deleteMenu.Items.Add(item);
            }

            flyout.Items.Add(deleteMenu);
        }
    }

    private Control BuildGeneralSection()
    {
        var nudGap = MakeFrameNud(nameof(_vm.Gap), _l.Gap);
        var content = Row(_l.Gap, nudGap, MakeUnit(_l.GapSuffix), UiUtil.MakeHintIcon(_l.HintGap, nudGap));
        return EditorChrome.MakeEditorSection(Se.Language.General.General, content, first: true);
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
            Spacing = 8,
            Children = { FramePreview(_previewIn), rowGap, rowZones },
        };

        return EditorChrome.MakeEditorSection(_l.InCues, stack);
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
            Spacing = 8,
            Children = { FramePreview(_previewOut), rowGap, rowZones },
        };

        return EditorChrome.MakeEditorSection(_l.OutCues, stack);
    }

    private Control BuildConnectedGroup()
    {
        _previewConnectedIn = new CuesPreviewControl();
        _previewConnectedOut = new CuesPreviewControl { IsVisible = false };

        var previewHost = new Grid();
        previewHost.Children.Add(_previewConnectedIn);
        previewHost.Children.Add(_previewConnectedOut);

        var pages = MakeSegmented(
            new[] { _l.InCueClosest, _l.OutCueClosest },
            nameof(_vm.SelectedConnectedTabIndex),
            () => _vm.SelectedConnectedTabIndex,
            index => _vm.SelectedConnectedTabIndex = index,
            () =>
            {
                _previewConnectedIn.IsVisible = _vm.SelectedConnectedTabIndex == 0;
                _previewConnectedOut.IsVisible = _vm.SelectedConnectedTabIndex == 1;
            });

        // Gap row — two NUDs depending on selected page
        var nudGapInLeft = MakeFrameNud(nameof(_vm.ConnectedInCueClosestLeftGap), _l.Gap);
        var nudGapInRight = MakeFrameNud(nameof(_vm.ConnectedInCueClosestRightGap), _l.Gap);
        var nudGapOutLeft = MakeFrameNud(nameof(_vm.ConnectedOutCueClosestLeftGap), _l.Gap);
        var nudGapOutRight = MakeFrameNud(nameof(_vm.ConnectedOutCueClosestRightGap), _l.Gap);

        var conv = new IndexEqualsConverter();
        nudGapInLeft.Bind(IsVisibleProperty, new Binding(nameof(_vm.SelectedConnectedTabIndex)) { Source = _vm, Converter = conv, ConverterParameter = 0 });
        nudGapInRight.Bind(IsVisibleProperty, new Binding(nameof(_vm.SelectedConnectedTabIndex)) { Source = _vm, Converter = conv, ConverterParameter = 0 });
        nudGapOutLeft.Bind(IsVisibleProperty, new Binding(nameof(_vm.SelectedConnectedTabIndex)) { Source = _vm, Converter = conv, ConverterParameter = 1 });
        nudGapOutRight.Bind(IsVisibleProperty, new Binding(nameof(_vm.SelectedConnectedTabIndex)) { Source = _vm, Converter = conv, ConverterParameter = 1 });

        var rowGap = Row(_l.Gap, nudGapInLeft, nudGapInRight, nudGapOutLeft, nudGapOutRight, MakeUnit(_l.Frames), UiUtil.MakeHintIcon(_l.HintConnected));

        var rowZones = MakeZonesRow(
            nameof(_vm.ConnectedLeftGreenZone), nameof(_vm.ConnectedLeftRedZone),
            nameof(_vm.ConnectedRightRedZone), nameof(_vm.ConnectedRightGreenZone));

        var rowTreatConnected = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeDimLabel(_l.TreatAsConnected, new Thickness(0)),
                MakeMsNud(nameof(_vm.ConnectedTreatConnectedMs), _l.TreatAsConnected),
                MakeUnit(_l.Milliseconds),
            }
        };

        var stack = new StackPanel
        {
            Spacing = 8,
            Children = { FramePreview(previewHost), pages, rowGap, rowZones, rowTreatConnected },
        };

        return EditorChrome.MakeEditorSection(_l.ConnectedSubtitles, stack);
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

        var pages = MakeSegmented(
            new[] { Se.Language.General.General, _l.InCueOnShot, _l.OutCueOnShot },
            nameof(_vm.SelectedChainingTabIndex),
            () => _vm.SelectedChainingTabIndex,
            index => _vm.SelectedChainingTabIndex = index,
            () =>
            {
                _previewChainGeneral.IsVisible = _vm.SelectedChainingTabIndex == 0;
                _previewChainInOnShot.IsVisible = _vm.SelectedChainingTabIndex == 1;
                _previewChainOutOnShot.IsVisible = _vm.SelectedChainingTabIndex == 2;
            });

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
            Spacing = 8,
            Children = { FramePreview(previewHost), pages, hostPanel },
        };

        return EditorChrome.MakeEditorSection(_l.Chaining, stack);
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
            FontSize = EditorChrome.FieldFontSize,
            MinWidth = 110,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(useMaxGapPath) { Source = _vm, Mode = BindingMode.TwoWay },
        };
        var nudMax = MakeMsNud(maxGapPath, _l.MaxGap);
        nudMax.Bind(IsEnabledProperty, new Binding(useMaxGapPath) { Source = _vm });
        var maxRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { radioMax, nudMax, MakeUnit(_l.Milliseconds) },
        };

        var radioZones = new RadioButton
        {
            GroupName = "chaining_" + useZonesPath,
            Content = _l.Zones,
            FontSize = EditorChrome.FieldFontSize,
            MinWidth = 110,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(useZonesPath) { Source = _vm, Mode = BindingMode.TwoWay },
        };

        var nudFields = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        if (leftGreenPath != null)
        {
            nudFields.Children.Add(MakeZoneNud(leftGreenPath, isGreen: true));
        }
        if (leftRedPath != null)
        {
            nudFields.Children.Add(MakeZoneNud(leftRedPath, isGreen: false));
        }
        if (rightRedPath != null)
        {
            nudFields.Children.Add(MakeZoneNud(rightRedPath, isGreen: false));
        }
        if (rightGreenPath != null)
        {
            nudFields.Children.Add(MakeZoneNud(rightGreenPath, isGreen: true));
        }
        // Only the first field would inherit the "Zones" radio as its label; name them all (#12087).
        foreach (var nud in nudFields.Children)
        {
            nud.WithAccessibleName(_l.Zones);
        }
        nudFields.Bind(IsEnabledProperty, new Binding(useZonesPath) { Source = _vm });
        var zonesRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { radioZones, nudFields },
        };

        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(maxRow);
        stack.Children.Add(zonesRow);

        if (includeShotChangeBehavior)
        {
            var combo = new ComboBox
            {
                Width = 220,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(_vm.ShotChangeBehaviors)) { Source = _vm },
                [!SelectingItemsControl.SelectedIndexProperty] = new Binding(nameof(_vm.SelectedChainingShotChangeBehaviorIndex)) { Source = _vm, Mode = BindingMode.TwoWay },
            };
            AutomationProperties.SetName(combo, _l.ShotChangeBehavior);
            var behaviorRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(0, 2, 0, 0),
                Children = { EditorChrome.MakeDimLabel(_l.ShotChangeBehavior, new Thickness(0)), combo, UiUtil.MakeHintIcon(_l.HintChaining, combo) },
            };
            stack.Children.Add(behaviorRow);
        }

        if (checkGeneralPath != null)
        {
            var switchRow = EditorChrome.MakeToggleRow(checkGeneralPath, _l.CheckGeneral, out _);
            switchRow.Margin = new Thickness(0, 2, 0, 0);
            stack.Children.Add(switchRow);
        }

        return stack;
    }

    /// <summary>
    /// Two sections side by side with a line between them, the columns sharing the width.
    /// </summary>
    private static Grid SideBySide(Control left, Control right)
    {
        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(1, GridUnitType.Star) },
        };
        grid.Add(left, 0, 0);
        grid.Add(new Border
        {
            Child = right,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);
        return grid;
    }

    /// <summary>
    /// The pages of a section (in/out cue closest, general/in/out on shot change) as segmented
    /// buttons; the selected page index lives in the view model, as it did for the tab controls.
    /// </summary>
    private Control MakeSegmented(string[] titles, string indexProperty, Func<int> getIndex, Action<int> setIndex, Action onChanged)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        var buttons = new Button[titles.Length];
        for (var i = 0; i < titles.Length; i++)
        {
            var index = i;
            var button = new Button
            {
                Content = new TextBlock { Text = titles[i], FontSize = EditorChrome.FieldFontSize, VerticalAlignment = VerticalAlignment.Center },
                Height = 26,
                Padding = new Thickness(10, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            button.Classes.Add("tool");
            button.Click += (_, _) => setIndex(index);
            AutomationProperties.SetName(button, titles[i]);
            buttons[i] = button;
            panel.Children.Add(button);
        }

        void Update()
        {
            var selected = getIndex();
            for (var i = 0; i < buttons.Length; i++)
            {
                buttons[i].Classes.Set("active", i == selected);
            }

            onChanged();
        }

        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == indexProperty)
            {
                Update();
            }
        };
        Update();

        return new Border
        {
            Child = panel,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
    }

    /// <summary>
    /// A cue preview in a rounded frame.
    /// </summary>
    private static Border FramePreview(Control preview)
    {
        return new Border
        {
            Child = preview,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
        };
    }

    /// <summary>
    /// A dim label of fixed width and the controls after it on one line.
    /// </summary>
    private static Grid Row(string label, params Control[] controls)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center };
        foreach (var control in controls)
        {
            panel.Children.Add(control);
        }

        return EditorChrome.MakeLabeledRow(label, panel, LabelWidth);
    }

    private static TextBlock MakeUnit(string text)
    {
        var unit = EditorChrome.MakeDimLabel(text, new Thickness(2, 0, 0, 0));
        unit.Foreground = EditorChrome.FaintTextBrush;
        return unit;
    }

    private Control MakeRowLabelNudSuffix(string label, string bindingPath, string suffix)
    {
        return Row(label, MakeFrameNud(bindingPath, label), MakeUnit(suffix));
    }

    private Control MakeZonesRow(string leftGreenPath, string leftRedPath, string rightRedPath, string rightGreenPath)
    {
        var first = MakeZoneNud(leftGreenPath, isGreen: true);
        return Row(_l.Zones,
            first,
            MakeZoneNud(leftRedPath, isGreen: false),
            MakeZoneNud(rightRedPath, isGreen: false),
            MakeZoneNud(rightGreenPath, isGreen: true),
            UiUtil.MakeHintIcon(_l.HintZones, first));
    }

    private NumericUpDown MakeFrameNud(string bindingPath, string accessibleName)
    {
        var nud = new NumericUpDown
        {
            Width = 64,
            Minimum = 0,
            Maximum = 100,
            Increment = 1,
            FormatString = "F0",
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(bindingPath) { Source = _vm, Mode = BindingMode.TwoWay, Converter = new IntDecimalConverter() },
        };
        nud.Classes.Add("field");
        AutomationProperties.SetName(nud, accessibleName);
        return nud;
    }

    private NumericUpDown MakeMsNud(string bindingPath, string accessibleName)
    {
        var nud = new NumericUpDown
        {
            Width = 80,
            Minimum = 0,
            Maximum = 5000,
            Increment = 10,
            FormatString = "F0",
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(bindingPath) { Source = _vm, Mode = BindingMode.TwoWay, Converter = new IntDecimalConverter() },
        };
        nud.Classes.Add("field");
        AutomationProperties.SetName(nud, accessibleName);
        return nud;
    }

    /// <summary>
    /// A zone field: a green (warn) or red (snap) dot inside it, the colors of the cue preview.
    /// </summary>
    private NumericUpDown MakeZoneNud(string bindingPath, bool isGreen)
    {
        var nud = MakeFrameNud(bindingPath, _l.Zones);
        nud.Width = 72;
        nud.InnerLeftContent = new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = isGreen ? GreenZoneBrush : RedZoneBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
        };
        return nud;
    }

    private sealed class IndexEqualsConverter : IValueConverter
    {
        public object? Convert(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int i && parameter is int p)
            {
                return i == p;
            }
            if (value is int i2 && int.TryParse(parameter?.ToString(), out var pi))
            {
                return i2 == pi;
            }
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
