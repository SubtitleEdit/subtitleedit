using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Nikse.SubtitleEdit.Logic.EditorChrome;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// The "Animation" section of the ASSA draw properties: In/Out switch, a gallery of effects with
/// animated thumbnails, the effect duration and a play button for the canvas.
/// </summary>
internal static class AssaDrawAnimationPanel
{
    private static readonly DrawAnimationEffect[] Effects = Enum.GetValues<DrawAnimationEffect>();

    public static Border Create(AssaDrawViewModel vm)
    {
        var rows = new StackPanel { Spacing = 8, [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.IsAnimationExpanded)) };

        var inOut = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*"), ColumnSpacing = 4 };
        var inButton = MakeSegment(Se.Language.Assa.DrawAnimationIn, vm.EditInEffectCommand, Part.In);
        var loopButton = MakeSegment(Se.Language.Assa.DrawAnimationLoop, vm.EditLoopEffectCommand, Part.Loop);
        var outButton = MakeSegment(Se.Language.Assa.DrawAnimationOut, vm.EditOutEffectCommand, Part.Out);
        Grid.SetColumn(loopButton, 1);
        Grid.SetColumn(outButton, 2);
        inOut.Children.Add(inButton);
        inOut.Children.Add(loopButton);
        inOut.Children.Add(outButton);
        rows.Children.Add(inOut);

        var gallery = new UniformGrid { Columns = 3, [!UniformGrid.IsVisibleProperty] = new Binding(nameof(vm.IsEditingLoop)) { Converter = new FuncValueConverter<bool, bool>(loop => !loop) } };
        foreach (var effect in Effects)
        {
            gallery.Children.Add(MakeTile(vm, effect));
        }

        rows.Children.Add(gallery);

        var loopGallery = new UniformGrid { Columns = 3, [!UniformGrid.IsVisibleProperty] = new Binding(nameof(vm.IsEditingLoop)) };
        foreach (var effect in Enum.GetValues<DrawLoopEffect>())
        {
            loopGallery.Children.Add(MakeLoopTile(vm, effect));
        }

        rows.Children.Add(loopGallery);

        var duration = MakeField(nameof(vm.EditedEffectMs), 1, 60_000, prefix: "ms");
        duration.Increment = 50;
        AutomationProperties.SetName(duration, Se.Language.General.Duration);
        var play = new Button
        {
            Command = vm.ToggleAnimationPlaybackCommand,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            [!Button.IsEnabledProperty] = new Binding(nameof(vm.HasAnyAnimation)),
            Content = new Optris.Icons.Avalonia.Icon
            {
                FontSize = 12,
                [!Optris.Icons.Avalonia.Icon.ValueProperty] = new Binding(nameof(vm.IsPlayingAnimation))
                {
                    Converter = new FuncValueConverter<bool, string>(playing => playing ? "fa-solid fa-stop" : "fa-solid fa-play"),
                },
            },
        };
        play.Bind(AutomationProperties.NameProperty, new Binding(nameof(vm.IsPlayingAnimation))
        {
            Converter = new FuncValueConverter<bool, string>(playing => playing ? Se.Language.Assa.DrawAnimationStop : Se.Language.Assa.DrawAnimationPlay),
        });
        if (Se.Settings.Appearance.ShowHints)
        {
            play.Bind(ToolTip.TipProperty, new Binding(nameof(vm.IsPlayingAnimation))
            {
                Converter = new FuncValueConverter<bool, string>(playing => playing ? Se.Language.Assa.DrawAnimationStop : Se.Language.Assa.DrawAnimationPlay),
            });
        }

        rows.Children.Add(MakePropertyRow(Se.Language.General.Duration, duration, play));

        rows.Children.Add(new TextBlock
        {
            Text = Se.Language.Assa.DrawAnimationLoopBlocked,
            FontSize = EditorChrome.ScaledFont(11.5),
            TextWrapping = TextWrapping.Wrap,
            Foreground = new SolidColorBrush(Color.FromRgb(230, 160, 40)),
            [!TextBlock.IsVisibleProperty] = new Binding(nameof(vm.IsLoopBlocked)),
        });

