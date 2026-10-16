using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// In/out effects of the selected layers (a group's layers move as one) and playing them on the canvas.
/// </summary>
public partial class AssaDrawViewModel
{
    /// <summary>
    /// Pause after the line ends before the canvas plays the animation again.
    /// </summary>
    private const int PlaybackPauseMs = 700;

    [ObservableProperty] private DrawAnimationEffect _animationIn;
    [ObservableProperty] private DrawAnimationEffect _animationOut;
    [ObservableProperty] private int _animationInMs = DrawAnimation.DefaultDurationMs;
    [ObservableProperty] private int _animationOutMs = DrawAnimation.DefaultDurationMs;

    /// <summary>
    /// The effect gallery edits the out effect (false: the in effect).
    /// </summary>
    [ObservableProperty] private bool _isEditingOutEffect;
    [ObservableProperty] private bool _canAnimate;
    [ObservableProperty] private bool _isAnimationSlideConflict;
    [ObservableProperty] private bool _isPlayingAnimation;
    [ObservableProperty] private bool _hasAnyAnimation;
    [ObservableProperty] private bool _isAnimationExpanded = Se.Settings.Assa.DrawAnimationExpanded;
    [ObservableProperty] private string _animationSummary = string.Empty;

    private bool _updatingAnimationFields;
    private DispatcherTimer? _playbackTimer;
    private readonly Stopwatch _playbackClock = new();

    /// <summary>
    /// The effect the gallery shows as picked: the in or the out effect.
    /// </summary>
    public DrawAnimationEffect EditedEffect
    {
        get => IsEditingOutEffect ? AnimationOut : AnimationIn;
        set
        {
            if (IsEditingOutEffect)
            {
                AnimationOut = value;
            }
            else
            {
                AnimationIn = value;
            }
        }
    }

    public int EditedEffectMs
    {
        get => IsEditingOutEffect ? AnimationOutMs : AnimationInMs;
        set
        {
            if (IsEditingOutEffect)
            {
                AnimationOutMs = value;
            }
            else
            {
                AnimationInMs = value;
            }
        }
    }

    /// <summary>
    /// What the effects apply to: every shape on the selected shapes' layers (a layer is one line).
    /// </summary>
    internal List<DrawShape> GetAnimationShapes()
    {
        var picked = GetSelectedShapeList();
        if (picked.Count == 0 && SelectedTreeItem?.IsLayer == true)
        {
            var layer = SelectedTreeItem.Layer;
            return Shapes.Where(s => s.Layer == layer).ToList();
        }

        var layers = picked.Select(s => s.Layer).ToHashSet();
        return Shapes.Where(s => layers.Contains(s.Layer)).ToList();
    }

    [RelayCommand]
    private void PickEffect(DrawAnimationEffect effect) => EditedEffect = effect;

    [RelayCommand]
    private void EditInEffect() => IsEditingOutEffect = false;

    [RelayCommand]
    private void EditOutEffect() => IsEditingOutEffect = true;

    private void RefreshAnimationFields()
    {
        var shapes = GetAnimationShapes();
        var animation = shapes.Select(s => s.Animation).FirstOrDefault(a => a != null) ?? new DrawAnimation();
        _updatingAnimationFields = true;
        try
        {
            CanAnimate = shapes.Count > 0;
            AnimationIn = animation.In;
            AnimationOut = animation.Out;
            AnimationInMs = animation.InMs;
            AnimationOutMs = animation.OutMs;
        }
        finally
        {
            _updatingAnimationFields = false;
        }

        UpdateAnimationState();
    }

    partial void OnAnimationInChanged(DrawAnimationEffect value) => ApplyAnimation("animation-in");

    partial void OnAnimationOutChanged(DrawAnimationEffect value) => ApplyAnimation("animation-out");

    partial void OnAnimationInMsChanged(int value) => ApplyAnimation("animation-in-ms");

    partial void OnAnimationOutMsChanged(int value) => ApplyAnimation("animation-out-ms");

    partial void OnIsEditingOutEffectChanged(bool value)
    {
        OnPropertyChanged(nameof(EditedEffect));
        OnPropertyChanged(nameof(EditedEffectMs));
    }

