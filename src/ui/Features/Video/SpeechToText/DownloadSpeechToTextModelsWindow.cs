using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public class DownloadSpeechToTextModelsWindow : Window
{
    public DownloadSpeechToTextModelsWindow(DownloadSpeechToTextModelsViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AudioToText.DownloadingSpeechToTextModel;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 500;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.SpeechToText);

        DataContext = vm;

        var comboBoxModel = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel)).WithMinWidth(200);
        comboBoxModel.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboBoxModel, Se.Language.Video.AudioToText.SelectModel);

        var buttonDownload = EditorChrome.AsPrimary(UiUtil.MakeButton(Se.Language.General.Download, vm.DownloadCommand));
        buttonDownload.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.DownloadIsEnabled)));

        var buttonOpenFolder = EditorChrome.CreateToolButton("fa-solid fa-folder-open", Se.Language.General.OpenContainingFolder, vm.OpenModelFolderCommand, 28);

        var buttonAddCustomModel = UiUtil.MakeButton(Se.Language.Video.AudioToText.AddCustomModelDotDotDot, vm.AddCustomModelCommand)
            .WithIconLeft("fa-solid fa-plus");
        buttonAddCustomModel.Margin = new Thickness(0);
        buttonAddCustomModel.HorizontalAlignment = HorizontalAlignment.Left;
        buttonAddCustomModel.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.SupportsCustomModels)));

        var labelCustomModelHelp = new TextBlock
        {
            Text = Se.Language.Video.AudioToText.CustomModelHelp,
            TextWrapping = TextWrapping.Wrap,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            MaxWidth = 460,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        labelCustomModelHelp.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.SupportsCustomModels)));
        labelCustomModelHelp.Bind(TextBlock.TextProperty, new Binding(nameof(vm.CustomModelHelpText)));

        var modelRows = new StackPanel { Spacing = 8 };
        modelRows.Children.Add(SpeechToTextChrome.WithTrailing(comboBoxModel, buttonOpenFolder));
        modelRows.Children.Add(buttonAddCustomModel);
        modelRows.Children.Add(labelCustomModelHelp);


        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Margin = new Thickness(0, 0, 0, 6);
        progressBar.Width = double.NaN;
        progressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.OpacityProperty, new Binding(nameof(vm.ProgressOpacity)));
        var statusText = new TextBlock
        {
            Margin = new Thickness(0, 1, 0, 1),
            FontSize = EditorChrome.FieldFontSize,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));
        statusText.Bind(TextBlock.OpacityProperty, new Binding(nameof(vm.ProgressOpacity)));

        // The view models set Error on every failure path, but nothing rendered it - the user saw
        // only the generic "Download failed" while the real cause was written to a property no
        // window bound. DownloadVideoFromUrlWindow and DownloadCrispEmbedWindow show it this way.
        var errorText = new TextBlock
        {
            Foreground = EditorChrome.ErrorTextBrush,
            TextWrapping = TextWrapping.Wrap,
        };
        errorText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Error)));
        errorText.Bind(IsVisibleProperty, new Binding(nameof(vm.Error))
        {
            Converter = new Avalonia.Data.Converters.FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s)),
        });

        var fileText = new TextBlock
        {
            Margin = new Thickness(0, 1, 0, 1),
            FontSize = EditorChrome.FieldFontSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = EditorChrome.FaintTextBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        fileText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressFileName)));

        var panelStatus = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(16, 14, 16, 14),
            Children =
            {
                progressBar,
                statusText,
                errorText,
                fileText,
            }
        };

        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonCancel, buttonDownload);

        Content = new StackPanel
        {
            Children =
            {
                new Border
                {
                    Child = EditorChrome.MakeEditorSection(Se.Language.Video.AudioToText.SelectModel, modelRows, first: true),
                    Background = EditorChrome.PanelBrush,
                    BorderBrush = EditorChrome.LineBrush,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                },
                panelStatus,
                buttonBar,
            },
        };

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
        });
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}