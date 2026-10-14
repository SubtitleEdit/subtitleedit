using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaSetBackground;

/// <summary>
/// Background box generator, laid out like ASSA draw: always dark, the libass preview in the
/// middle and a properties panel on the right with the box styles as a gallery.
/// </summary>
public class AssSetBackgroundWindow : Window
{
    private static readonly IBrush ThumbnailBrush = new SolidColorBrush(Color.FromRgb(214, 218, 226));

    public AssSetBackgroundWindow(AssSetBackgroundViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.BackgroundBoxGenerator;
        CanResize = true;
        Width = 1200;
        Height = 760;
        MinWidth = 950;
        MinHeight = 650;
        RequestedThemeVariant = ThemeVariant.Dark;
        Background = AssaDrawWindow.WindowBrush;
        AssaDrawWindow.AddEditorStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = false;
        var videoPanel = new Border
        {
            Child = vm.VideoPlayerControl,
            Background = Brushes.Black,
            Margin = new Thickness(12),
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
        };

        var sidePanel = CreateSidePanel(vm, out var firstInput);

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(320)),
            },
        };
        content.Add(videoPanel, 0, 0);
        content.Add(sidePanel, 0, 1);

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
            Background = AssaDrawWindow.PanelBrush,
            BorderBrush = AssaDrawWindow.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        };
    }

    /// <summary>
    /// A properties section like ASSA draw's: small caps header (with optional text on the right), line on top.
    /// </summary>
    private static Border MakeSection(string title, Control content, Control? headerRight = null, bool first = false)
    {
        var header = new DockPanel();
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            header.Children.Add(headerRight);
        }

        header.Children.Add(AssaDrawWindow.MakeSectionHeader(title));
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(header);
        panel.Children.Add(content);
        return new Border
        {
            Child = panel,
            BorderBrush = AssaDrawWindow.LineBrush,
            BorderThickness = new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
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
                Content = new Avalonia.Controls.Shapes.Path
                {
                    Data = MakeThumbnail(index),
                    Fill = ThumbnailBrush,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(12, 10),
                },
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

        var radius = AssaDrawWindow.MakeField(nameof(vm.Radius), 0, 200);
        radius.Bind(NumericUpDown.IsEnabledProperty, new Binding(nameof(vm.IsRadiusEnabled)));
        AutomationProperties.SetName(radius, Se.Language.Assa.BackgroundBoxRadius);

        var content = new StackPanel { Spacing = 6 };
        content.Children.Add(gallery);
        content.Children.Add(AssaDrawWindow.MakePropertyRow(Se.Language.Assa.BackgroundBoxRadius, radius, new Control()));

        var name = new TextBlock
        {
            FontSize = 12,
            Foreground = AssaDrawWindow.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.BoxStyleName)),
        };
        return MakeSection(Se.Language.General.Style, content, name, first: true);
    }

    /// <summary>
    /// A box style drawn into a 140 x 60 sample box, as geometry for its gallery tile.
    /// </summary>
    internal static Geometry MakeThumbnail(int styleIndex)
    {
        var drawing = AssSetBackgroundViewModel.GenerateBackgroundBox(styleIndex, 14, 10, 150, 10, 70);
        var shapes = DrawShape.ParseAssa(drawing, 0, Colors.White, false);
        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.SetFillRule(FillRule.NonZero);
        foreach (var shape in shapes.Where(s => s.Points.Count > 2))
        {
            ctx.BeginFigure(new Point(shape.Points[0].X, shape.Points[0].Y), true);
            var i = 1;
            while (i < shape.Points.Count)
            {
                var point = shape.Points[i];
                if (point.DrawType == DrawCoordinateType.BezierCurveSupport1 && i + 2 < shape.Points.Count)
                {
                    ctx.CubicBezierTo(new Point(point.X, point.Y),
                        new Point(shape.Points[i + 1].X, shape.Points[i + 1].Y),
                        new Point(shape.Points[i + 2].X, shape.Points[i + 2].Y));
                    i += 3;
                }
                else
                {
                    ctx.LineTo(new Point(point.X, point.Y));
                    i++;
                }
            }

            ctx.EndFigure(true);
        }

        return geometry;
    }

    /// <summary>
    /// Padding as a box diagram: the four sides around the text they pad.
    /// </summary>
    private static Border CreatePaddingSection(AssSetBackgroundViewModel vm, out NumericUpDown firstInput)
    {
        NumericUpDown Side(string property, string prefix, string name)
        {
            var field = AssaDrawWindow.MakeField(property, 0, 500, 74, prefix);
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
            Background = AssaDrawWindow.AccentSoftBrush,
            BorderBrush = AssaDrawWindow.AccentBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Height = 28,
            Child = new TextBlock
            {
                Text = Se.Language.General.Text,
                FontSize = 12,
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
        return MakeSection(Se.Language.General.Padding, grid);
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
        toggleRow.Children.Add(new TextBlock { Text = Se.Language.Assa.BackgroundBoxFillWidth, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center });

        var left = AssaDrawWindow.MakeField(nameof(vm.FillWidthMarginLeft), 0, 2000, prefix: "L");
        var right = AssaDrawWindow.MakeField(nameof(vm.FillWidthMarginRight), 0, 2000, prefix: "R");
        AutomationProperties.SetName(left, Se.Language.General.Left);
        AutomationProperties.SetName(right, Se.Language.General.Right);
        left.Bind(NumericUpDown.IsEnabledProperty, new Binding(nameof(vm.FillWidth)));
        right.Bind(NumericUpDown.IsEnabledProperty, new Binding(nameof(vm.FillWidth)));

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(toggleRow);
        content.Children.Add(AssaDrawWindow.MakePropertyRow(Se.Language.General.Margin, left, right));
        return MakeSection(Se.Language.Assa.BackgroundBoxFillWidth, content);
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

        var outlineWidth = AssaDrawWindow.MakeField(nameof(vm.OutlineWidth), 0, 20, prefix: "W");
        var shadowDistance = AssaDrawWindow.MakeField(nameof(vm.ShadowDistance), 0, 20, prefix: "D");
        AutomationProperties.SetName(outlineWidth, Se.Language.General.OutlineWidth);
        AutomationProperties.SetName(shadowDistance, Se.Language.General.ShadowWidth);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(outlineWidth, Se.Language.General.OutlineWidth);
            ToolTip.SetTip(shadowDistance, Se.Language.General.ShadowWidth);
        }

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(AssaDrawWindow.MakePropertyRow(Se.Language.General.BoxColor, Swatch(nameof(vm.BoxColor), Se.Language.General.BoxColor), new Control()));
        content.Children.Add(AssaDrawWindow.MakePropertyRow(Se.Language.General.Outline, Swatch(nameof(vm.OutlineColor), Se.Language.General.Outline), outlineWidth));
        content.Children.Add(AssaDrawWindow.MakePropertyRow(Se.Language.General.Shadow, Swatch(nameof(vm.ShadowColor), Se.Language.General.Shadow), shadowDistance));
        return MakeSection(Se.Language.General.Color, content);
    }

    private static Border CreateButtonBar(AssSetBackgroundViewModel vm)
    {
        var progress = new TextBlock
        {
            FontSize = 12,
            Foreground = AssaDrawWindow.DimTextBrush,
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
        return new Border
        {
            Child = grid,
            Background = AssaDrawWindow.ButtonBarBrush,
            BorderBrush = AssaDrawWindow.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
        };
    }
}
