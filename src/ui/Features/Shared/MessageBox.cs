using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Threading.Tasks;
using Attached = Optris.Icons.Avalonia.Attached;

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
    private readonly bool _hasNo;

    private MessageBox(string title, string message, MessageBoxButtons buttons, MessageBoxIcon icon, string? custom1 = null, string? custom2 = null, string? custom3 = null)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = title;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 400;
        MinHeight = 150;
        MaxWidth = 900;
        MaxHeight = 700;

        var message1 = message;

        var grid = new Grid
        {
            Margin = new Thickness(16),
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

        // Most call sites pass an icon; for the few that don't, derive one from the buttons so
        // a question is visibly a question and a plain notification reads as information.
        if (icon == MessageBoxIcon.None)
        {
            icon = buttons switch
            {
                MessageBoxButtons.YesNo or MessageBoxButtons.YesNoCancel or MessageBoxButtons.OKCancel => MessageBoxIcon.Question,
                MessageBoxButtons.OK => MessageBoxIcon.Information,
                _ => MessageBoxIcon.None,
            };
        }

        if (icon != MessageBoxIcon.None)
        {
            var (iconName, iconColor) = icon switch
            {
                MessageBoxIcon.Warning => (IconNames.Alert, Color.FromRgb(0xF5, 0x9E, 0x0B)),
                MessageBoxIcon.Error => (IconNames.AlertCircle, Color.FromRgb(0xEF, 0x44, 0x44)),
                MessageBoxIcon.Question => (IconNames.HelpCircle, Color.FromRgb(0x3B, 0x82, 0xF6)),
                _ => (IconNames.Information, Color.FromRgb(0x3B, 0x82, 0xF6)),
            };

            var iconControl = new ContentControl
            {
                FontSize = 38,
                Foreground = new SolidColorBrush(iconColor),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 16, 0),
            };
            Attached.SetIcon(iconControl, iconName);

            grid.Children.Add(iconControl);
            Grid.SetRow(iconControl, 0);
            Grid.SetColumn(iconControl, 0);
        }

        // Selectable so users can copy a file name or error detail directly from the text
        // (the context menu's "copy text to clipboard" still copies the whole message).
        var textBlock = new SelectableTextBlock
        {
            Text = message,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 700, // wrap long messages instead of growing the window unbounded
        };

        // Always host the text in a ScrollViewer: it sizes to the text and stays vertically
        // centered next to the icon, and long messages scroll instead of overflowing.
        var scrollView = new ScrollViewer
        {
            MaxHeight = 400,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12),
            Content = textBlock,
        };

        grid.Children.Add(scrollView);
        Grid.SetRow(scrollView, 0);
        Grid.SetColumn(scrollView, 1);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 8, 0, 0),
        };

        void AddButton(string text, MessageBoxResult result)
        {
            var btn = UiUtil.MakeButton(text);
            btn.IsDefault = buttonPanel.Children.Count == 0; // Enter always confirms the primary choice
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
                _hasNo = true;
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
        Grid.SetRow(buttonPanel, 1);
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
        UiUtil.AttachMacContextFlyoutHandler(this, grid);

        Activated += delegate { buttonPanel.Children[0].Focus(); }; // hack to make OnKeyDown work

        UiTheme.ApplyScaleToWindow(this);
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
        var msgBox = new MessageBox(title, message, buttons, icon, custom1, custom2, custom3);

        // Keep the message box above undocked tool windows (audio visualizer / video player),
        // which float on top of the main window via KeepTopmostWhileOwnerActive. Without this the
        // message box opens behind them in undocked mode. (#12268)
        WindowService.KeepTopmostWhileOwnerActive(msgBox, owner);

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
        else if (e.Key == Key.Escape && _hasNo)
        {
            _result = MessageBoxResult.No;
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
