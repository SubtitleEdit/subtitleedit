using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.ShowImage;

public partial class ShowImageViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private Bitmap? _previewImage;
    [ObservableProperty] private string _text;

    public Window? Window { get; set; }
    public bool LeftPressed { get; set; }
    public bool RightPressed { get; set; }

    /// <summary>
    /// Steps to the previous (-1) or next (+1) image and returns it, or null at either end.
    /// When set, Left/Right (Up/Down) swap the image in place instead of closing the window,
    /// so the caller doesn't reopen it for every step (which flickered).
    /// </summary>
    public Func<int, ShowImageItem?>? Navigate { get; set; }

    private readonly IFileHelper _fileHelper;

    public ShowImageViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        Title = string.Empty;
        Text = string.Empty;
        PreviewImage = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
    }

    internal void Initialize(string title, Bitmap bitmap, string text = "")
    {
        Title = title;
        PreviewImage = bitmap;
        Text = text;

        if (Window != null) 
        {
            Window.Width = bitmap.PixelSize.Width + 40;
            Window.Height = bitmap.PixelSize.Height + 150;
        }   
    }

    [RelayCommand]
    private async Task CopyImageToClipboard()
    {
        if (PreviewImage == null || Window == null || Window.Clipboard == null)
        {
            return;
        }

        await ClipboardHelper.CopyImageToClipboard(PreviewImage);
    }

    [RelayCommand]
    private async Task SaveImageAs()
    {
        if (PreviewImage == null || Window == null || Window.Clipboard == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveSubtitleFile(Window!, ".png", "image", Se.Language.General.SaveImageAs);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        PreviewImage.Save(fileName, PngBitmapEncoderOptions.Default);
    }


    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Left || e.Key == Key.Up)
        {
            e.Handled = true;
            Step(-1);
        }
        else if (e.Key == Key.Right || e.Key == Key.Down)
        {
            e.Handled = true;
            Step(1);
        }
    }

    private void Step(int direction)
    {
        if (Navigate != null)
        {
            if (Navigate(direction) is { } item)
            {
                Title = item.Title;
                PreviewImage = item.Bitmap;
                Text = item.Text;
            }

            return;
        }

        LeftPressed = direction < 0;
        RightPressed = direction > 0;
        Window?.Close();
    }

    internal void Loaded()
    {
        UiUtil.RestoreWindowPosition(Window);
    }

    internal void Closing(WindowClosingEventArgs e)
    {
        UiUtil.SaveWindowPosition(Window);
    }
}

public record ShowImageItem(string Title, Bitmap Bitmap, string Text = "");
