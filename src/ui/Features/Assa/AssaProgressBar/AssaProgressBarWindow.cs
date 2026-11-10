using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaProgressBar;

/// <summary>
/// Progress bar generator, styled like ASSA draw: settings on the left with the bar
/// styles as a gallery, the libass preview on the right.
/// </summary>
public class AssaProgressBarWindow : Window
{

    public AssaProgressBarWindow(AssaProgressBarViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.ProgressBarTitle;
        CanResize = true;
        Width = 1250;
        Height = 800;
        MinWidth = 950;
        MinHeight = 650;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = false;

        // No clip or rounded corners around the player: on macOS the video is a native surface
        // (composition child visual), and a clipped parent kept it from showing
        var videoPanel = new Border
        {
            Child = vm.VideoPlayerControl,
            Margin = new Thickness(12),
        };

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(340)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        content.Add(CreateSettingsPanel(vm, out var firstInput), 0, 0);
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
        Loaded += (_, __) => vm.LoadVideoAndSubtitle();
        KeyDown += vm.KeyDown;
    }

    private static Border CreateSettingsPanel(AssaProgressBarViewModel vm, out Control firstInput)
    {
        var sections = new StackPanel();
        sections.Children.Add(CreateStyleSection(vm));
        sections.Children.Add(CreatePositionSection(vm, out firstInput));
        sections.Children.Add(CreateColorsSection(vm));
        sections.Children.Add(CreateChaptersSection(vm));
        sections.Children.Add(CreateChapterTextSection(vm));

        return new Border
        {
            Child = new ScrollViewer { Content = sections, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled },
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };
    }

    /// <summary>
    /// Shows a tool button as pressed (accent colored) while <paramref name="isOn"/> is true.
    /// </summary>
    private static void BindActive(Button button, AssaProgressBarViewModel vm, Func<bool> isOn, params string[] properties)
    {
        button.Classes.Set("active", isOn());
        vm.PropertyChanged += (_, e) =>
        {
            if (Array.IndexOf(properties, e.PropertyName) >= 0)
            {
                button.Classes.Set("active", isOn());
            }
        };
    }

    private static NumericUpDown Field(string property, double min, double max, string? prefix, string name, double width = double.NaN)
    {
        var field = EditorChrome.MakeField(property, min, max, width, prefix);
        AutomationProperties.SetName(field, name);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(field, name);
        }

