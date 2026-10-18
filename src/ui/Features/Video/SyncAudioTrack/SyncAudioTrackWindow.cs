using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.SyncAudioTrack;

/// <summary>
/// Sync audio track, in the editor look (see <see cref="EditorChrome"/>): the video playing with the
/// offset applied live on top, the audio track (when there is more than one) and the offset with
/// nudge buttons below, Cancel and Save video in the button bar.
/// </summary>
public class SyncAudioTrackWindow : Window
{
    public SyncAudioTrackWindow(SyncAudioTrackViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.SyncAudioTrackTitle;
        CanResize = true;
        Width = 900;
        Height = 700;
        MinWidth = 600;
        MinHeight = 450;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        vm.VideoPlayer = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayer.FullScreenIsVisible = false;

        var lang = Se.Language.Video;

        // Audio track
        var labelAudioTrack = EditorChrome.MakeDimLabel(lang.SyncAudioTrackAudioTrack, new Thickness(0, 0, 8, 0));
        var comboBoxAudioTrack = UiUtil.MakeComboBox(vm.AudioTracks, vm, nameof(vm.SelectedAudioTrack))
            .WithMinWidth(320)
            .WithAccessibleName(lang.SyncAudioTrackAudioTrack);
        comboBoxAudioTrack.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.AudioTracks)));
        var panelAudioTrack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10),
            Children = { labelAudioTrack, comboBoxAudioTrack },
        };
        panelAudioTrack.Bind(IsVisibleProperty, new Binding(nameof(vm.IsAudioTrackPickerVisible)));

        // Offset
        var labelOffset = EditorChrome.MakeDimLabel(lang.SyncAudioTrackOffset, new Thickness(0, 0, 8, 0));
        var numericUpDownOffset = UiUtil.MakeNumericUpDownInt(-600000, 600000, 0, 130, vm, nameof(vm.OffsetMs))
            .WithAccessibleName(lang.SyncAudioTrackOffset);
        numericUpDownOffset.Increment = 10;
        numericUpDownOffset.Classes.Add("field"); // the compact field, not the theme's big spinner
        var panelOffset = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                labelOffset,
                numericUpDownOffset,
                MakeNudgeButton(vm, "-1000"),
                MakeNudgeButton(vm, "-100"),
                MakeNudgeButton(vm, "-10"),
                MakeNudgeButton(vm, "+10"),
                MakeNudgeButton(vm, "+100"),
                MakeNudgeButton(vm, "+1000"),
                EditorChrome.MakeTextToolButton(IconNames.Restore, Se.Language.General.Reset, vm.ResetOffsetCommand),
            },
        };

        var textHint = new TextBlock
        {
            Text = lang.SyncAudioTrackHint,
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap,
            Foreground = EditorChrome.DimTextBrush,
            Margin = new Thickness(0, 10, 0, 0),
        };
        var textPreviewNeedsMpv = new TextBlock
        {
            Text = lang.SyncAudioTrackPreviewNeedsMpv,
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0),
        };
        textPreviewNeedsMpv.Bind(IsVisibleProperty, new Binding(nameof(vm.IsPreviewNotSupported)));
        var textError = new TextBlock
        {
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0),
        };
        textError.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ErrorText)));

        var controls = new StackPanel
        {
            Margin = new Thickness(12, 4, 12, 8),
            Children = { panelAudioTrack, panelOffset, textHint, textPreviewNeedsMpv, textError },
        };

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        var progressView = EditorChrome.MakeProgressStrip(progressBar, nameof(vm.ProgressText), nameof(vm.IsSaving));

        var buttonSave = UiUtil.MakeButton(lang.SyncAudioTrackSaveVideoDotDotDot, vm.SaveVideoCommand);
        buttonSave.Classes.Add("primary");
        buttonSave.Bind(IsEnabledProperty, new Binding(nameof(vm.IsSaving)) { Converter = InverseBooleanConverter.Instance });
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonCancel, buttonSave);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
        };

        // The player in a plain border - no corner radius or clipping, the macOS native video
        // surface disappears inside either
        grid.Add(new Border
        {
            Child = EditorChrome.MakeVideoFrame(vm.VideoPlayer),
            Padding = new Thickness(12),
        }, 0, 0);
        grid.Add(controls, 1, 0);
        grid.Add(progressView, 2, 0);
        grid.Add(buttonBar, 3, 0);

        Content = grid;

        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, _) => vm.OnClosing();
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Button MakeNudgeButton(SyncAudioTrackViewModel vm, string milliseconds)
    {
        var button = UiUtil.MakeButton(milliseconds, vm.NudgeCommand);
        button.CommandParameter = milliseconds;
        button.MinWidth = 0;
        button.Padding = new Thickness(8, 4);
        Avalonia.Automation.AutomationProperties.SetName(button, milliseconds + " ms");
        return button;
    }
}
