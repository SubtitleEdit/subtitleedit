using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared;

public enum MessageBoxResult
{
    None,
    OK,
    Cancel,
    Yes,
    No,
    Custom1,
    Custom2,
    Custom3,
}

public enum MessageBoxButtons
{
    OK,
    OKCancel,
    YesNo,
    YesNoCancel,
    Cancel,
    Custom1,
    Custom2,
}

public enum MessageBoxIcon
{
    None,
    Information,
    Warning,
    Error,
    Question
}

public class MessageBox : Window
{
    private static string CustomButton1Text { get; set; } = string.Empty;
    private static string CustomButton2Text { get; set; } = string.Empty;
    private MessageBoxResult _result = MessageBoxResult.None;
    private readonly bool _hasCancel;
    private readonly bool _hasOnlyOk;    

    private MessageBox(string title, string message, MessageBoxButtons buttons, MessageBoxIcon icon, string? custom1 = null, string? custom2 = null, string? custom3 = null)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Width = 400;
        Height = 180;
        Title = title;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        CanResize = false;

        var message1 = message;

        var grid = new Grid
        {
            Margin = new Thickness(10),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
            }
        };

        // Icon (optional)
        var iconImage = new Image
        {
            Width = 48,
            Height = 48,
            Margin = new Thickness(10)
        };

        if (icon != MessageBoxIcon.None)
        {
            var iconPath = $"Assets/Themes/Dark/{icon}.png";
            try
            {
                iconImage.Source = new Bitmap(iconPath);
            }
            catch
            {
                // If icon not found, silently ignore
            }

            grid.Children.Add(iconImage);
            Grid.SetRow(iconImage, 0);
            Grid.SetColumn(iconImage, 0);
        }

        var textBlock = new TextBlock
        {
            Text = message,
            Margin = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Height = double.NaN,
            Width = double.NaN,
        };


        var x = TextMeasurer.MeasureString(message, textBlock.FontFamily.Name, (float)textBlock.FontSize);
        if (x.Width > Width - 100)
        {
            Width += 200;
            Height += 50;
        }

        if (message.Length > 1000)
        {
            var scrollView = new ScrollViewer
            {
                Width = double.NaN,
                Height = double.NaN,
                Margin = new Thickness(10),
                Content = textBlock
            };

            grid.Children.Add(scrollView);
            Grid.SetRow(scrollView, 0);
            Grid.SetColumn(scrollView, 1);
        }
        else
        {
            grid.Children.Add(textBlock);
            Grid.SetRow(textBlock, 0);
            Grid.SetColumn(textBlock, 1);
        }

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10)
        };

        void AddButton(string text, MessageBoxResult result)
        {
            var btn = UiUtil.MakeButton(text);
            btn.Click += (_, _) => { _result = result; Close(_result); };
            buttonPanel.Children.Add(btn);
        }

        if (!string.IsNullOrEmpty(custom1))
        {
            AddButton(custom1, MessageBoxResult.Custom1);
        }

        if (!string.IsNullOrEmpty(custom2))
        {
            AddButton(custom2, MessageBoxResult.Custom2);
        }

        if (!string.IsNullOrEmpty(custom3))
        {
            AddButton(custom3, MessageBoxResult.Custom3);
        }

        // Add buttons based on MessageBoxButtons
        switch (buttons)
        {
            case MessageBoxButtons.OK:
                AddButton(Se.Language.General.Ok, MessageBoxResult.OK);
                _hasOnlyOk = true;
                break;
            case MessageBoxButtons.OKCancel:
                AddButton(Se.Language.General.Ok, MessageBoxResult.OK);
                AddButton(Se.Language.General.Cancel, MessageBoxResult.Cancel);
                _hasCancel = true;
                break;
            case MessageBoxButtons.YesNo:
                AddButton(Se.Language.General.Yes, MessageBoxResult.Yes);
                AddButton(Se.Language.General.No, MessageBoxResult.No);
                break;
            case MessageBoxButtons.YesNoCancel:
                AddButton(Se.Language.General.Yes, MessageBoxResult.Yes);
                AddButton(Se.Language.General.No, MessageBoxResult.No);
                AddButton(Se.Language.General.Cancel, MessageBoxResult.Cancel);
                _hasCancel = true;
                break;
            case MessageBoxButtons.Cancel:
                AddButton(Se.Language.General.Cancel, MessageBoxResult.Cancel);
                _hasCancel = true;
                break;
        }

        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, 2);
        Grid.SetColumn(buttonPanel, 0);
        Grid.SetColumnSpan(buttonPanel, 2);

        if (!string.IsNullOrEmpty(custom1) ||  !string.IsNullOrEmpty(custom2))
        {
            _hasOnlyOk = false;
        }

        Content = grid;

        var contextMenu = new MenuFlyout
        {
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.General.CopyTextToClipboard,
                    Command = new RelayCommand(async() =>
                    {
                        await ClipboardHelper.SetTextAsync(this, message1);
                    })
                }
            }
        };
        grid.ContextFlyout = contextMenu;

        Activated += delegate { buttonPanel.Children[0].Focus(); }; // hack to make OnKeyDown work
    }

    public static async Task<MessageBoxResult> Show(
        Window owner,
        string title,
        string message,
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        MessageBoxIcon icon = MessageBoxIcon.None,
        string? custom1 = null,
        string? custom2 = null,
        string? custom3 = null)
    {
        var msgBox = new MessageBox(title, message, buttons, icon, custom1, custom2);
        return await msgBox.ShowDialog<MessageBoxResult>(owner);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape && _hasCancel)
        {
            _result = MessageBoxResult.Cancel;
            Close(_result);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && _hasOnlyOk)
        {
            _result = MessageBoxResult.OK;
            Close(_result);
            e.Handled = true;
        }
    }
}
