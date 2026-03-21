using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class BurnInLogoViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public BurnInLogo BurnInLogo { get; set; }
    public string VideoFileName { get; set; }
    public int VideoWidth { get; set; }
    public int VideoHeight { get; set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl? VideoPlayerControl { get; set; }
    public double PositionInSeconds { get; set; }

    public Image? LogoImage { get; set; }
    public Grid? VideoGrid { get; set; }
    public Border? VideoContentBorder { get; set; }

    [ObservableProperty] private Bitmap? _logoOverlay;
    [ObservableProperty] private string _logoFileName = string.Empty;
    [ObservableProperty] private string _logoPositionText = string.Empty;
    [ObservableProperty] private bool _hasLogo;

    public BurnInLogoViewModel()
    {
        BurnInLogo = new BurnInLogo();
        VideoFileName = string.Empty;
    }

    [RelayCommand]
    private void Ok()
    {
        PositionInSeconds = VideoPlayerControl?.Position ?? 0;

        // Save logo settings
        BurnInLogo.LogoFileName = LogoFileName;
        BurnInLogo.X = BurnInLogo.X;
        BurnInLogo.Y = BurnInLogo.Y;

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task PickLogo()
    {
        if (Window == null)
        {
            return;
        }

        var fileHelper = new FileHelper();
        var fileName = await fileHelper.PickOpenFile(Window, Se.Language.General.OpenImageFile, "PNG files", "*.png", "All files", "*.*");
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return;
        }

        LogoFileName = fileName;
        await LoadLogo();
    }

    private async Task LoadLogo()
    {
        if (string.IsNullOrEmpty(LogoFileName) || !File.Exists(LogoFileName))
        {
            return;
        }

        try
        {
            // Load logo as Avalonia bitmap
            await using var stream = File.OpenRead(LogoFileName);
            LogoOverlay = new Bitmap(stream);

            HasLogo = true;

            // Set initial position (top-right corner inside the border)
            if (BurnInLogo.X == 0 && BurnInLogo.Y == 0)
            {
                BurnInLogo.X = VideoWidth - (int)LogoOverlay.Size.Width - 10;
                BurnInLogo.Y = 10;
            }

            UpdateOverlayPosition();
            UpdateOverlayOpacity();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading logo: {ex.Message}");
        }
    }

    public void UpdateOverlayPosition()
    {
        UpdateBorder();
        UpdateLogoPosition();
    }

    public void UpdateBorder()
    {
        if (VideoContentBorder == null || VideoPlayerControl == null || VideoWidth <= 0 || VideoHeight <= 0)
        {
            return;
        }

        // Use the actual content dimensions from the video player
        var contentWidth = VideoPlayerControl.ContentWidth;
        var contentHeight = VideoPlayerControl.ContentHeight;

        if (contentWidth <= 0 || contentHeight <= 0)
        {
            return;
        }

        // Calculate the actual video display area based on aspect ratio
        var screenAspect = (double)VideoWidth / VideoHeight;
        var contentAspect = (double)contentWidth / contentHeight;

        double rectWidth, rectHeight, rectX, rectY;

        if (contentAspect > screenAspect)
        {
            // Video is narrower than content space - pillarboxed
            rectHeight = contentHeight;
            rectWidth = rectHeight * screenAspect;
            rectX = (contentWidth - rectWidth) / 2;
            rectY = 0;
        }
        else
        {
            // Video is wider than content space - letterboxed
            rectWidth = contentWidth;
            rectHeight = rectWidth / screenAspect;
            rectX = 0;
            rectY = (contentHeight - rectHeight) / 2;
        }

        // Round to avoid sub-pixel rendering issues
        rectWidth = Math.Round(rectWidth);
        rectHeight = Math.Round(rectHeight);
        rectX = Math.Round(rectX);
        rectY = Math.Round(rectY);

        // Update the green border
        Dispatcher.UIThread.Post(() =>
        {
            VideoContentBorder.Width = rectWidth;
            VideoContentBorder.Height = rectHeight;
            VideoContentBorder.Margin = new Thickness(rectX, rectY, 0, 0);
        });
    }

    public void UpdateLogoPosition()
    {
        if (LogoImage == null || LogoOverlay == null || VideoContentBorder == null || VideoPlayerControl == null)
        {
            return;
        }

        var videoPlayerWidth = VideoPlayerControl.Bounds.Width;
        if (videoPlayerWidth <= 0 || VideoWidth <= 0)
        {
            return;
        }

        // Get border dimensions
        var rectWidth = VideoContentBorder.Width;
        var rectX = VideoContentBorder.Margin.Left;
        var rectY = VideoContentBorder.Margin.Top;

        if (rectWidth <= 0)
        {
            return;
        }

        // Calculate scale factor from video coordinates to screen coordinates
        var scale = rectWidth / VideoWidth;

        // Apply logo size scaling (percentage) to original logo dimensions
        var sizeScale = BurnInLogo.Size / 100.0;
        var scaledLogoWidth = LogoOverlay.Size.Width * sizeScale;
        var scaledLogoHeight = LogoOverlay.Size.Height * sizeScale;

        // Calculate scaled logo position relative to the border
        var left = rectX + (BurnInLogo.X * scale);
        var top = rectY + (BurnInLogo.Y * scale);

        // Calculate final screen size by applying video scale to the size-scaled logo
        var finalWidth = scaledLogoWidth * scale;
        var finalHeight = scaledLogoHeight * scale;

        // Update logo position and size
        Dispatcher.UIThread.Post(() =>
        {
            Canvas.SetLeft(LogoImage, left);
            Canvas.SetTop(LogoImage, top);
            LogoImage.Width = finalWidth;
            LogoImage.Height = finalHeight;
        });

        // Update position text
        LogoPositionText = $"X: {BurnInLogo.X}, Y: {BurnInLogo.Y}";

        UpdateOverlayOpacity();
    }

    public void UpdateOverlayOpacity()
    {
        if (LogoImage == null)
        {
            return;
        }

        // Convert alpha percentage (0-100) to opacity (0.0-1.0)
        var opacity = BurnInLogo.Alpha / 100.0;
        Dispatcher.UIThread.Post(() =>
        {
            LogoImage.Opacity = opacity;
        });
    }

    public void UpdateLogoSize()
    {
        UpdateOverlayPosition();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal async void OnLoaded()
    {
        if (!string.IsNullOrEmpty(BurnInLogo.LogoFileName) && File.Exists(BurnInLogo.LogoFileName))
        {
            LogoFileName = BurnInLogo.LogoFileName;
            await LoadLogo();
        }

        // Initialize video player if video file is available
        if (!string.IsNullOrEmpty(VideoFileName) && File.Exists(VideoFileName) && VideoPlayerControl != null)
        {
            await VideoPlayerControl.Open(VideoFileName);
            await VideoPlayerControl.WaitForPlayersReadyAsync();

            // Seek to a position to show a frame
            if (PositionInSeconds > 0)
            {
                VideoPlayerControl.Position = PositionInSeconds;
            }
            else
            {
                VideoPlayerControl.Position = 1.0; // Show frame at 1 second
            }

            VideoPlayerControl.VideoPlayerInstance.Pause();

            // Wait for the video player to render and have valid bounds
            // Try multiple times with increasing delays to ensure accurate border calculation
            for (int i = 0; i < 3; i++)
            {
                await Task.Delay(50 + (i * 50)); // 50ms, 100ms, 150ms
                UpdateBorder();
            }

            if (HasLogo)
            {
                UpdateLogoPosition();
            }
        }
    }

    internal void Initialize(string videoFileName, int videoWidth, int videoHeight)
    {
        VideoFileName = videoFileName;
        VideoWidth = videoWidth > 0 ? videoWidth : 1920;
        VideoHeight = videoHeight > 0 ? videoHeight : 1080;
    }
}