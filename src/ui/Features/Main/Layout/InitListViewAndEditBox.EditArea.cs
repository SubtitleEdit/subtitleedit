using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

/// <summary>
/// The edit area under the subtitle grid in the editor look: a timing card (Show / Hide /
/// Duration / Layer rows with nudge buttons, the link toggle, a reading-speed bar and the gaps
/// to the neighbours) and a text card (formatting tool buttons, the text box framed as an editor
/// field with the per-line lengths beside it, CPS / total length chips underneath).
/// </summary>
public static partial class InitListViewAndEditBox
{
    /// <summary>Name of the timing card's content stack, so the height tracker can find it.</summary>
    internal const string TimingCardContentName = "SubtitleTimingCardContent";

    /// <summary>Automation id of the Show/Duration link toggle (tests find it by this name).</summary>
    internal const string StartDurationLinkToggleName = "SubtitleStartDurationLinkToggle";

    private const double CardCornerRadius = 8;
    private const double CardGap = 8;
    private const double TextFrameBorder = 1;
    private const string NudgeEarlierIcon = "mdi-menu-left";
    private const string NudgeLaterIcon = "mdi-menu-right";
    private const string LinkedIcon = "mdi-link-variant";
    private const string UnlinkedIcon = "mdi-link-variant-off";

    private static readonly IBrush OverLimitBrush = new ImmutableSolidColorBrush(Color.FromRgb(224, 82, 82));

    private static readonly IValueConverter GapVisibleConverter =
        new FuncValueConverter<double, bool>(v => v != double.MaxValue && !double.IsNaN(v));

    private static readonly IValueConverter UpperCaseConverter =
        new FuncValueConverter<string?, string>(v => (v ?? string.Empty).ToUpperInvariant());

    /// <summary>
    /// Binding path of the Show time code box: StartTimeKeepDuration moves the whole line
    /// (linked), StartTimeOnly moves only the start and keeps the end (unlinked).
    /// </summary>
    internal static string GetStartTimeBindingPath(bool keepDuration)
    {
        return nameof(MainViewModel.SelectedSubtitle) + "." + (keepDuration
            ? nameof(SubtitleLineViewModel.StartTimeKeepDuration)
            : nameof(SubtitleLineViewModel.StartTimeOnly));
    }

    internal static void BindStartTimeUpDown(TimeCodeUpDown upDown, bool keepDuration)
    {
        upDown[!TimeCodeUpDown.ValueProperty] = new Binding(GetStartTimeBindingPath(keepDuration))
        {
            Mode = BindingMode.TwoWay,
        };
    }

    /// <summary>
    /// One step of a time code box, like its own spinner on the last part: the Settings step in
    /// milliseconds, or one frame in frame mode. Written through the box's Value, so the two-way
    /// binding, ValueChanged (undo, CPS refresh) and the link mode behave as for the spinner.
    /// </summary>
    internal static void NudgeTimeCode(TimeCodeUpDown upDown, int direction)
    {
        if (!upDown.IsEffectivelyEnabled)
        {
            return;
        }

        var value = upDown.Value;
        TimeSpan newValue;
        if (Se.Settings.General.UseFrameMode)
        {
            // Via the total frame count, so an unaligned value is aligned to a frame first.
            var totalFrames = SubtitleFormat.MillisecondsToFrames(value.TotalMilliseconds);
            newValue = TimeSpan.FromMilliseconds(SubtitleFormat.FramesToMilliseconds(totalFrames + direction));
        }
        else
        {
            var step = Math.Max(1, Se.Settings.General.TimeCodeUpDownStepMs);
            newValue = value.Add(TimeSpan.FromMilliseconds(direction * step));
        }

        upDown.Value = newValue;
    }

