using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.RemuxVideo;

public class PickAudioTrackWindow : Window
{
    public PickAudioTrackWindow(PickAudioTrackViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.RemuxVideoSelectAudioTrack;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        MinWidth = 500;

        DataContext = vm;

        var titleBlock = new TextBlock
        {
            FontSize = 14,
            FontWeight = FontWeight.Bold,
            TextWrapping = TextWrapping.Wrap,
        };
        titleBlock.Bind(TextBlock.TextProperty, new Binding(nameof(PickAudioTrackViewModel.TitleText)));

        var messageBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
        };
        messageBlock.Bind(TextBlock.TextProperty, new Binding(nameof(PickAudioTrackViewModel.MessageText)));

        var comboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            DisplayMemberBinding = new Binding(nameof(AudioTrackOption.DisplayName)),
        };
        comboBox.Bind(ComboBox.ItemsSourceProperty, new Binding(nameof(PickAudioTrackViewModel.Tracks)));
        comboBox.Bind(ComboBox.SelectedItemProperty, new Binding(nameof(PickAudioTrackViewModel.SelectedTrack)));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        Content = new StackPanel
        {
            Spacing = 10,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                titleBlock,
                messageBlock,
                comboBox,
                buttonBar,
            }
        };
    }
}