    private void ApplyAnimation(string undoKey)
    {
        OnPropertyChanged(nameof(EditedEffect));
        OnPropertyChanged(nameof(EditedEffectMs));
        if (_updatingAnimationFields)
        {
            return;
        }

        var shapes = GetAnimationShapes();
        if (shapes.Count > 0)
        {
            var animation = new DrawAnimation
            {
                In = AnimationIn,
                Out = AnimationOut,
                InMs = Math.Clamp(AnimationInMs, 1, 60_000),
                OutMs = Math.Clamp(AnimationOutMs, 1, 60_000),
            };

            if (shapes.Any(s => !Equals(s.Animation ?? new DrawAnimation(), animation)))
            {
                SaveUndo(undoKey);
                foreach (var shape in shapes)
                {
                    shape.Animation = animation.IsNone ? null : animation.Clone();
                }

                UpdateCodePreview();
            }
        }

        UpdateAnimationState();

        // Show what was picked
        if (!IsPlayingAnimation && !_updatingAnimationFields && HasAnyAnimation && shapes.Count > 0)
        {
            StartAnimationPlayback();
        }
    }

    partial void OnIsAnimationExpandedChanged(bool value) => Se.Settings.Assa.DrawAnimationExpanded = value;

    [RelayCommand]
    private void ToggleAnimationExpanded() => IsAnimationExpanded = !IsAnimationExpanded;

    private void UpdateAnimationState()
    {
        AnimationSummary = AnimationIn == DrawAnimationEffect.None && AnimationOut == DrawAnimationEffect.None
            ? string.Empty
            : AssaDrawAnimationPanel.GetName(AnimationIn, isOut: false) + " · " + AssaDrawAnimationPanel.GetName(AnimationOut, isOut: true);
        IsAnimationSlideConflict = DrawAnimation.IsSlide(AnimationIn) && DrawAnimation.IsSlide(AnimationOut);
        HasAnyAnimation = Shapes.Any(s => s.Animation is { IsNone: false });
        if (!HasAnyAnimation && IsPlayingAnimation)
        {
            StopAnimationPlayback();
        }
    }

    [RelayCommand]
    private void ToggleAnimationPlayback()
    {
        if (IsPlayingAnimation)
        {
            StopAnimationPlayback();
        }
        else
        {
            StartAnimationPlayback();
        }
    }

    private void StartAnimationPlayback()
    {
        if (Canvas == null)
        {
            return;
        }

        IsPlayingAnimation = true;
        Canvas.GetAnimationTransform = GetCanvasAnimationTransform;
        _playbackClock.Restart();
        _playbackTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, OnPlaybackTick);
        _playbackTimer.Start();
        OnPlaybackTick(null, EventArgs.Empty);
    }

    internal void StopAnimationPlayback()
    {
        _playbackTimer?.Stop();
        _playbackClock.Reset();
        IsPlayingAnimation = false;
        if (Canvas != null)
        {
            Canvas.AnimationTime = null;
            Canvas.InvalidateVisual();
        }
    }

    private void OnPlaybackTick(object? sender, EventArgs e)
    {
        if (Canvas == null)
        {
            _playbackTimer?.Stop();
            return;
        }

        var loop = LineDurationMs + PlaybackPauseMs;
        var time = _playbackClock.Elapsed.TotalMilliseconds % loop;
        Canvas.AnimationTime = Math.Min(time, LineDurationMs);
        Canvas.InvalidateVisual();
    }

    /// <summary>
    /// The canvas asks this for each layer while playing: the state of the layer's animation at the
    /// time, and the anchor it scales and turns around.
    /// </summary>
    internal (DrawAnimationState State, float AnchorX, float AnchorY) GetCanvasAnimationTransform(DrawShape styleShape, double timeMs)
    {
        if (styleShape.Animation is not { IsNone: false } animation)
        {
            return (DrawAnimationState.Rest, 0, 0);
        }

        var anchor = GetAnimationAnchor(styleShape);
        return (animation.GetState(timeMs, LineDurationMs, anchor.Bounds, CanvasWidth, CanvasHeight), anchor.X, anchor.Y);
    }
}