        rows.Children.Add(new TextBlock
        {
            Text = Se.Language.Assa.DrawAnimationLayerHint,
            FontSize = EditorChrome.ScaledFont(11),
            TextWrapping = TextWrapping.Wrap,
            Foreground = FaintTextBrush,
        });

        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(MakeHeader(vm));
        panel.Children.Add(rows);
        return new Border
        {
            Child = panel,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(12, 10, 12, 12),
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.CanAnimate)),
        };
    }

    /// <summary>
    /// Collapsible header: the gallery is tall, so it stays closed until wanted (remembered) and the
    /// header shows the effects picked.
    /// </summary>
    private static Button MakeHeader(AssaDrawViewModel vm)
    {
        var chevron = new Optris.Icons.Avalonia.Icon
        {
            FontSize = 10,
            Foreground = DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!Optris.Icons.Avalonia.Icon.ValueProperty] = new Binding(nameof(vm.IsAnimationExpanded))
            {
                Converter = new FuncValueConverter<bool, string>(expanded => expanded ? "fa-solid fa-chevron-down" : "fa-solid fa-chevron-right"),
            },
        };
        var summary = new TextBlock
        {
            FontSize = EditorChrome.ScaledFont(11),
            Foreground = AccentBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.AnimationSummary)),
        };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"), ColumnSpacing = 6 };
        grid.Children.Add(chevron);
        var title = MakeSectionHeader(Se.Language.Assa.DrawAnimation);
        Grid.SetColumn(title, 1);
        grid.Children.Add(title);
        summary.HorizontalAlignment = HorizontalAlignment.Right;
        Grid.SetColumn(summary, 2);
        grid.Children.Add(summary);

        var header = new Button
        {
            Content = grid,
            Command = vm.ToggleAnimationExpandedCommand,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        };
        AutomationProperties.SetName(header, Se.Language.Assa.DrawAnimation);
        return header;
    }

    private enum Part
    {
        In,
        Loop,
        Out,
    }

    private static Button MakeSegment(string text, System.Windows.Input.ICommand command, Part part)
    {
        // Selected when the loop flag and the out flag say this part
        bool IsSelected(IReadOnlyList<object?> values) =>
            values is [bool loop, bool isOut] && (part == Part.Loop ? loop : !loop && isOut == (part == Part.Out));
        MultiBinding Selected(Func<bool, object> pick) => new()
        {
            Bindings =
            {
                new Binding(nameof(AssaDrawViewModel.IsEditingLoop)),
                new Binding(nameof(AssaDrawViewModel.IsEditingOutEffect)),
            },
            Converter = new FuncMultiValueConverter<object?, object>(values => pick(IsSelected(values.ToList()))),
        };

        var button = new Button
        {
            Content = text,
            Command = command,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Padding = new Thickness(6, 3),
            FontSize = EditorChrome.FieldFontSize,
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            [!Button.BackgroundProperty] = Selected(selected => selected ? AccentSoftBrush : Brushes.Transparent),
            [!Button.BorderBrushProperty] = Selected(selected => selected ? AccentBrush : SeparatorBrush),
            [!Button.FontWeightProperty] = Selected(selected => selected ? FontWeight.SemiBold : FontWeight.Normal),
        };
        AutomationProperties.SetName(button, text);
        return button;
    }

    private static Button MakeLoopTile(AssaDrawViewModel vm, DrawLoopEffect effect)
    {
        var thumbnail = new AnimationThumbnail { LoopEffect = effect, Width = 64, Height = 36 };
        var name = GetLoopName(effect);
        var label = new TextBlock
        {
            Text = name,
            FontSize = EditorChrome.ScaledFont(10.5),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = DimTextBrush,
        };
        var content = new StackPanel { Spacing = 2 };
        content.Children.Add(thumbnail);
        content.Children.Add(label);
        var tile = new Button
        {
            Content = content,
            Command = vm.PickLoopEffectCommand,
            CommandParameter = effect,
            Padding = new Thickness(3),
            Margin = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1.5),
            [!Button.BorderBrushProperty] = new Binding(nameof(vm.AnimationLoop))
            {
                Converter = new FuncValueConverter<DrawLoopEffect, IBrush>(e => e == effect ? AccentBrush : Brushes.Transparent),
            },
            [!Button.BackgroundProperty] = new Binding(nameof(vm.AnimationLoop))
            {
                Converter = new FuncValueConverter<DrawLoopEffect, IBrush>(e => e == effect ? AccentSoftBrush : Brushes.Transparent),
            },
        };
        AutomationProperties.SetName(tile, name);
        return tile;
    }

    public static string GetLoopName(DrawLoopEffect effect)
    {
        var l = Se.Language.Assa;
        return effect switch
        {
            DrawLoopEffect.Spin => l.DrawAnimationSpin,
            DrawLoopEffect.Pulse => l.DrawAnimationPulse,
            DrawLoopEffect.Blink => l.DrawAnimationBlink,
            DrawLoopEffect.Bounce => l.DrawAnimationBounce,
            DrawLoopEffect.Shake => l.DrawAnimationShake,
            DrawLoopEffect.Swing => l.DrawAnimationSwing,
            DrawLoopEffect.Flip => l.DrawAnimationFlip,
            _ => Se.Language.General.None,
        };
    }

    private static Button MakeTile(AssaDrawViewModel vm, DrawAnimationEffect effect)
    {
        var thumbnail = new AnimationThumbnail { Effect = effect, Width = 64, Height = 36 };
        thumbnail.Bind(AnimationThumbnail.IsOutProperty, new Binding(nameof(vm.IsEditingOutEffect)));
        var label = new TextBlock
        {
            FontSize = EditorChrome.ScaledFont(10.5),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = DimTextBrush,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.IsEditingOutEffect))
            {
                Converter = new FuncValueConverter<bool, string>(isOut => GetName(effect, isOut)),
            },
        };
        var content = new StackPanel { Spacing = 2 };
        content.Children.Add(thumbnail);
        content.Children.Add(label);

        var tile = new Button
        {
            Content = content,
            Command = vm.PickEffectCommand,
            CommandParameter = effect,
            Padding = new Thickness(3),
            Margin = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1.5),
            [!Button.BorderBrushProperty] = new Binding(nameof(vm.EditedEffect))
            {
                Converter = new FuncValueConverter<DrawAnimationEffect, IBrush>(e => e == effect ? AccentBrush : Brushes.Transparent),
            },
            [!Button.BackgroundProperty] = new Binding(nameof(vm.EditedEffect))
            {
                Converter = new FuncValueConverter<DrawAnimationEffect, IBrush>(e => e == effect ? AccentSoftBrush : Brushes.Transparent),
            },
        };
        tile.Bind(AutomationProperties.NameProperty, new Binding(nameof(vm.IsEditingOutEffect))
        {
            Converter = new FuncValueConverter<bool, string>(isOut => GetName(effect, isOut)),
        });
        return tile;
    }

    public static string GetName(DrawAnimationEffect effect, bool isOut)
    {
        var l = Se.Language.Assa;
        return effect switch
        {
            DrawAnimationEffect.Fade => l.DrawAnimationFade,
            DrawAnimationEffect.Pop => l.DrawAnimationPop,
            DrawAnimationEffect.Zoom => l.DrawAnimationZoom,
            DrawAnimationEffect.Spin => l.DrawAnimationSpin,
            DrawAnimationEffect.Left => isOut ? l.DrawAnimationToLeft : l.DrawAnimationFromLeft,
            DrawAnimationEffect.Right => isOut ? l.DrawAnimationToRight : l.DrawAnimationFromRight,
            DrawAnimationEffect.Top => isOut ? l.DrawAnimationToTop : l.DrawAnimationFromTop,
            DrawAnimationEffect.Bottom => isOut ? l.DrawAnimationToBottom : l.DrawAnimationFromBottom,
            _ => Se.Language.General.None,
        };
    }
}