    /// <summary>
    /// Local styles of the edit area only - flat tool buttons, compact time code fields and the
    /// borderless text box inside its editor frame. Menus, grid and toolbar keep the app look.
    /// </summary>
    private static void AddEditAreaStyles(Styles styles)
    {
        static Style Presenter(Func<Selector?, Selector> selector, params (AvaloniaProperty Property, object Value)[] setters)
        {
            var style = new Style(x => selector(x).Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            foreach (var (property, value) in setters)
            {
                style.Setters.Add(new Setter(property, value));
            }

            return style;
        }

        foreach (var type in new[] { typeof(Button), typeof(ToggleButton) })
        {
            styles.Add(new Style(x => x.OfType(type).Class("tool"))
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                    new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
                    new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
                    new Setter(TemplatedControl.ForegroundProperty, EditorChrome.IconBrush),
                    new Setter(TemplatedControl.PaddingProperty, new Thickness(0)),
                    new Setter(Layoutable.MinWidthProperty, 0.0),
                    new Setter(Layoutable.MinHeightProperty, 0.0),
                },
            });
            styles.Add(Presenter(x => x.OfType(type).Class("tool").Class(":pointerover"),
                (ContentPresenter.BackgroundProperty, EditorChrome.HoverBrush), (ContentPresenter.ForegroundProperty, EditorChrome.StrongTextBrush)));
            styles.Add(Presenter(x => x.OfType(type).Class("tool").Class(":pressed"),
                (ContentPresenter.BackgroundProperty, EditorChrome.PressedBrush)));
            styles.Add(Presenter(x => x.OfType(type).Class("tool").Class(":disabled"),
                (ContentPresenter.BackgroundProperty, Brushes.Transparent), (ContentPresenter.ForegroundProperty, EditorChrome.SeparatorBrush)));
        }

        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("tool").Class(":checked"),
            (ContentPresenter.BackgroundProperty, EditorChrome.AccentSoftBrush), (ContentPresenter.ForegroundProperty, EditorChrome.StrongTextBrush)));
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("tool").Class(":checked").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, EditorChrome.AccentSoftBrush), (ContentPresenter.ForegroundProperty, EditorChrome.StrongTextBrush)));

        // Compact time code / duration / layer fields on the editor field colors. The inner text
        // box is transparent so the spinner's field color shows; a local background (the
        // duration's too short/long warning, see SecondsUpDown) still wins over this style.
        foreach (var type in new[] { typeof(TimeCodeUpDown), typeof(SecondsUpDown) })
        {
            styles.Add(new Style(x => x.OfType(type))
            {
                Setters = { new Setter(Layoutable.MinHeightProperty, 28.0) },
            });
            styles.Add(new Style(x => x.OfType(type).Descendant().OfType<ButtonSpinner>())
            {
                Setters =
                {
                    new Setter(Layoutable.MinHeightProperty, 28.0),
                    new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
                    new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                    new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
                },
            });
            styles.Add(new Style(x => x.OfType(type).Descendant().OfType<TextBox>())
            {
                Setters =
                {
                    new Setter(Layoutable.MinHeightProperty, 26.0),
                    new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                },
            });
        }

        styles.Add(new Style(x => x.OfType<NumericUpDown>().Class("field"))
        {
            Setters =
            {
                new Setter(NumericUpDown.ShowButtonSpinnerProperty, false),
                new Setter(Layoutable.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
                new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        styles.Add(new Style(x => x.OfType<NumericUpDown>().Class("field").Descendant().OfType<TextBox>())
        {
            Setters =
            {
                new Setter(Layoutable.MinHeightProperty, 26.0),
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
            },
        });

        // The subtitle text boxes: borderless inside their frame, also while hovered or focused
        // (the frame shows the focus instead).
        styles.Add(new Style(x => x.OfType<TextBox>().Class("editorText"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(10, 7, 6, 7)),
            },
        });
        foreach (var state in new[] { ":pointerover", ":focus", ":disabled" })
        {
            styles.Add(new Style(x => x.OfType<TextBox>().Class("editorText").Class(state).Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters =
                {
                    new Setter(Border.BackgroundProperty, Brushes.Transparent),
                    new Setter(Border.BorderThicknessProperty, new Thickness(0)),
                },
            });
        }

        styles.Add(new Style(x => x.OfType<Border>().Class("textFrame").Class(":focus-within"))
        {
            Setters = { new Setter(Border.BorderBrushProperty, EditorChrome.AccentBrush) },
        });
    }

    /// <summary>
    /// The compact field look: no spinner buttons on a time code / duration box (they are set
    /// in its template, so a style cannot reach them). Up/Down and the mouse wheel still step the
    /// part under the caret, and Show/Hide have the nudge buttons beside them.
    /// </summary>
    private static void HideSpinnerButtons(TemplatedControl upDown)
    {
        upDown.TemplateApplied += (_, e) =>
        {
            if (e.NameScope.Find<ButtonSpinner>("PART_Spinner") is { } spinner)
            {
                spinner.ShowButtonSpinner = false;
            }
        };
    }

    private static Border MakeEditCardBackground()
    {
        return new Border
        {
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(CardCornerRadius),
        };
    }

    private static TextBlock MakeTimingLabel(string text)
    {
        return new TextBlock
        {
            Text = text,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = UiUtil.ScaledFontSize(12),
            [UiUtil.DesignFontSizeProperty] = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 4, 0),
        };
    }

    /// <summary>
    /// A "Label [field] [extra]" row of the timing card. The label and extra columns share their
    /// width across the rows (shared size scope on the rows panel), so the fields line up.
    /// </summary>
    private static Grid MakeTimingRow(string label, bool showLabel, Control field, Control? extra)
    {
        var row = new Grid { ColumnSpacing = 6 };
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto) { SharedSizeGroup = "TimingLabel" });
        row.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto) { SharedSizeGroup = "TimingExtra" });
        if (showLabel)
        {
            row.Add(MakeTimingLabel(label), 0, 0);
        }

        row.Add(field, 0, 1);
        if (extra != null)
        {
            row.Add(extra, 0, 2);
        }

        return row;
    }

    private static Button MakeEditToolButton(ICommand command, string icon, string hint, double size = 26)
    {
        var button = UiUtil.MakeButton(command as IRelayCommand, icon, hint);
        button.Classes.Add("tool");
        button.Width = size;
        button.Height = size;
        button.Margin = new Thickness(0);
        button.Padding = new Thickness(0);
        return button;
    }

    private static StackPanel MakeNudgeButtons(MainViewModel vm, TimeCodeUpDown upDown, string earlierName, string laterName)
    {
        Button Make(string icon, string name, int direction)
        {
            var button = MakeEditToolButton(new RelayCommand(() => NudgeTimeCode(upDown, direction)), icon, name, 22);
            button.Height = 28;
            button.FontSize = 20;
            button.IsTabStop = false; // Up/Down in the field step the same way; keep Tab on the fields
            button.Bind(InputElement.IsEnabledProperty, new MultiBinding
            {
                Converter = BoolConverters.And,
                Bindings =
                {
                    new Binding(nameof(vm.AreTimeCodesEditable)) { Source = vm },
                    new Binding(nameof(vm.SelectedSubtitle)) { Source = vm, Converter = ObjectConverters.IsNotNull },
                },
            });
            return button;
        }

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                Make(NudgeEarlierIcon, earlierName, -1),
                Make(NudgeLaterIcon, laterName, +1),
            },
        };
    }

    /// <summary>
    /// The link toggle beside Duration: linked = changing Show moves the whole line (keeps the
    /// duration), unlinked = it moves only the start. Persisted in Settings; rebinds the Show box.
    /// </summary>
    private static ToggleButton MakeStartDurationLinkToggle(TimeCodeUpDown startUpDown)
    {
        var linked = Se.Settings.Appearance.IsEditBoxStartLinkedToDuration;
        var icon = new Icon { Value = linked ? LinkedIcon : UnlinkedIcon, FontSize = 14 };
        var toggle = new ToggleButton
        {
            Name = StartDurationLinkToggleName,
            Content = icon,
            IsChecked = linked,
            Width = 22,
            Height = 28,
            IsTabStop = false,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        toggle.Classes.Add("tool");
        AutomationProperties.SetName(toggle, Se.Language.General.SetStartAndKeepDuration);
        AutomationProperties.SetAutomationId(toggle, StartDurationLinkToggleName);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(toggle, Se.Language.General.SetStartAndKeepDuration);
        }

        toggle.IsCheckedChanged += (_, _) =>
        {
            var keepDuration = toggle.IsChecked == true;
            Se.Settings.Appearance.EditBoxStartKeepsDuration = keepDuration;
            icon.Value = keepDuration ? LinkedIcon : UnlinkedIcon;
            BindStartTimeUpDown(startUpDown, keepDuration);
        };

        return toggle;
    }

    /// <summary>
    /// "Chars/second: 12.3" style chip; the background binding keeps the old label semantics
    /// (transparent, or the error color when over the limit).
    /// </summary>
    private static Border MakeInfoChip(MainViewModel vm, string textProperty, string backgroundProperty)
    {
        var chip = new Border
        {
            CornerRadius = new CornerRadius(5),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(6, 1),
            Margin = new Thickness(0, 0, 6, 0),
            VerticalAlignment = VerticalAlignment.Center,
            [!Border.BackgroundProperty] = new Binding(backgroundProperty) { Source = vm, Mode = BindingMode.OneWay },
            [!Visual.IsVisibleProperty] = new Binding(textProperty) { Source = vm, Converter = StringConverters.IsNotNullOrEmpty },
            Child = new TextBlock
            {
                FontSize = UiUtil.ScaledFontSize(11.5),
                [UiUtil.DesignFontSizeProperty] = 11.5,
                [!TextBlock.TextProperty] = new Binding(textProperty) { Source = vm, Mode = BindingMode.OneWay },
            },
        };
        return chip;
    }

    private static TextBlock MakeGapText(MainViewModel vm, string gapProperty, string format)
    {
        var path = nameof(vm.SelectedSubtitle) + "." + gapProperty;
        return new TextBlock
        {
            Foreground = EditorChrome.FaintTextBrush,
            FontSize = UiUtil.ScaledFontSize(11.5),
            [UiUtil.DesignFontSizeProperty] = 11.5,
            [!TextBlock.TextProperty] = new Binding(path)
            {
                Source = vm,
                Mode = BindingMode.OneWay,
                Converter = Logic.ValueConverters.DoubleToDisplayShortConverter.Instance,
                StringFormat = format,
                FallbackValue = string.Empty,
            },
            [!Visual.IsVisibleProperty] = new Binding(path)
            {
                Source = vm,
                Mode = BindingMode.OneWay,
                Converter = GapVisibleConverter,
                FallbackValue = false,
                TargetNullValue = false,
            },
        };
    }

    private static Border MakeTextFrame(Control textBox, StackPanel lengthsColumn)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        grid.Add(textBox, 0, 0);
        grid.Add(lengthsColumn, 0, 1);
        var frame = new Border
        {
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(TextFrameBorder),
            CornerRadius = new CornerRadius(6),
            Child = grid,
        };
        frame.Classes.Add("textFrame");
        return frame;
    }

    private static LineLengthsColumn MakeLineLengthsColumn(TextBox textBox)
    {
        var column = new LineLengthsColumn(textBox)
        {
            Margin = new Thickness(4, 0, 8, 0),
            MinWidth = 18,
        };
        TextBlock.SetTextAlignment(column, TextAlignment.Right);
        column.SetValue(Avalonia.Controls.Documents.TextElement.ForegroundProperty, EditorChrome.FaintTextBrush);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(column, Se.Language.Main.SingleLineLength.Trim().TrimEnd(':'));
        }

        return column;
    }

    /// <summary>
    /// The per-line lengths of the text box as a narrow column beside the text, one number per
    /// line. It is the panel <see cref="SubtitleTextInfoHelper.FillLineLengthPanel"/> fills
    /// ("Line length:", 12, "/", 30, ... ) - only the numbers (the odd children) are laid out,
    /// stacked at the text box's line height so they sit next to their lines.
    /// </summary>
    internal sealed class LineLengthsColumn : StackPanel
    {
        private readonly TextBox _lineSource;
        private double _rowHeight;
        private (FontFamily Family, double Size, FontWeight Weight) _rowHeightKey;

        public LineLengthsColumn(TextBox lineSource)
        {
            _lineSource = lineSource;
            Orientation = Orientation.Vertical;
        }

        private double RowHeight
        {
            get
            {
                var key = (_lineSource.FontFamily, _lineSource.FontSize, _lineSource.FontWeight);
                if (_rowHeight <= 0 || key != _rowHeightKey)
                {
                    var probe = new TextBlock { Text = "Ag", FontFamily = key.FontFamily, FontSize = key.FontSize, FontWeight = key.FontWeight };
                    probe.Measure(Size.Infinity);
                    _rowHeight = probe.DesiredSize.Height;
                    _rowHeightKey = key;
                }

                return _rowHeight;
            }
        }

        private double TopOffset => _lineSource.Padding.Top + _lineSource.BorderThickness.Top;

        protected override Size MeasureOverride(Size availableSize)
        {
            var rowHeight = RowHeight;
            double width = 0;
            double height = TopOffset;
            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                child.Measure(Size.Infinity);
                if (i % 2 == 1)
                {
                    width = Math.Max(width, child.DesiredSize.Width);
                    height += Math.Max(rowHeight, child.DesiredSize.Height);
                }
            }

            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var rowHeight = RowHeight;
            var y = TopOffset;
            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (i % 2 == 1)
                {
                    var slot = Math.Max(rowHeight, child.DesiredSize.Height);
                    var childHeight = child.DesiredSize.Height;
                    child.Arrange(new Rect(0, y + (slot - childHeight) / 2, finalSize.Width, childHeight));
                    y += slot;
                }
                else
                {
                    child.Arrange(new Rect(0, 0, 0, 0));
                }
            }

            return finalSize;
        }
    }

    /// <summary>
    /// Thin reading-speed bar: fill = current CPS / max CPS (red when over the max), with a tick
    /// at the optimal CPS.
    /// </summary>
    internal sealed class ReadingSpeedBar : Control
    {
        public static readonly StyledProperty<double> CpsProperty =
            AvaloniaProperty.Register<ReadingSpeedBar, double>(nameof(Cps));

        static ReadingSpeedBar()
        {
            AffectsRender<ReadingSpeedBar>(CpsProperty);
        }

        public double Cps
        {
            get => GetValue(CpsProperty);
            set => SetValue(CpsProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            var width = Bounds.Width;
            var height = Bounds.Height;
            var max = Se.Settings.General.SubtitleMaximumCharactersPerSeconds;
            if (width <= 0 || height <= 0 || max <= 0)
            {
                return;
            }

            const double barHeight = 4;
            var top = (height - barHeight) / 2;
            var track = new Rect(0, top, width, barHeight);
            context.DrawRectangle(EditorChrome.FieldBorderBrush, null, new RoundedRect(track, barHeight / 2));

            var cps = Cps;
            if (cps > 0)
            {
                var fill = Math.Min(1.0, cps / max) * width;
                var brush = CpsHelper.IsAboveMax(cps, max) ? OverLimitBrush : EditorChrome.AccentBrush;
                context.DrawRectangle(brush, null, new RoundedRect(new Rect(0, top, Math.Max(barHeight, fill), barHeight), barHeight / 2));
            }

            var optimal = Se.Settings.General.SubtitleOptimalCharactersPerSeconds;
            if (optimal > 0 && optimal < max)
            {
                var x = Math.Round(optimal / max * width);
                context.DrawRectangle(EditorChrome.DimTextBrush, null, new Rect(x - 1, 0, 2, height));
            }
        }
    }
}
