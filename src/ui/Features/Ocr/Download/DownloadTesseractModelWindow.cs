using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

public class DownloadTesseractModelWindow : Window
{
    public DownloadTesseractModelWindow(DownloadTesseractModelViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.General.DownloadingX, "Tesseract model");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.EngineDialog);

        DataContext = vm;

        // Before the download: pick the dictionary. While downloading: title, progress and status.
        var titleText = EditorChrome.MakeDownloadTitle(string.Format(Se.Language.General.DownloadingX, "Tesseract model"));
        titleText.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.IsProgressVisible)));

        var comboBoxDictionary = new ComboBox
        {
            MinWidth = 440,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsTextSearchEnabled = true,
            ItemsSource = vm.TesseractDictionaryItems,
            SelectedItem = vm.SelectedTesseractDictionaryItem
        }.WithBindSelected(nameof(vm.SelectedTesseractDictionaryItem));
        var dictionaryLabel = Se.Language.Ocr.SelectTesseractDictionary.TrimEnd(':', ' ');
        AutomationProperties.SetName(comboBoxDictionary, dictionaryLabel);

        var panelPickDictionary = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                EditorChrome.MakeSectionHeader(dictionaryLabel),
                comboBoxDictionary,
            }
        };
        panelPickDictionary.Bind(Panel.IsVisibleProperty, new Binding(nameof(vm.IsProgressVisible)) { Converter = InverseBooleanConverter.Instance });

        var progressBar = EditorChrome.MakeDownloadProgressBar(nameof(DownloadFfmpegViewModel.Progress));
        progressBar.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(vm.IsProgressVisible)));

        var statusText = EditorChrome.MakeDownloadStatus(nameof(DownloadFfmpegViewModel.StatusText));
        statusText.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.IsProgressVisible)));

        var errorText = EditorChrome.MakeDownloadError(nameof(vm.Error));

        var buttonDownload = EditorChrome.AsPrimary(UiUtil.MakeButton(Se.Language.General.Download, vm.DownloadCommand));
        buttonDownload.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsProgressVisible)) { Converter = InverseBooleanConverter.Instance });

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonCancel, buttonDownload);

        Content = EditorChrome.MakeRoot(EditorChrome.MakeDownloadBody(titleText, panelPickDictionary, progressBar, statusText, errorText), buttonBar, stretch: false);

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
        });
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
