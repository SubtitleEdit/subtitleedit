using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

/// <summary>
/// The layout picker in the editor look (see <see cref="EditorChrome"/>): the layout thumbnails as
/// cards on the panel color, each with its number under it, the layout in use marked in the accent
/// with a check, the hovered/keyboard-picked card lifted, and Cancel in the button bar.
/// </summary>
public class LayoutWindow : Window
{
    private readonly LayoutViewModel _vm;
    private List<Border> _borders = new List<Border>();
    private static readonly int LayoutCount = LayoutThumbnail.Count;
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

        var currentLayout = Se.Settings.General.LayoutNumber;
        for (var i = 1; i <= LayoutCount; i++)
        {
            var isCurrent = i == currentLayout;
            var text = new TextBlock
            {
                Text = i.ToString(CultureInfo.InvariantCulture),
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = isCurrent ? EditorChrome.AccentBrush : EditorChrome.DimTextBrush,
                FontSize = EditorChrome.ScaledFont(12),
                FontWeight = FontWeight.SemiBold,
            };

            var thumbnail = new Grid { RowDefinitions = new RowDefinitions("Auto,Auto"), RowSpacing = 7 };
            thumbnail.Add(LayoutThumbnail.Make(i), 0, 0);
            thumbnail.Add(text, 1, 0);

            var content = new Grid();
            content.Children.Add(thumbnail);
            if (isCurrent)
            {
                content.Children.Add(MakeCurrentBadge());
            }

            var border = new Border
            {
                Margin = new Thickness(5),
                Padding = new Thickness(10, 10, 10, 7),
                Child = content,
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(isCurrent ? 2 : 1),
                Cursor = new Cursor(StandardCursorType.Hand),
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                RenderTransform = new ScaleTransform(1, 1)
            };
            AutomationProperties.SetName(border, Se.Language.Options.Shortcuts.GeneralChooseLayout + " " + i.ToString(CultureInfo.InvariantCulture));
            _borders.Add(border);
            var layoutNumber = i;
            SetHighlight(border, false);

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

    private static Border MakeCurrentBadge()
    {
        return new Border
        {
            Width = 22,
            Height = 22,
            CornerRadius = new CornerRadius(11),
            Background = EditorChrome.AccentBrush,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, -6, -6, 0),
            Child = new Optris.Icons.Avalonia.Icon
            {
                Value = IconNames.Check,
                FontSize = 11,
                Foreground = EditorChrome.OnAccentBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
    }

    /// <summary>
    /// The picked card is lifted: a little larger, on the hover color. The layout in use stays in
    /// the accent, picked or not.
    /// </summary>
    private void SetHighlight(Border border, bool isHighlighted)
    {
        var isCurrent = _borders.IndexOf(border) + 1 == Se.Settings.General.LayoutNumber;
        border.RenderTransform = isHighlighted ? new ScaleTransform(1.06, 1.06) : new ScaleTransform(1.0, 1.0);
        border.Background = isCurrent ? EditorChrome.AccentSoftBrush : isHighlighted ? EditorChrome.HoverBrush : EditorChrome.PanelBrush;
        border.BorderBrush = isCurrent ? EditorChrome.AccentBrush : isHighlighted ? EditorChrome.DimTextBrush : EditorChrome.LineBrush;
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
