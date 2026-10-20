using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;

/// <summary>
/// Set position, styled like ASSA draw: settings on the left (position with an anchor grid,
/// rotation, guides), the frame with the draggable text on the right, the tags OK writes at the bottom.
/// </summary>
public class AssaSetPositionWindow : Window
{
    private static readonly IBrush GuideBrush = new ImmutableSolidColorBrush(Color.FromArgb(110, 255, 255, 255));
    private static readonly IBrush CenterGuideBrush = new ImmutableSolidColorBrush(Color.FromArgb(140, 124, 160, 255));

    public AssaSetPositionWindow(AssaSetPositionViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.SetPosition;
        CanResize = true;
        MinWidth = 800;
        MinHeight = 500;
        Width = 1150;
        Height = 760;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(300)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        content.Add(CreateSettingsPanel(vm), 0, 0);
        content.Add(CreatePreview(vm), 0, 1);

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

        // Initial focus on the frame, so the arrow keys nudge the text right away (a focused X/Y
        // field keeps them for its caret - #16289), and not on an action button, which clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, vm.VideoGrid!);
        KeyDown += vm.KeyDown;
    }

    private static Border CreateSettingsPanel(AssaSetPositionViewModel vm)
    {
        var sections = new StackPanel();
        sections.Children.Add(CreatePositionSection(vm));
        sections.Children.Add(CreateRotationSection(vm));
        sections.Children.Add(CreateGuidesSection(vm));

        return new Border
        {
            Child = new ScrollViewer { Content = sections, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled },
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };
    }

    private static TextBlock MakeHeaderValue(string property, string? format = null) => new()
    {
        FontSize = 12,
        Foreground = EditorChrome.DimTextBrush,
        VerticalAlignment = VerticalAlignment.Center,
        [!TextBlock.TextProperty] = new Binding(property) { StringFormat = format },
    };

