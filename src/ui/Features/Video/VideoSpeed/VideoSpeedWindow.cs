using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.VideoSpeed;

public class VideoSpeedWindow : Window
{
    private readonly VideoSpeedViewModel _vm;

    public VideoSpeedWindow(VideoSpeedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.VideoSpeedTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = true;
        MinWidth = 740;
        MinHeight = 560;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var fileView = MakeFileView(vm);
        var segmentConfigView = MakeSegmentConfigView(vm);
        var segmentsListView = MakeSegmentsListView(vm);
        var targetFileSizeView = MakeTargetFileSizeView(vm);
        var encodingView = MakeEncodingView(vm);
        var burnInView = MakeBurnInView(vm);
        var progressView = MakeProgressView(vm);

        var buttonGenerate = UiUtil.MakeButton(Se.Language.General.Generate, vm.GenerateCommand)
            .WithBindEnabled(nameof(vm.IsGenerating), InverseBooleanConverter.Instance);

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var buttonBar = UiUtil.MakeButtonBar(buttonGenerate, buttonCancel);

        var mainLayout = new StackPanel
        {
            Spacing = 10,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                fileView,
                segmentConfigView,
                segmentsListView,
                targetFileSizeView,
                encodingView,
                burnInView,
                progressView,
                buttonBar
            }
        };

        Content = new ScrollViewer
        {
            Content = mainLayout
        };

