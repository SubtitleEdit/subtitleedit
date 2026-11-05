using Nikse.SubtitleEdit.Logic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;

public class TranscriptionProgressWindow : Window
{
    public TranscriptionProgressViewModel ViewModel => (TranscriptionProgressViewModel)DataContext!;

    public TranscriptionProgressWindow()
    {
        InitializeComponent();
    }

    public TranscriptionProgressWindow(TranscriptionProgressViewModel viewModel)
    {
        DataContext = viewModel;
        viewModel.Window = this;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Title = Se.Language.Video.AudioToText.Transcribing;
        Width = 500;
        Height = 450;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var connectionInfoText = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap
        };
        connectionInfoText.Bind(TextBlock.TextProperty, new Binding(nameof(TranscriptionProgressViewModel.ServerUrl)) { StringFormat = Se.Language.General.TranscriptionProgressServerFormat });

        var modelInfoText = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
        };
        modelInfoText.Bind(TextBlock.TextProperty, new Binding(nameof(TranscriptionProgressViewModel.ModelName)) { StringFormat = Se.Language.General.TranscriptionProgressModelFormat });

        var statusText = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(TranscriptionProgressViewModel.StatusText)));

        var info = new StackPanel { Spacing = 4, Children = { statusText, connectionInfoText, modelInfoText } };
        var infoSection = EditorChrome.MakeEditorSection(Se.Language.Video.AudioToText.Transcribing, info, first: true);

        var streamedTextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontFamily = new FontFamily("Monospace"),
            FontSize = EditorChrome.FieldFontSize,
            Margin = new Thickness(8, 6),
        };
        streamedTextBlock.Bind(TextBlock.TextProperty, new Binding(nameof(TranscriptionProgressViewModel.StreamedText)));
        var streamedSection = EditorChrome.MakeFillSection(Se.Language.General.Text,
            EditorChrome.MakeListFrame(new ScrollViewer { Content = streamedTextBlock }));

        var segmentsList = new ListBox();
        segmentsList.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(TranscriptionProgressViewModel.ReceivedSegments)));
        // Same text as the section header, so the list is announced as "Received segments (N)" (#12087).
        segmentsList.Bind(Avalonia.Automation.AutomationProperties.NameProperty,
            new Binding(nameof(TranscriptionProgressViewModel.SegmentCount)) { StringFormat = Se.Language.General.TranscriptionProgressReceivedSegmentsFormat });

        // A section like the other windows' lists instead of an expander (it was always expanded)
        var segmentsHeader = EditorChrome.MakeBoundSectionHeader(nameof(TranscriptionProgressViewModel.SegmentCount));
        segmentsHeader.Bind(TextBlock.TextProperty, new Binding(nameof(TranscriptionProgressViewModel.SegmentCount))
        {
            Converter = new Avalonia.Data.Converters.FuncValueConverter<int, string>(count =>
                string.Format(Se.Language.General.TranscriptionProgressReceivedSegmentsFormat, count).ToUpperInvariant()),
        });
        var segmentsSection = EditorChrome.MakeFillSection(segmentsHeader, EditorChrome.MakeListFrame(segmentsList, clearListBackground: true));

        var cancelButton = UiUtil.MakeButtonCancel(null);
        cancelButton.Bind(Button.CommandProperty, new Binding(nameof(TranscriptionProgressViewModel.CancelCommand)));
        cancelButton.Bind(Button.IsVisibleProperty, new Binding("!" + nameof(TranscriptionProgressViewModel.IsCompleted)));

        var closeButton = UiUtil.MakeButton(Se.Language.General.Close, null);
        closeButton.Classes.Add("primary");
        closeButton.IsDefault = true;
        closeButton.Bind(Button.CommandProperty, new Binding(nameof(TranscriptionProgressViewModel.CloseCommand)));
        closeButton.Bind(Button.IsVisibleProperty, new Binding(nameof(TranscriptionProgressViewModel.IsCompleted)));

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
        };
        sections.Add(infoSection, 0);
        sections.Add(streamedSection, 1);
        sections.Add(segmentsSection, 2);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, cancelButton, closeButton));

        // The list, not Cancel: a stray Space must not stop a long transcription.
        UiUtil.FocusOnFirstActivation(this, segmentsList);
        KeyDown += (_, e) => (DataContext as TranscriptionProgressViewModel)?.OnKeyDown(e);
    }
}
