using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate;

public class CopyPasteTranslateBlockWindow : Window
{
    public CopyPasteTranslateBlockWindow(CopyPasteTranslateBlockViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(CopyPasteTranslateBlockViewModel.WindowTitle)) { Mode = BindingMode.OneWay });
        SizeToContent = SizeToContent.WidthAndHeight;
        vm.Window = this;
        DataContext = vm;

        var labelInfo = UiUtil.MakeLabel(Se.Language.Translate.BlockCopyInfo);

        var buttonGetFromClipboard = new Button
        {
            Content = Se.Language.Translate.BlockCopyGetFromClipboard,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(15),
            Margin = new Thickness(15),
            FontWeight = Avalonia.Media.FontWeight.Bold,
            FontSize = 16,
            Command = vm.CopyFromClipboardCommand,
        };

        var buttonOk = UiUtil.MakeButton(Se.Language.Translate.ReCopyTextToClipboard, vm.CopyFromClipboardCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelInfo, 0);
        grid.Add(buttonGetFromClipboard, 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
        Loaded += vm.Loaded;
    }
}