        Opened += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            var width = ClientSize.Width;
            var height = ClientSize.Height;
            SizeToContent = SizeToContent.Manual;
            if (width > 0 && height > 0)
            {
                Width = Math.Max(820, width);
                Height = Math.Max(680, height);
            }
        }, DispatcherPriority.Loaded);
    }

    private static Border MakeGroupBox(string title, Control content)
    {
        return new Border
        {
            BorderBrush = new SolidColorBrush(UiUtil.GetBorderColor()),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(10),
            Child = new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    new TextBlock { Text = title, FontWeight = FontWeight.Bold, Foreground = UiUtil.GetTextColor() },
                    content
                }
            }
        };
    }

    private Border MakeFileView(VideoSpeedViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            RowSpacing = 6,
            ColumnSpacing = 8
        };

        var labelInput = UiUtil.MakeLabel("Input Video:");
        var textBoxInput = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.InputVideoFileName));
        var buttonInput = UiUtil.MakeButtonBrowse(vm.BrowseInputVideoCommand, accessibleName: "Browse input video");

        grid.Add(labelInput, 0, 0);
        grid.Add(textBoxInput, 0, 1);
        grid.Add(buttonInput, 0, 2);

        var labelOutput = UiUtil.MakeLabel("Output Video:");
        var textBoxOutput = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.OutputVideoFileName));
        var buttonOutput = UiUtil.MakeButtonBrowse(vm.BrowseOutputVideoCommand, accessibleName: "Browse output video");

        grid.Add(labelOutput, 1, 0);
        grid.Add(textBoxOutput, 1, 1);
        grid.Add(buttonOutput, 1, 2);

        var labelVideoInfo = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.DodgerBlue,
            Margin = new Thickness(0, 2, 0, 0)
        };
        labelVideoInfo.Bind(TextBlock.TextProperty, new Binding(nameof(vm.VideoInfoText)));

        grid.Add(labelVideoInfo, 2, 1);

        return MakeGroupBox("1. Video Files", grid);
    }

    private Border MakeSegmentConfigView(VideoSpeedViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            RowSpacing = 6,
            ColumnSpacing = 8
        };

        var labelStart = UiUtil.MakeLabel("Start Time:");
        var timeUpDownStart = new TimeCodeUpDown
        {
            Width = 125,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.NewStartTime))
            {
                Mode = BindingMode.TwoWay
            }
        };

        var labelEnd = UiUtil.MakeLabel("End Time:");
        var timeUpDownEnd = new TimeCodeUpDown
        {
            Width = 125,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.NewEndTime))
            {
                Mode = BindingMode.TwoWay
            }
        };

        var rbTargetMode = new RadioButton
        {
            Content = "Target Duration (sec):",
            IsChecked = vm.IsTargetDurationMode
        };
        rbTargetMode.Bind(RadioButton.IsCheckedProperty, new Binding(nameof(vm.IsTargetDurationMode)));

        var numTargetSec = new NumericUpDown
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
            Minimum = 0.1m,
            Maximum = 86400m,
            Increment = 0.5m,
            FormatString = "0.0#",
            Width = 130,
            Foreground = UiUtil.GetTextColor(),
        };
        numTargetSec.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.NewTargetDurationSeconds),
            Mode = BindingMode.TwoWay,
            Converter = new NullableDoubleConverter { DefaultValue = 5.0 },
        });

        grid.Add(labelStart, 0, 0);
        grid.Add(timeUpDownStart, 0, 1);
        grid.Add(labelEnd, 0, 2);
        grid.Add(timeUpDownEnd, 0, 3);
        grid.Add(rbTargetMode, 0, 4);
        grid.Add(numTargetSec, 0, 5);

        var rbSpeedMode = new RadioButton
        {
            Content = "Speed Multiplier (x):",
            IsChecked = !vm.IsTargetDurationMode
        };

        var numSpeedMult = new NumericUpDown
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
            Minimum = 0.1m,
            Maximum = 500m,
            Increment = 0.1m,
            FormatString = "0.0",
            Width = 130,
            Foreground = UiUtil.GetTextColor(),
        };
        numSpeedMult.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.NewSpeedMultiplier),
            Mode = BindingMode.TwoWay,
            Converter = new NullableDoubleConverter { DefaultValue = 1.0 },
        });

        var buttonAdd = UiUtil.MakeButton("Add Segment", vm.AddSegmentCommand);
        var buttonUpdate = UiUtil.MakeButton("Update Segment", vm.UpdateSegmentCommand);

        var actionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { buttonAdd, buttonUpdate }
        };

        grid.Add(rbSpeedMode, 1, 4);
        grid.Add(numSpeedMult, 1, 5);
        grid.Add(actionPanel, 1, 6);

        return MakeGroupBox("2. Custom Segment Configuration (CapCut Target Duration / Multiplier)", grid);
    }

    private Border MakeSegmentsListView(VideoSpeedViewModel vm)
    {
        var listBox = new ListBox
        {
            Height = 140,
            ItemTemplate = new FuncDataTemplate<VideoSpeedSegmentItem>((item, _) =>
            {
                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(160, GridUnitType.Pixel) },
                        new ColumnDefinition { Width = new GridLength(140, GridUnitType.Pixel) },
                        new ColumnDefinition { Width = new GridLength(140, GridUnitType.Pixel) },
                        new ColumnDefinition { Width = new GridLength(120, GridUnitType.Pixel) }
                    },
                    Margin = new Thickness(4)
                };

                var txtTime = new TextBlock { Text = $"{item?.StartTime:hh\\:mm\\:ss\\.fff} ➔ {item?.EndTime:hh\\:mm\\:ss\\.fff}" };
                var txtOrig = new TextBlock { Text = $"Orig: {item?.OriginalDurationDisplay}" };
                var txtOut = new TextBlock { Text = $"Target: {item?.OutputDurationDisplay}" };
                var txtRatio = new TextBlock { Text = $"Speed: {item?.SpeedDisplay}", FontWeight = FontWeight.Bold, Foreground = Brushes.MediumSeaGreen };

                grid.Add(txtTime, 0, 0);
                grid.Add(txtOrig, 0, 1);
                grid.Add(txtOut, 0, 2);
                grid.Add(txtRatio, 0, 3);

                return grid;
            })
        };

        listBox.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Segments)));
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(nameof(vm.SelectedSegment)));

        var btnRemove = UiUtil.MakeButton("Remove Selected", vm.RemoveSegmentCommand);
        var btnMoveUp = UiUtil.MakeButton("Move Up ▲", vm.MoveUpSegmentCommand);
        var btnMoveDown = UiUtil.MakeButton("Move Down ▼", vm.MoveDownSegmentCommand);

        var buttonStack = new StackPanel
        {
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Top,
            Children = { btnRemove, btnMoveUp, btnMoveDown }
        };

        var containerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10
        };

        containerGrid.Add(listBox, 0, 0);
        containerGrid.Add(buttonStack, 0, 1);

        var textTotalTime = new TextBlock
        {
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.MediumSeaGreen,
            Margin = new Thickness(0, 0, 0, 6)
        };
        textTotalTime.Bind(TextBlock.TextProperty, new Binding(nameof(vm.TotalTimeInfo)));

        var mainPanel = new StackPanel
        {
            Spacing = 4,
            Children =
            {
                textTotalTime,
                containerGrid
            }
        };

        return MakeGroupBox("3. Segments List", mainPanel);
    }

    private static Border MakeTargetFileSizeView(VideoSpeedViewModel vm)
    {
        var checkBoxUseTargetFileSize = UiUtil.MakeCheckBox(Se.Language.Video.BurnIn.TargetFileSize, vm, nameof(vm.UseTargetFileSize));
        checkBoxUseTargetFileSize.IsCheckedChanged += (_, _) => vm.CalculateTargetFileBitRate();

        var checkBoxMatchSource = UiUtil.MakeCheckBox(Se.Language.Video.BurnIn.MatchSourceVideoSize, vm, nameof(vm.MatchSourceVideoSize))
            .WithMarginLeft(10);
        checkBoxMatchSource.IsCheckedChanged += (_, _) => vm.CalculateTargetFileBitRate();

        var labelTargetFileSize = UiUtil.MakeLabel(Se.Language.Video.BurnIn.FileSizeMb).WithMarginLeft(10);
        var numericUpDownTargetFileSize = UiUtil.MakeNumericUpDownInt(1, 1000_000_000, 0, 150, vm, nameof(vm.TargetFileSize));
        numericUpDownTargetFileSize.ValueChanged += (_, _) => vm.CalculateTargetFileBitRate();
        numericUpDownTargetFileSize.Bind(NumericUpDown.IsEnabledProperty, new Binding(nameof(vm.MatchSourceVideoSize)) { Converter = InverseBooleanConverter.Instance });

        var labelVideoBitRate = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.TargetVideoBitRateInfo));
        labelVideoBitRate.FontSize = UiUtil.ScaledFontSize(10);
        labelVideoBitRate.Opacity = 0.7;

        var panelTargetFileSize = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 8,
            Children =
            {
                numericUpDownTargetFileSize,
                labelVideoBitRate
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        grid.Add(checkBoxUseTargetFileSize, 0, 0, 1, 2);
        grid.Add(checkBoxMatchSource, 1, 0, 1, 2);
        grid.Add(labelTargetFileSize, 2, 0);
        grid.Add(panelTargetFileSize, 2, 1);

        return MakeGroupBox("4. Target File Size", grid);
    }

    private Border MakeEncodingView(VideoSpeedViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            RowSpacing = 6,
            ColumnSpacing = 10
        };

        // Row 0: Resolution & FPS
        var lblResolution = UiUtil.MakeLabel(Se.Language.General.Resolution);
        var labelWidth = UiUtil.MakeLabel("W");
        var textBoxWidth = UiUtil.MakeNumericUpDownInt(0, 10_000, 0, 140, vm, nameof(vm.VideoWidth));
        textBoxWidth.MinWidth = 130;
        var labelX = UiUtil.MakeLabel("x");
        var labelHeight = UiUtil.MakeLabel("H");
        var textBoxHeight = UiUtil.MakeNumericUpDownInt(0, 10_000, 0, 140, vm, nameof(vm.VideoHeight));
        textBoxHeight.MinWidth = 130;
        var buttonResolution = UiUtil.MakeButtonBrowse(vm.BrowseResolutionCommand, accessibleName: Se.Language.General.Resolution);

        var lblFps = UiUtil.MakeLabel(Se.Language.General.FrameRate).WithMarginLeft(15);
        var comboBoxFrameRate = UiUtil.MakeComboBox(vm.FrameRates, vm, nameof(vm.SelectedFrameRate)).WithFrameRateDisplay();
        comboBoxFrameRate.MinWidth = 110;
        comboBoxFrameRate.Width = 110;

        var panelResolution = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                labelWidth,
                textBoxWidth,
                labelX,
                labelHeight,
                textBoxHeight,
                buttonResolution,
                lblFps,
                comboBoxFrameRate,
            }
        };

        grid.Add(lblResolution, 0, 0);
        grid.Add(panelResolution, 0, 1, 1, 6);

        // Row 1: Video Encoding
        var lblEncoding = UiUtil.MakeLabel("Encoding:");
        var cbEncoding = UiUtil.MakeComboBox(vm.VideoEncodings, vm, nameof(vm.SelectedVideoEncoding));

        var lblPreset = UiUtil.MakeLabel("Preset:");
        var cbPreset = UiUtil.MakeComboBox(vm.VideoPresets, vm, nameof(vm.SelectedVideoPreset));

        var lblCrf = UiUtil.MakeLabel("CQ / Quality:");
        var cbCrf = UiUtil.MakeComboBox(vm.VideoCrfs, vm, nameof(vm.SelectedVideoCrf));
        cbCrf.Bind(ComboBox.IsEnabledProperty, new Binding(nameof(vm.UseTargetFileSize)) { Converter = InverseBooleanConverter.Instance });

        grid.Add(lblEncoding, 1, 0);
        grid.Add(cbEncoding, 1, 1);
        grid.Add(lblPreset, 1, 2);
        grid.Add(cbPreset, 1, 3);
        grid.Add(lblCrf, 1, 4);
        grid.Add(cbCrf, 1, 5);

        // Row 2: Audio Encoding
        var lblAudioEnc = UiUtil.MakeLabel("Audio encoding:");
        var cbAudioEnc = UiUtil.MakeComboBox(vm.AudioEncodings, vm, nameof(vm.SelectedAudioEncoding));

        var lblSampleRate = UiUtil.MakeLabel("Sample rate:");
        var cbSampleRate = UiUtil.MakeComboBox(vm.AudioSampleRates, vm, nameof(vm.SelectedAudioSampleRate));

        var lblBitRate = UiUtil.MakeLabel("Bit rate:");
        var cbBitRate = UiUtil.MakeComboBox(vm.AudioBitRates, vm, nameof(vm.SelectedAudioBitRate));

        var cbStereo = UiUtil.MakeCheckBox("Stereo", vm, nameof(vm.AudioIsStereo));

        grid.Add(lblAudioEnc, 2, 0);
        grid.Add(cbAudioEnc, 2, 1);
        grid.Add(lblSampleRate, 2, 2);
        grid.Add(cbSampleRate, 2, 3);
        grid.Add(lblBitRate, 2, 4);
        grid.Add(cbBitRate, 2, 5);
        grid.Add(cbStereo, 2, 6);

        return MakeGroupBox("5. Video & Audio Encoding Settings (GPU Acceleration)", grid);
    }

    private Border MakeBurnInView(VideoSpeedViewModel vm)
    {
        var cbBurnIn = UiUtil.MakeCheckBox(Se.Language.Video.VideoSpeedBurnInCurrentSubtitle, vm, nameof(vm.BurnInSubtitle));
        return MakeGroupBox("6. Subtitle Options", cbBurnIn);
    }

    private Grid MakeProgressView(VideoSpeedViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar(14);
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        var textBlockProgressStatus = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.ProgressText)).WithBold();
        var textBlockProgressTime = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.ProgressTimeInfo));

        var timeRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 20,
            Children =
            {
                textBlockProgressStatus,
                textBlockProgressTime
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            RowSpacing = 6
        };

        grid.Add(progressBar, 0, 0);
        grid.Add(timeRow, 1, 0);

        return grid;
    }
}