    private static Border CreatePositionSection(AssaSetPositionViewModel vm)
    {
        var x = EditorChrome.MakeField(nameof(vm.PositionX), -10000, 10000, prefix: "X");
        var y = EditorChrome.MakeField(nameof(vm.PositionY), -10000, 10000, prefix: "Y");
        AutomationProperties.SetName(x, "X");
        AutomationProperties.SetName(y, "Y");

        // 3 x 3 anchor grid: a dot in each cell, the chosen one highlighted
        var anchors = new UniformGrid { Columns = 3, Rows = 3, Width = 3 * 30, Height = 3 * 30, HorizontalAlignment = HorizontalAlignment.Left };
        for (var i = 0; i < 9; i++)
        {
            var index = i;
            var button = new Button
            {
                Width = 26,
                Height = 26,
                Margin = new Thickness(0, 0, 4, 4),
                Padding = new Thickness(0),
                Background = EditorChrome.FieldBrush,
                BorderBrush = EditorChrome.FieldBorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Content = new Ellipse { Width = 6, Height = 6, Fill = EditorChrome.FaintTextBrush },
                Command = vm.SetAnchorCommand,
                CommandParameter = index,
            };
            button.Classes.Add("tool");
            AutomationProperties.SetName(button, AssaSetPositionViewModel.AnchorNames[index]);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(button, AssaSetPositionViewModel.AnchorNames[index]);
            }

            void Update()
            {
                var on = vm.AnchorIndex == index;
                button.Classes.Set("active", on);
                ((Ellipse)button.Content!).Fill = on ? Brushes.White : EditorChrome.FaintTextBrush; // on the accent fill
            }

            Update();
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.AnchorIndex))
                {
                    Update();
                }
            };
            anchors.Children.Add(button);
        }

        var actions = new StackPanel { Spacing = 4 };
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-arrows-left-right", Se.Language.General.CenterHorizontally, vm.CenterHorizontallyCommand, 28));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-arrows-up-down", Se.Language.General.CenterVertically, vm.CenterVerticallyCommand, 28));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-rotate-left", Se.Language.Assa.SetPositionReset, vm.ResetPositionCommand, 28));

        var anchorRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 14 };
        anchorRow.Children.Add(anchors);
        anchorRow.Children.Add(actions);

        var content = new StackPanel { Spacing = 10 };
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Position, x, y));
        content.Children.Add(anchorRow);
        content.Children.Add(new TextBlock
        {
            Text = Se.Language.Assa.SetPositionNudgeHint,
            FontSize = 11.5,
            Foreground = EditorChrome.FaintTextBrush,
            TextWrapping = TextWrapping.Wrap,
        });
        return EditorChrome.MakeEditorSection(Se.Language.General.Position, content, MakeHeaderValue(nameof(vm.AnchorName)), first: true);
    }

    private static Border CreateRotationSection(AssaSetPositionViewModel vm)
    {
        var slider = new Slider
        {
            Minimum = -180,
            Maximum = 180,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding(nameof(vm.Rotation))
            {
                Mode = BindingMode.TwoWay,
                Converter = new FuncValueConverter<decimal, double>(d => (double)d, v => Math.Round((decimal)v)),
            },
        };
        AutomationProperties.SetName(slider, Se.Language.Assa.Rotation);

        var field = EditorChrome.MakeField(nameof(vm.Rotation), -360, 360, 64, "°", "0.##");
        AutomationProperties.SetName(field, Se.Language.Assa.Rotation);
        var reset = EditorChrome.CreateToolButton("fa-solid fa-rotate", Se.Language.Assa.SetPositionResetRotation, vm.ResetRotationCommand, 28);

        var row = new Grid { ColumnSpacing = 8, ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) } };
        row.Add(slider, 0, 0);
        row.Add(field, 0, 1);
        row.Add(reset, 0, 2);
        return EditorChrome.MakeEditorSection(Se.Language.Assa.Rotation, row, MakeHeaderValue(nameof(vm.Rotation), "{0:0.##}°"));
    }

    private static Border CreateGuidesSection(AssaSetPositionViewModel vm)
    {
        var content = new StackPanel { Spacing = 4 };
        content.Children.Add(EditorChrome.MakeToggleRow(nameof(vm.ShowSafeArea), Se.Language.Assa.SetPositionSafeArea));
        content.Children.Add(EditorChrome.MakeToggleRow(nameof(vm.ShowCenterLines), Se.Language.Assa.SetPositionCenterLines));
        content.Children.Add(EditorChrome.MakeToggleRow(nameof(vm.SnapToGuides), Se.Language.Assa.SetPositionSnapToGuides));
        return EditorChrome.MakeEditorSection(Se.Language.Assa.SetPositionGuides, content);
    }

    /// <summary>
    /// The frame with the guides over it and the draggable text (selection box + anchor dot).
    /// </summary>
    private static Control CreatePreview(AssaSetPositionViewModel vm)
    {
        var videoGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ClipToBounds = true,
            Focusable = true,
        };
        AutomationProperties.SetName(videoGrid, Se.Language.General.Position);

        // A click on the frame takes focus back from the X/Y fields, so the arrow keys nudge again
        videoGrid.PointerPressed += (_, _) => videoGrid.Focus();

        var bitmapImage = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.Screenshot)),
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        videoGrid.Children.Add(bitmapImage);

        // Guides, drawn over the shown frame (positioned when the frame is laid out)
        var guides = new Canvas { IsHitTestVisible = false };
        var safeArea = new Rectangle { Stroke = GuideBrush, StrokeThickness = 1, StrokeDashArray = [5, 4] };
        var vertical = new Line { Stroke = CenterGuideBrush, StrokeThickness = 1, StrokeDashArray = [5, 4] };
        var horizontal = new Line { Stroke = CenterGuideBrush, StrokeThickness = 1, StrokeDashArray = [5, 4] };
        safeArea.Bind(IsVisibleProperty, new Binding(nameof(vm.ShowSafeArea)));
        vertical.Bind(IsVisibleProperty, new Binding(nameof(vm.ShowCenterLines)));
        horizontal.Bind(IsVisibleProperty, new Binding(nameof(vm.ShowCenterLines)));
        guides.Children.Add(safeArea);
        guides.Children.Add(vertical);
        guides.Children.Add(horizontal);
        videoGrid.Children.Add(guides);

        // The text: rendered line, dashed selection box, and a dot on the \pos anchor
        var textImage = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.ScreenshotOverlayText)),
            Stretch = Stretch.Fill,
        };
        var selection = new Rectangle
        {
            Stroke = EditorChrome.AccentBrush,
            StrokeThickness = 1,
            StrokeDashArray = [4, 3],
            Margin = new Thickness(-3),
        };
        var anchorDot = new Ellipse
        {
            Width = 10,
            Height = 10,
            Fill = EditorChrome.AccentBrush,
            Stroke = Brushes.White,
            StrokeThickness = 1.5,
            Margin = new Thickness(-5),
        };
        var overlay = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.SizeAll),
            Background = Brushes.Transparent,
        };
        overlay.Children.Add(textImage);
        overlay.Children.Add(selection);
        overlay.Children.Add(anchorDot);
        videoGrid.Children.Add(overlay);

        vm.ScreenshotImage = bitmapImage;
        vm.ScreenshotOverlayImage = overlay;
        vm.VideoGrid = videoGrid;

        vm.FrameRectChanged += (_, rect) =>
        {
            var marginX = rect.Width * AssaSetPositionViewModel.SafeAreaMargin;
            var marginY = rect.Height * AssaSetPositionViewModel.SafeAreaMargin;
            Canvas.SetLeft(safeArea, rect.X + marginX);
            Canvas.SetTop(safeArea, rect.Y + marginY);
            safeArea.Width = Math.Max(0, rect.Width - 2 * marginX);
            safeArea.Height = Math.Max(0, rect.Height - 2 * marginY);
            vertical.StartPoint = new Point(rect.X + rect.Width / 2, rect.Y);
            vertical.EndPoint = new Point(rect.X + rect.Width / 2, rect.Bottom);
            horizontal.StartPoint = new Point(rect.X, rect.Y + rect.Height / 2);
            horizontal.EndPoint = new Point(rect.Right, rect.Y + rect.Height / 2);

            // The dot sits on the alignment point of the text box, where \pos puts it
            var alignment = vm.Alignment;
            anchorDot.HorizontalAlignment = alignment is "1" or "4" or "7" ? HorizontalAlignment.Left : alignment is "3" or "6" or "9" ? HorizontalAlignment.Right : HorizontalAlignment.Center;
            anchorDot.VerticalAlignment = alignment is "7" or "8" or "9" ? VerticalAlignment.Top : alignment is "4" or "5" or "6" ? VerticalAlignment.Center : VerticalAlignment.Bottom;
        };

        bitmapImage.SizeChanged += (_, _) => vm.UpdateOverlayPosition();
        videoGrid.SizeChanged += (_, _) => vm.UpdateOverlayPosition();

        // Drag the text; positions go through the view model so snapping applies
        Point? dragStart = null;
        var originalX = 0;
        var originalY = 0;
        overlay.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(overlay).Properties.IsLeftButtonPressed)
            {
                videoGrid.Focus();
                dragStart = e.GetPosition(videoGrid);
                originalX = vm.ScreenshotX;
                originalY = vm.ScreenshotY;
                e.Pointer.Capture(overlay);
                e.Handled = true;
            }
        };
        overlay.PointerMoved += (_, e) =>
        {
            if (dragStart is not { } start || bitmapImage.Bounds.Width <= 0)
            {
                return;
            }

            var delta = e.GetPosition(videoGrid) - start;
            var scale = bitmapImage.Bounds.Width / vm.RenderWidth;
            vm.DragTo(originalX + (int)Math.Round(delta.X / scale), originalY + (int)Math.Round(delta.Y / scale));
            e.Handled = true;
        };
        overlay.PointerReleased += (_, e) =>
        {
            if (dragStart != null)
            {
                dragStart = null;
                e.Pointer.Capture(null);
                e.Handled = true;
            }
        };

        var info = new DockPanel { Margin = new Thickness(2, 8, 2, 0) };
        info.Children.Add(new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.LineInfo)),
        });

        var panel = new Grid { Margin = new Thickness(12), RowDefinitions = { new RowDefinition(new GridLength(1, GridUnitType.Star)), new RowDefinition(GridLength.Auto) } };
        panel.Add(videoGrid, 0, 0);
        panel.Add(info, 1, 0);
        return panel;
    }

    private static Border CreateButtonBar(AssaSetPositionViewModel vm)
    {
        var tags = new TextBlock
        {
            FontFamily = new FontFamily("Menlo, Consolas, Cascadia Mono, DejaVu Sans Mono, monospace"),
            FontSize = 12,
            Foreground = EditorChrome.FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.TagsPreview)),
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
        grid.Add(tags, 0, 0);
        grid.Add(buttons, 0, 1);
        return EditorChrome.WrapButtonBar(grid);
    }
}
