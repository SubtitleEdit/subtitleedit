using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;

public class GetDictionariesWindow : Window
{
    public GetDictionariesWindow(GetDictionariesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.GetDictionariesTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = Se.Language.SpellCheck.GetDictionaryInstructions,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0),
        };

        var combo = new ComboBox
        {
            ItemsSource = vm.Dictionaries,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 180,
            Margin = new Thickness(0, 10, 10, 2),
            [!ComboBox.IsEnabledProperty] = new Binding(nameof(vm.IsDownloadEnabled)),
            [!ComboBox.SelectedValueProperty] = new Binding(nameof(vm.SelectedDictionary)),
        };

        var buttonDownload = UiUtil
            .MakeButton(Se.Language.General.Download, vm.DownloadCommand)
            .WithLeftAlignment()
            .WithMargin(0, 10, 10, 2)
            .WithBindEnabled(nameof(vm.IsDownloadEnabled));

        var panelDownload = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                combo,
                buttonDownload
            }
        };

        var labelDescription = new Label
        {
            Content = "Description:",
            VerticalAlignment = VerticalAlignment.Center,
            [!Label.ContentProperty] = new Binding(nameof(vm.SelectedDictionary) + "." + nameof(vm.Description)),
        };

        var sliderProgress = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            MinWidth = 400,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    }
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 6.0)
                    }
                },
            },
            [!Slider.OpacityProperty] = new Binding(nameof(vm.ProgressOpacity)),
            [!Slider.ValueProperty] = new Binding(nameof(vm.Progress)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };

        var labelDownloadStatus = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Width = double.NaN,
            Margin = new Thickness(0, 20, 0, 20),
            [!Label.ContentProperty] = new Binding(nameof(vm.StatusText)),
            [!Label.IsVisibleProperty] = new Binding(nameof(vm.IsProgressVisible)),
        };

        var linkOpenFolder = UiUtil.MakeLink(Se.Language.General.OpenDictionaryFolder, vm.OpenFolderCommand);

        var buttonOk = UiUtil.MakeButtonDone(vm.OkCommand);
        buttonOk.WithBindIsVisible(nameof(vm.IsDownloadEnabled));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        buttonCancel.WithBindIsVisible(nameof(vm.IsProgressVisible));
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(label, 0, 0);
        grid.Add(panelDownload, 1, 0);
        grid.Add(labelDescription, 2, 0);
        grid.Add(sliderProgress, 3, 0);
        grid.Add(labelDownloadStatus, 3, 0);
        grid.Add(linkOpenFolder, 4, 0);
        grid.Add(panelButtons, 4, 0);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
