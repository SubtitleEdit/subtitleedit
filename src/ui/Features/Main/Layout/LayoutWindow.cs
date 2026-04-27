using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class LayoutWindow : Window
{
    private readonly LayoutViewModel _vm;
    private List<Border> _borders = new List<Border>();
    private int _focusedLayout = -1;

    public LayoutWindow(LayoutViewModel viewViewModel)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        _vm = viewViewModel;
        Title = "Choose layout";
        Width = 925;
        Height = 500;
        CanResize = false;

        var wrapPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10)
        };

        for (var i = 1; i <= 12; i++)
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
                FontSize = 34,
                FontWeight = FontWeight.Bold,
                Opacity = 0.7,
            };

            // Layer the image and text
            var grid = new Grid
            {
                Width = 200,
                Height = 139
            };

            grid.Children.Add(image);
            grid.Children.Add(text);

            var border = new Border
            {
                Margin = new Thickness(10),
                Child = grid,
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                RenderTransform = new ScaleTransform(1, 1)
            };

            var layoutNumber = i;

            border.PointerEntered += (_, __) =>
            {
                border.RenderTransform = new ScaleTransform(1.1, 1.1);
                border.Background = Brushes.DarkSlateGray;
                _focusedLayout = layoutNumber;
            };

            border.PointerExited += (_, __) =>
            {
                border.RenderTransform = new ScaleTransform(1.0, 1.0);
                border.Background = Brushes.Transparent;
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

        Content = new ScrollViewer { Content = wrapPanel };
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
            if (_focusedLayout > 0)
            {
                ResetOldLayout();

                _focusedLayout--;
                _borders[_focusedLayout].RenderTransform = new ScaleTransform(1.1, 1.1);
                _borders[_focusedLayout].Background = Brushes.DarkSeaGreen;
            }
            else
            {
                ResetOldLayout();

                _focusedLayout = 11;
                _borders[_focusedLayout].RenderTransform = new ScaleTransform(1.1, 1.1);
                _borders[_focusedLayout].Background = Brushes.DarkSeaGreen;
            }
            return;
        }

        if (e.Key == Key.Right)
        {
            e.Handled = true;
            if (_focusedLayout < 11)
            {
                ResetOldLayout();
                _focusedLayout++;
                _borders[_focusedLayout].RenderTransform = new ScaleTransform(1.1, 1.1);
                _borders[_focusedLayout].Background = Brushes.DarkSeaGreen;
            }
            else
            {
                ResetOldLayout();
                _focusedLayout = 0;
                _borders[_focusedLayout].RenderTransform = new ScaleTransform(1.1, 1.1);
                _borders[_focusedLayout].Background = Brushes.DarkSeaGreen;
            }
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
                _borders[fl].RenderTransform = new ScaleTransform(1.0, 1.0);
                _borders[fl].Background = Brushes.Transparent;
            }

            _borders[layoutNumber - 1].RenderTransform = new ScaleTransform(1.1, 1.1);
            _borders[layoutNumber - 1].Background = Brushes.DarkSeaGreen;
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
            _borders[i].RenderTransform = new ScaleTransform(1.0, 1.0);
            _borders[i].Background = Brushes.Transparent;
        }
    }
}