/// <summary>
/// A small looping preview of an in or out effect on a badge (circle with a star).
/// </summary>
internal sealed class AnimationThumbnail : Control
{
    public static readonly StyledProperty<bool> IsOutProperty = AvaloniaProperty.Register<AnimationThumbnail, bool>(nameof(IsOut));

    private const int LineMs = 1300;
    private const int EffectMs = 450;
    private const int LoopMs = LineMs + 500;

    private static readonly Stopwatch Clock = Stopwatch.StartNew();
    private static readonly List<AnimationThumbnail> Running = [];
    private static DispatcherTimer? _timer;

    private static readonly IBrush FrameBrush = new SolidColorBrush(Color.FromRgb(38, 40, 46));
    private static readonly IBrush BlueBrush = new SolidColorBrush(Color.FromRgb(0x1E, 0x88, 0xE5));
    private static readonly IBrush StarBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07));

    public new DrawAnimationEffect Effect { get; set; }

    /// <summary>
    /// Shows this loop instead of the in/out effect.
    /// </summary>
    public DrawLoopEffect? LoopEffect { get; set; }

    public bool IsOut
    {
        get => GetValue(IsOutProperty);
        set => SetValue(IsOutProperty, value);
    }

    public AnimationThumbnail()
    {
        ClipToBounds = true;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Running.Add(this);
        _timer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(33), DispatcherPriority.Background, (_, _) =>
        {
            foreach (var thumbnail in Running)
            {
                if (thumbnail.IsEffectivelyVisible)
                {
                    thumbnail.InvalidateVisual();
                }
            }
        });
        _timer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Running.Remove(this);
        if (Running.Count == 0)
        {
            _timer?.Stop();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsOutProperty)
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        var width = (float)Bounds.Width;
        var height = (float)Bounds.Height;
        context.FillRectangle(FrameBrush, new Rect(0, 0, width, height), 4);

        var animation = LoopEffect is { } loop
            ? new DrawAnimation { Loop = loop, LoopMs = loop == DrawLoopEffect.Shake ? 400 : 900 }
            : IsOut
                ? new DrawAnimation { Out = Effect, OutMs = EffectMs }
                : new DrawAnimation { In = Effect, InMs = EffectMs };
        var time = Clock.Elapsed.TotalMilliseconds % LoopMs;
        var radius = height * 0.36f;
        var cx = width / 2;
        var cy = height / 2;
        var state = animation.GetState(Math.Min(time, LineMs), LineMs, (cx - radius, cy - radius, cx + radius, cy + radius), width, height);

        using (context.PushTransform(
                   Matrix.CreateTranslation(-cx, -cy) *
                   Matrix.CreateScale(Math.Max(0.001, state.Scale), Math.Max(0.001, state.Scale)) *
                   Matrix.CreateRotation(state.Rotation * Math.PI / 180) *
                   Matrix.CreateTranslation(cx + state.Dx, cy + state.Dy)))
        using (context.PushOpacity(state.Opacity))
        {
            context.DrawEllipse(BlueBrush, null, new Point(cx, cy), radius, radius);
            context.DrawEllipse(Brushes.White, null, new Point(cx, cy), radius * 0.78, radius * 0.78);
            context.DrawGeometry(StarBrush, null, MakeStar(cx, cy, radius * 0.62f, radius * 0.27f));
        }
    }

    private static StreamGeometry MakeStar(float cx, float cy, float outer, float inner)
    {
        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        for (var i = 0; i < 10; i++)
        {
            var angle = -Math.PI / 2 + i * Math.PI / 5;
            var r = i % 2 == 0 ? outer : inner;
            var point = new Point(cx + r * Math.Cos(angle), cy + r * Math.Sin(angle));
            if (i == 0)
            {
                ctx.BeginFigure(point, true);
            }
            else
            {
                ctx.LineTo(point);
            }
        }

        ctx.EndFigure(true);
        return geometry;
    }
}
