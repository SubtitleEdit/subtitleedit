using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.RemuxVideo;

/// <summary>
/// Pick one of the audio tracks of a video, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);

        DataContext = vm;

        var titleBlock = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(14),
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        };
        titleBlock.Bind(TextBlock.TextProperty, new Binding(nameof(PickAudioTrackViewModel.TitleText)));

        var messageBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            MaxWidth = 560,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        messageBlock.Bind(TextBlock.TextProperty, new Binding(nameof(PickAudioTrackViewModel.MessageText)));

        var comboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            DisplayMemberBinding = new Binding(nameof(AudioTrackOption.DisplayName)),
        };
        comboBox.Bind(ComboBox.ItemsSourceProperty, new Binding(nameof(PickAudioTrackViewModel.Tracks)));
        comboBox.Bind(ComboBox.SelectedItemProperty, new Binding(nameof(PickAudioTrackViewModel.SelectedTrack)));
        Avalonia.Automation.AutomationProperties.SetName(comboBox, Se.Language.Video.RemuxVideoSelectAudioTrack);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var panel = new StackPanel
        {
            Spacing = 10,
            Margin = new Avalonia.Thickness(16, 14, 16, 16),
            Children =
            {
                titleBlock,
                messageBlock,
                comboBox,
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Add(EditorChrome.MakePanel(panel), 0);
        grid.Add(buttonBar, 1);

        Content = grid;
    }
}
