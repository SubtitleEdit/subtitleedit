using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

/// <summary>
/// The layout picker in the editor look (see <see cref="EditorChrome"/>): the layout thumbnails as
/// cards on the panel color, each with its number in a badge, the hovered/keyboard-picked card
/// lifted in the accent, and Cancel in the button bar.
/// </summary>
public class LayoutWindow : Window
{
    private readonly LayoutViewModel _vm;
    private List<Border> _borders = new List<Border>();
    private const int LayoutCount = 15;
    private int _focusedLayout = -1;

    public LayoutWindow(LayoutViewModel viewViewModel)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        _vm = viewViewModel;
        Title = Se.Language.Options.Shortcuts.GeneralChooseLayout;
        Width = 1180;
        Height = 590;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);

        var wrapPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(14),
        };

        for (var i = 1; i <= LayoutCount; i++)
        {
            var uri = new Uri($"avares://SubtitleEdit/Assets/Layout/Layout{i:D2}.png");

            var image = new Image
            {
                Source = new Bitmap(AssetLoader.Open(uri)),
                Width = 200,
                Height = 139
            };

            var text = new TextBlock
            {
                Text = i.ToString(CultureInfo.InvariantCulture),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.White,
                FontSize = EditorChrome.FieldFontSize,
                FontWeight = FontWeight.Bold,
            };
            var badge = new Border
            {
                MinWidth = 26,
                Height = EditorChrome.ScaledSize(26),
                CornerRadius = new CornerRadius(13),
                Padding = new Thickness(6, 0),
                Background = new SolidColorBrush(Color.FromArgb(0xb0, 0x10, 0x12, 0x18)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(6),
                Child = text,
            };

            // Layer the image and the number badge
            var grid = new Grid
            {
                Width = 200,
                Height = 139
            };

            grid.Children.Add(image);
            grid.Children.Add(badge);

            var border = new Border
            {
                Margin = new Thickness(6),
                Padding = new Thickness(6),
                Child = grid,
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(1),
                Cursor = new Cursor(StandardCursorType.Hand),
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                RenderTransform = new ScaleTransform(1, 1)
            };
            AutomationProperties.SetName(border, Se.Language.Options.Shortcuts.GeneralChooseLayout + " " + i.ToString(CultureInfo.InvariantCulture));
            SetHighlight(border, false);

            var layoutNumber = i;

            border.PointerEntered += (_, __) =>
            {
                SetHighlight(border, true);
                _focusedLayout = layoutNumber - 1;
            };

            border.PointerExited += (_, __) =>
            {
                SetHighlight(border, false);
                _focusedLayout = -1;
            };

            border.PointerPressed += (_, __) =>
            {
                _vm.SelectedLayout = layoutNumber;
                _vm.OkPressed = true;
                Close();
            };

            wrapPanel.Children.Add(border);
            _borders.Add(border);
        }

        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel);
        buttonCancel.Click += (_, _) => Close();

        Content = EditorChrome.MakeRoot(new ScrollViewer { Content = wrapPanel }, EditorChrome.MakeDockedButtonBar(null, buttonCancel));

        // Tunnel, so the scroll viewer doesn't take Up/Down for scrolling and a focused button doesn't take Enter
        AddHandler(KeyDownEvent, (_, e) =>
        {
            if (e.KeyModifiers != KeyModifiers.None)
            {
                return;
            }

            if (e.Key is Key.Up or Key.Down)
            {
                e.Handled = true;
                MoveVertically(e.Key == Key.Up ? -1 : 1);
            }
            else if (e.Key == Key.Enter && _focusedLayout >= 0)
            {
                e.Handled = true;
                _vm.SelectedLayout = _focusedLayout + 1;
                _vm.OkPressed = true;
                Close();
            }
        }, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// Moves the picked card one row up/down in the wrap panel, keeping the column.
    /// </summary>
    private void MoveVertically(int direction)
    {
        if (_focusedLayout < 0)
        {
            ResetOldLayout();
            _focusedLayout = direction > 0 ? 0 : LayoutCount - 1;
            SetHighlight(_borders[_focusedLayout], true);
            _borders[_focusedLayout].BringIntoView();
            return;
        }

        var columns = 0;
        foreach (var border in _borders)
        {
            if (Math.Abs(border.Bounds.Y - _borders[0].Bounds.Y) > 1)
            {
                break;
            }

            columns++;
        }

        if (columns <= 0)
        {
            return;
        }

        var newIndex = _focusedLayout + direction * columns;
        if (newIndex < 0 || newIndex >= LayoutCount)
        {
            return;
        }

        ResetOldLayout();
        _focusedLayout = newIndex;
        SetHighlight(_borders[_focusedLayout], true);
        _borders[_focusedLayout].BringIntoView();
    }

    /// <summary>
    /// The picked card is lifted: a little larger, soft accent behind it, accent border.
    /// </summary>
    private static void SetHighlight(Border border, bool isHighlighted)
    {
        border.RenderTransform = isHighlighted ? new ScaleTransform(1.06, 1.06) : new ScaleTransform(1.0, 1.0);
        border.Background = isHighlighted ? EditorChrome.AccentSoftBrush : EditorChrome.FieldBrush;
        border.BorderBrush = isHighlighted ? EditorChrome.AccentBrush : EditorChrome.FieldBorderBrush;
        border.ZIndex = isHighlighted ? 1 : 0;
    }

    protected override async void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
            return;
        }

        if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/main-window", "layouts");
            return;
        }

        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            if (_focusedLayout >= 0)
            {
                _vm.SelectedLayout = _focusedLayout + 1;
                _vm.OkPressed = true;
                Close();
            }
            return;
        }

        if (e.Key == Key.Left)
        {
            e.Handled = true;
            ResetOldLayout();
            _focusedLayout = _focusedLayout > 0 ? _focusedLayout - 1 : LayoutCount - 1;
            SetHighlight(_borders[_focusedLayout], true);
            return;
        }

        if (e.Key == Key.Right)
        {
            e.Handled = true;
            ResetOldLayout();
            _focusedLayout = _focusedLayout < LayoutCount - 1 ? _focusedLayout + 1 : 0;
            SetHighlight(_borders[_focusedLayout], true);
            return;
        }

        var layoutLookup = new Dictionary<Key, int>
        {
            { Key.D1, 1 },
            { Key.D2, 2 },
            { Key.D3, 3 },
            { Key.D4, 4 },
            { Key.D5, 5 },
            { Key.D6, 6 },
            { Key.D7, 7 },
            { Key.D8, 8 },
            { Key.D9, 9 },
        };
        if (layoutLookup.TryGetValue(e.Key, out var layoutNumber))
        {
            var fl = _focusedLayout;
            if (fl >= 0)
            {
                SetHighlight(_borders[fl], false);
            }

            SetHighlight(_borders[layoutNumber - 1], true);
            await Task.Delay(500);

            _vm.SelectedLayout = layoutNumber;
            _vm.OkPressed = true;
            Close();
        }
    }

    private void ResetOldLayout()
    {
        for (var i = 0; i < _borders.Count; i++)
        {
            SetHighlight(_borders[i], false);
        }
    }
}
