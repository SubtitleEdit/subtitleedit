using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect;

/// <summary>
/// Animated effect preview (see <see cref="EffectPreviewRenderer"/>). While not animating it
/// shows a still "poster" frame, so a gallery full of tiles costs nothing until hovered.
/// </summary>
public class EffectPreviewControl : Control
{
    public static readonly StyledProperty<IAdvancedEffectDisplay?> AdvancedEffectProperty =
        AvaloniaProperty.Register<EffectPreviewControl, IAdvancedEffectDisplay?>(nameof(AdvancedEffect));

    public static readonly StyledProperty<string> PreviewTextProperty =
        AvaloniaProperty.Register<EffectPreviewControl, string>(nameof(PreviewText), "Subtitle Edit");

    public static readonly StyledProperty<bool> IsPlayingProperty =
        AvaloniaProperty.Register<EffectPreviewControl, bool>(nameof(IsPlaying));

    private readonly Stopwatch _clock = new();
    private bool _frameRequested;

    static EffectPreviewControl()
    {
        AffectsRender<EffectPreviewControl>(AdvancedEffectProperty, PreviewTextProperty, IsPlayingProperty);
        ClipToBoundsProperty.OverrideDefaultValue<EffectPreviewControl>(true);
    }

    public IAdvancedEffectDisplay? AdvancedEffect
    {
        get => GetValue(AdvancedEffectProperty);
        set => SetValue(AdvancedEffectProperty, value);
    }

    public string PreviewText
    {
        get => GetValue(PreviewTextProperty);
        set => SetValue(PreviewTextProperty, value);
    }

    public bool IsPlaying
    {
        get => GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsPlayingProperty || change.Property == AdvancedEffectProperty)
        {
            _clock.Restart();
            RequestFrame();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RequestFrame();
    }

    private void RequestFrame()
    {
        if (_frameRequested || !IsPlaying)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
        {
            return;
        }

        _frameRequested = true;
        topLevel.RequestAnimationFrame(_ =>
        {
            _frameRequested = false;
            if (!IsPlaying || VisualRoot == null)
            {
                return;
            }

            InvalidateVisual();
            RequestFrame();
        });
    }

    public override void Render(DrawingContext context)
    {
        var effect = AdvancedEffect;
        var time = IsPlaying
            ? _clock.Elapsed.TotalSeconds
            : EffectPreviewRenderer.GetPosterTime(AdvancedEffectCatalog.GetPreviewKind(effect));
        EffectPreviewRenderer.Render(context, new Rect(Bounds.Size), effect, time, PreviewText);
    }
}