        return field;
    }

    /// <summary>
    /// The bar styles as a gallery; each thumbnail is the real bar shape, filled part of the way.
    /// </summary>
    private static Border CreateStyleSection(AssaProgressBarViewModel vm)
    {
        var gallery = new UniformGrid { Columns = 3 };
        var tiles = new List<Button>();
        for (var i = 0; i < vm.CornerStyles.Count; i++)
        {
            var index = i;
            var button = new Button
            {
                Content = MakeThumbnail(index),
                Height = 40,
                Margin = new Thickness(0, 0, 6, 6),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
            };
            button.Classes.Add("tool");
            AutomationProperties.SetName(button, vm.CornerStyles[index]);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(button, vm.CornerStyles[index]);
            }

            button.Click += (_, _) => vm.CornerStyleIndex = index;
            BindActive(button, vm, () => vm.CornerStyleIndex == index, nameof(vm.CornerStyleIndex));
            tiles.Add(button);
            gallery.Children.Add(button);
        }

        var name = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.BarStyleName)),
        };
        return EditorChrome.MakeEditorSection(Se.Language.General.Style, gallery, name, first: true);
    }

    /// <summary>
    /// A bar style 200 x 14, 60% filled - like the bar part way through the video.
    /// </summary>
    internal static Control MakeThumbnail(int styleIndex)
    {
        const double width = 200;
        const double height = 14;
        var geometry = AssaDrawingGeometry.FromDrawing(ProgressBarStyles.Drawing(styleIndex, (int)width, (int)height));
        var canvas = new Canvas { Width = width, Height = height };
        canvas.Children.Add(new Avalonia.Controls.Shapes.Path { Data = geometry, Fill = EditorChrome.ThumbnailMutedBrush });
        canvas.Children.Add(new Avalonia.Controls.Shapes.Path
        {
            Data = geometry,
            Fill = EditorChrome.ThumbnailBrush,
            Clip = new RectangleGeometry(new Rect(0, 0, width * 0.6, height)),
        });

        return new Viewbox { Child = canvas, Stretch = Stretch.Uniform, Margin = new Thickness(8, 6) };
    }

    private static Border CreatePositionSection(AssaProgressBarViewModel vm, out Control firstInput)
    {
        var top = EditorChrome.CreateToolButton("fa-solid fa-arrow-up", Se.Language.General.Top, null, 28);
        var bottom = EditorChrome.CreateToolButton("fa-solid fa-arrow-down", Se.Language.General.Bottom, null, 28);
        top.Click += (_, _) => vm.PositionTop = true;
        bottom.Click += (_, _) => vm.PositionBottom = true;
        BindActive(top, vm, () => vm.PositionTop, nameof(vm.PositionTop), nameof(vm.PositionBottom));
        BindActive(bottom, vm, () => vm.PositionBottom, nameof(vm.PositionTop), nameof(vm.PositionBottom));
        var topBottom = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        topBottom.Children.Add(top);
        topBottom.Children.Add(bottom);

        var height = Field(nameof(vm.BarHeight), 1, 200, "H", Se.Language.General.Height);
        var margin = Field(nameof(vm.EdgeMargin), 0, 1000, "M", Se.Language.Assa.ProgressBarEdgeMargin);
        firstInput = height;

        var playhead = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(nameof(vm.ShowPlayhead)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(playhead, Se.Language.Assa.ProgressBarPlayhead);
        var playheadRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        playheadRow.Children.Add(playhead);
        playheadRow.Children.Add(new TextBlock { Text = Se.Language.Assa.ProgressBarPlayhead, FontSize = EditorChrome.FieldFontSize, VerticalAlignment = VerticalAlignment.Center });

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Position, topBottom, new Control()));
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Size, height, margin));
        content.Children.Add(playheadRow);
        return EditorChrome.MakeEditorSection(Se.Language.General.Position, content);
    }

    private static Border CreateColorsSection(AssaProgressBarViewModel vm)
    {
        Button Swatch(string property, string name)
        {
            var button = UiUtil.MakeColorPickerButton(vm, property);
            button.HorizontalAlignment = HorizontalAlignment.Left;
            AutomationProperties.SetName(button, name);
            return button;
        }

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.Assa.ProgressBarForeColor, Swatch(nameof(vm.ForegroundColor), Se.Language.Assa.ProgressBarForeColor)));
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.BackgroundColor, Swatch(nameof(vm.BackgroundColor), Se.Language.General.BackgroundColor)));
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Text, Swatch(nameof(vm.TextColor), Se.Language.General.Text)));
        return EditorChrome.MakeEditorSection(Se.Language.General.Color, content);
    }

    private static Border CreateChaptersSection(AssaProgressBarViewModel vm)
    {
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        buttons.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.Add, vm.AddChapterCommand, 24));
        buttons.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Remove, vm.RemoveChapterCommand, 24));
        buttons.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-eraser", Se.Language.General.Remove + " " + Se.Language.General.All, vm.RemoveAllChaptersCommand, 24));

        var listBox = new ListBox
        {
            Height = 140,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.Chapters)),
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedChapter)) { Mode = BindingMode.TwoWay },
        };
        listBox.ItemTemplate = new FuncDataTemplate<ProgressBarChapter>((_, _) =>
        {
            var row = new DockPanel();
            var time = new TextBlock
            {
                FontSize = EditorChrome.ScaledFont(11.5),
                Foreground = EditorChrome.FaintTextBrush,
                VerticalAlignment = VerticalAlignment.Center,
                [!TextBlock.TextProperty] = new Binding(nameof(ProgressBarChapter.StartTimeDisplay)),
            };
            DockPanel.SetDock(time, Dock.Right);
            row.Children.Add(time);
            row.Children.Add(new TextBlock
            {
                FontSize = EditorChrome.FieldFontSize,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
                [!TextBlock.TextProperty] = new Binding(nameof(ProgressBarChapter.Text)),
            });
            return row;
        });

        var textBox = new TextBox
        {
            FontSize = EditorChrome.FieldFontSize,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(6),
            [!TextBox.TextProperty] = new Binding(nameof(vm.ChapterText)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(textBox, Se.Language.General.Text);

        var timeBox = new TextBox
        {
            FontSize = EditorChrome.FieldFontSize,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(6),
            PlaceholderText = "hh:mm:ss.fff",
        };
        timeBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.ChapterStartTime))
        {
            Mode = BindingMode.TwoWay,
            Converter = new FuncValueConverter<TimeSpan, string>(
                ts => ts.ToString(@"hh\:mm\:ss\.fff"),
                s => TimeSpan.TryParse(s, out var result) ? result : TimeSpan.Zero),
        });
        AutomationProperties.SetName(timeBox, Se.Language.General.StartTime);

        var editor = new StackPanel
        {
            Spacing = 6,
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.SelectedChapter)) { Converter = ObjectConverters.IsNotNull },
        };
        editor.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Text, textBox));
        editor.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.StartTime, timeBox));

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(listBox);
        content.Children.Add(editor);
        return EditorChrome.MakeEditorSection(Se.Language.General.Chapters, content, buttons);
    }

    private static Border CreateChapterTextSection(AssaProgressBarViewModel vm)
    {
        var fontCombo = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinHeight = 28,
            FontSize = EditorChrome.FieldFontSize,
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Fonts)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.FontName)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(fontCombo, Se.Language.General.Font);

        var size = Field(nameof(vm.FontSize), 1, 200, null, Se.Language.General.Size, 70);

        // Left / center / right as icon buttons
        var alignment = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        var icons = new[] { "fa-solid fa-align-left", "fa-solid fa-align-center", "fa-solid fa-align-right" };
        for (var i = 0; i < icons.Length; i++)
        {
            var index = i;
            var button = EditorChrome.CreateToolButton(icons[i], vm.TextAlignments[i], null, 28);
            button.Click += (_, _) => vm.TextAlignmentIndex = index;
            BindActive(button, vm, () => vm.TextAlignmentIndex == index, nameof(vm.TextAlignmentIndex));
            alignment.Children.Add(button);
        }

        var x = Field(nameof(vm.XAdjustment), -2000, 2000, "X", Se.Language.Assa.ProgressBarXAdjustment);
        var y = Field(nameof(vm.YAdjustment), -2000, 2000, "Y", Se.Language.Assa.ProgressBarYAdjustment);
        var splitterWidth = Field(nameof(vm.SplitterWidth), 0, 100, "W", Se.Language.Assa.ProgressBarSplitterWidth);
        var splitterHeight = Field(nameof(vm.SplitterHeight), 0, 500, "H", Se.Language.Assa.ProgressBarSplitterHeight);

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Font, fontCombo));
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Size, size, alignment));
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Position, x, y));
        content.Children.Add(EditorChrome.MakePropertyRow(Se.Language.Assa.ProgressBarSplitter, splitterWidth, splitterHeight));
        return EditorChrome.MakeEditorSection(Se.Language.General.Chapters + " - " + Se.Language.General.Text, content);
    }

    private static Border CreateButtonBar(AssaProgressBarViewModel vm)
    {
        var reset = new Button
        {
            Content = Se.Language.General.Reset,
            Command = vm.ResetCommand,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        buttons.Children.Add(EditorChrome.WithoutMargin(buttonCancel));
        buttons.Children.Add(EditorChrome.WithoutMargin(buttonOk));

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        grid.Add(reset, 0, 0);
        grid.Add(buttons, 0, 1);
        return EditorChrome.WrapButtonBar(grid);
    }
}
