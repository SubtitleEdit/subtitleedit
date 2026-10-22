using System.Collections.Generic;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaSetBackground;

/// <summary>
/// Background box generator, styled like ASSA draw: a properties panel on the
/// left with the box styles as a gallery, the libass preview on the right.
/// </summary>
public class AssSetBackgroundWindow : Window
{

    public AssSetBackgroundWindow(AssSetBackgroundViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.BackgroundBoxGenerator;
        CanResize = true;
        Width = 1200;
        Height = 760;
        MinWidth = 950;
        MinHeight = 650;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = false;

        // A click (or a wheel / trackpad scroll) on the preview would play or seek away from
        // the line being styled
        vm.VideoPlayerControl.ClickToTogglePlay = false;
        vm.VideoPlayerControl.WheelToSeek = false;
        // No clip or rounded corners around the player: on macOS the video is a native surface
        // (composition child visual), and a clipped parent kept it from showing
        var videoPanel = new Border
        {
            Child = vm.VideoPlayerControl,
            Margin = new Thickness(12),
        };

        var sidePanel = CreateSidePanel(vm, out var firstInput);

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(320)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        content.Add(sidePanel, 0, 0);
        content.Add(videoPanel, 0, 1);

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        mainGrid.Add(content, 0, 0);
        mainGrid.Add(CreateButtonBar(vm), 1, 0);
        Content = mainGrid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, firstInput);
        AddHandler(KeyDownEvent, vm.KeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, _) => vm.OnClosing();
    }

    private static Border CreateSidePanel(AssSetBackgroundViewModel vm, out NumericUpDown firstInput)
    {
        var sections = new StackPanel();
        sections.Children.Add(CreateStyleSection(vm));
        sections.Children.Add(CreatePaddingSection(vm, out firstInput));
        sections.Children.Add(CreateFillWidthSection(vm));
        sections.Children.Add(CreateColorsSection(vm));

        return new Border
        {
            Child = new ScrollViewer { Content = sections, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled },
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };
    }

    /// <summary>
    /// The box styles as a gallery, each thumbnail drawn by the generator itself.
    /// </summary>
    private static Border CreateStyleSection(AssSetBackgroundViewModel vm)
    {
        const int columns = 3;
        var gallery = new UniformGrid { Columns = columns };
        var tiles = new List<Button>();
        for (var i = 0; i < vm.BoxStyles.Count; i++)
        {
            var index = i;
            var button = new Button
            {
                Content = MakeThumbnailView(index),
                Height = 54,
                Margin = new Thickness(0, 0, 6, 6),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
            };
            button.Classes.Add("tool");
            AutomationProperties.SetName(button, vm.BoxStyles[index]);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(button, vm.BoxStyles[index]);
            }

            button.Click += (_, _) => vm.BoxStyleIndex = index;
            tiles.Add(button);
            gallery.Children.Add(button);
        }

        void UpdateSelection()
        {
            for (var i = 0; i < tiles.Count; i++)
            {
                tiles[i].Classes.Set("active", i == vm.BoxStyleIndex);
            }
        }

        UpdateSelection();
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.BoxStyleIndex))
            {
                UpdateSelection();
            }
        };

        var radius = EditorChrome.MakeField(nameof(vm.Radius), 0, 200);
        AutomationProperties.SetName(radius, Se.Language.Assa.BackgroundBoxRadius);
        var radiusRow = EditorChrome.MakePropertyRow(Se.Language.Assa.BackgroundBoxRadius, radius, new Control());
        radiusRow.Bind(Grid.IsVisibleProperty, new Binding(nameof(vm.IsRadiusEnabled)));

        var content = new StackPanel { Spacing = 6 };
        content.Children.Add(gallery);
        content.Children.Add(radiusRow);

        var name = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.BoxStyleName)),
        };
        return EditorChrome.MakeEditorSection(Se.Language.General.Style, content, name, first: true);
    }

    // The sample box the thumbnails are drawn around, and the "text" inside it
    private static readonly Rect SampleBox = new(10, 10, 140, 60);

    /// <summary>
    /// A tile: the style drawn around a sample text box, scaled as a whole - so a bar under the
    /// text stays under it (stretching the bar alone showed the underline as a line at the top).
    /// </summary>
    private static Control MakeThumbnailView(int styleIndex)
    {
        var geometry = MakeThumbnail(styleIndex);
        var bounds = geometry.Bounds.Union(SampleBox);
        var canvas = new Canvas { Width = bounds.Width, Height = bounds.Height };
        var path = new Avalonia.Controls.Shapes.Path { Data = geometry, Fill = EditorChrome.ThumbnailBrush };
        Canvas.SetLeft(path, -bounds.X);
        Canvas.SetTop(path, -bounds.Y);
        canvas.Children.Add(path);

        // Two lines of "text" in the middle of the sample box
        foreach (var (width, y) in new[] { (80.0, 30.0), (56.0, 44.0) })
        {
            var line = new Border { Width = width, Height = 7, CornerRadius = new CornerRadius(3.5), Background = EditorChrome.ThumbnailMutedBrush };
            Canvas.SetLeft(line, SampleBox.Center.X - width / 2 - bounds.X);
            Canvas.SetTop(line, y - bounds.Y);
            canvas.Children.Add(line);
        }

        return new Viewbox { Child = canvas, Stretch = Stretch.Uniform, Margin = new Thickness(10, 8) };
    }

    /// <summary>
    /// A box style drawn into the 140 x 60 sample box, as geometry for its gallery tile.
    /// </summary>
    internal static Geometry MakeThumbnail(int styleIndex) =>
        AssaDrawingGeometry.FromDrawing(AssSetBackgroundViewModel.GenerateBackgroundBox(styleIndex, 14, 10, 150, 10, 70));

    /// <summary>
    /// Padding as a box diagram: the four sides around the text they pad.
    /// </summary>
    private static Border CreatePaddingSection(AssSetBackgroundViewModel vm, out NumericUpDown firstInput)
    {
        NumericUpDown Side(string property, string prefix, string name)
        {
            var field = EditorChrome.MakeField(property, 0, 500, 74, prefix);
            AutomationProperties.SetName(field, name);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(field, name);
            }

            return field;
        }

        var top = Side(nameof(vm.PaddingTop), "T", Se.Language.General.Top);
        var left = Side(nameof(vm.PaddingLeft), "L", Se.Language.General.Left);
        var right = Side(nameof(vm.PaddingRight), "R", Se.Language.General.Right);
        var bottom = Side(nameof(vm.PaddingBottom), "B", Se.Language.General.Bottom);
        top.HorizontalAlignment = HorizontalAlignment.Center;
        bottom.HorizontalAlignment = HorizontalAlignment.Center;
        firstInput = left;

        var text = new Border
        {
            Background = EditorChrome.AccentSoftBrush,
            BorderBrush = EditorChrome.AccentBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Height = 28,
            Child = new TextBlock
            {
                Text = Se.Language.General.Text,
                FontSize = EditorChrome.FieldFontSize,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
            ColumnSpacing = 6,
            RowSpacing = 6,
        };
        grid.Add(top, 0, 1);
        grid.Add(left, 1, 0);
        grid.Add(text, 1, 1);
        grid.Add(right, 1, 2);
        grid.Add(bottom, 2, 1);
        return EditorChrome.MakeEditorSection(Se.Language.General.Padding, grid);
    }

    private static Border CreateFillWidthSection(AssSetBackgroundViewModel vm)
    {
        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(nameof(vm.FillWidth)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggle, Se.Language.Assa.BackgroundBoxFillWidth);
        var toggleRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        toggleRow.Children.Add(toggle);
        toggleRow.Children.Add(new TextBlock { Text = Se.Language.Assa.BackgroundBoxFillWidth, FontSize = EditorChrome.FieldFontSize, VerticalAlignment = VerticalAlignment.Center });

        var left = EditorChrome.MakeField(nameof(vm.FillWidthMarginLeft), 0, 2000, prefix: "L");
        var right = EditorChrome.MakeField(nameof(vm.FillWidthMarginRight), 0, 2000, prefix: "R");
        AutomationProperties.SetName(left, Se.Language.General.Left);
        AutomationProperties.SetName(right, Se.Language.General.Right);
        left.Bind(NumericUpDown.IsEnabledProperty, new Binding(nameof(vm.FillWidth)));
        right.Bind(NumericUpDown.IsEnabledProperty, new Binding(nameof(vm.FillWidth)));

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(toggleRow);
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Margin, left, right));
        return EditorChrome.MakeEditorSection(Se.Language.Assa.BackgroundBoxFillWidth, content);
    }

    /// <summary>
    /// Box, outline and shadow colors, with the outline width and shadow distance next to theirs.
    /// </summary>
    private static Border CreateColorsSection(AssSetBackgroundViewModel vm)
    {
        Button Swatch(string property, string name)
        {
            var button = UiUtil.MakeColorPickerButton(vm, property);
            button.HorizontalAlignment = HorizontalAlignment.Left;
            AutomationProperties.SetName(button, name);
            return button;
        }

        var outlineWidth = EditorChrome.MakeField(nameof(vm.OutlineWidth), 0, 20, prefix: "W");
        var shadowDistance = EditorChrome.MakeField(nameof(vm.ShadowDistance), 0, 20, prefix: "D");
        AutomationProperties.SetName(outlineWidth, Se.Language.General.OutlineWidth);
        AutomationProperties.SetName(shadowDistance, Se.Language.General.ShadowWidth);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(outlineWidth, Se.Language.General.OutlineWidth);
            ToolTip.SetTip(shadowDistance, Se.Language.General.ShadowWidth);
        }

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.BoxColor, Swatch(nameof(vm.BoxColor), Se.Language.General.BoxColor), new Control()));
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Outline, Swatch(nameof(vm.OutlineColor), Se.Language.General.Outline), outlineWidth));
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Shadow, Swatch(nameof(vm.ShadowColor), Se.Language.General.Shadow), shadowDistance));
        return EditorChrome.MakeEditorSection(Se.Language.General.Color, content);
    }

    private static Border CreateButtonBar(AssSetBackgroundViewModel vm)
    {
        var progress = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ProgressText)),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        buttons.Children.Add(buttonCancel);
        buttons.Children.Add(buttonOk);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        grid.Add(progress, 0, 0);
        grid.Add(buttons, 0, 1);
        return EditorChrome.WrapButtonBar(grid);
    }
}
