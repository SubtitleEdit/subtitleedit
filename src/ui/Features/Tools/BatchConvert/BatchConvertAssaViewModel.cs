using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public partial class BatchConvertAssaViewModel : ObservableObject
{
    [ObservableProperty] private bool _useSourceStylesIfPossible;
    [ObservableProperty] private string _text;
    [ObservableProperty] private StyleDisplay? _currentStyle;
    [ObservableProperty] private ObservableCollection<string> _fonts;
    [ObservableProperty] private ObservableCollection<BorderStyleItem> _borderTypes;
    [ObservableProperty] private BorderStyleItem _selectedBorderType;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public Border TextBoxContainer { get; set; }
    public ITextBoxWrapper SourceViewTextBox { get; set; }

    private readonly IWindowService _windowService;

    private Subtitle _subtitle;

    public BatchConvertAssaViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Text = string.Empty;
        SourceViewTextBox = new SyntaxTextEditorWrapper(new SyntaxTextEditor());
        TextBoxContainer = new Border();
        Fonts = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        BorderTypes = new ObservableCollection<BorderStyleItem>(BorderStyleItem.List());
        SelectedBorderType = BorderTypes[0];
        _subtitle = new Subtitle();
        _subtitle.Paragraphs.Add(new Paragraph("Sample subtitle", 0, 2000));

        UseSourceStylesIfPossible = Se.Settings.Tools.BatchConvert.AssaUseSourceStylesIfPossible;
        _subtitle.Header = Se.Settings.Tools.BatchConvert.AssaHeader;
        _subtitle.Footer = Se.Settings.Tools.BatchConvert.AssaFooter;

        // Generate the source view after the saved header/footer have been applied - otherwise
        // the window always shows the default styles instead of the saved ones (#12839).
        Text = _subtitle.ToText(new AdvancedSubStationAlpha());
    }

    [RelayCommand]
    private async Task EditStyles()
    {
        if (Window == null)
        {
            return;
        }

        UpdateSubtitleFromText();

        var result = await _windowService.ShowDialogAsync<AssaStylesWindow, AssaStylesViewModel>(Window, vm =>
        {
            vm.Initialize(_subtitle, new AdvancedSubStationAlpha(), string.Empty, string.Empty, null);
        });

        if (result.OkPressed)
        {
            _subtitle.Header = result.Header;
            Text = _subtitle.ToText(new AdvancedSubStationAlpha());
        }
    }

    [RelayCommand]
    private async Task EditAttachment()
    {
        if (Window == null)
        {
            return;
        }

        UpdateSubtitleFromText();

        var result = await _windowService.ShowDialogAsync<AssaAttachmentsWindow, AssaAttachmentsViewModel>(Window, vm =>
        {
            vm.Initialize(_subtitle, new AdvancedSubStationAlpha(), string.Empty);
        });

        if (result.OkPressed)
        {
            _subtitle.Footer = result.Footer;
            Text = _subtitle.ToText(new AdvancedSubStationAlpha());
        }
    }

    [RelayCommand]
    private async Task EditProperties()
    {
        if (Window == null)
        {
            return;
        }

        UpdateSubtitleFromText();

        var result = await _windowService.ShowDialogAsync<AssaPropertiesWindow, AssaPropertiesViewModel>(Window, vm =>
        {
            vm.Initialize(_subtitle, new AdvancedSubStationAlpha(), string.Empty, string.Empty);
        });

        if (result.OkPressed)
        {
            _subtitle.Header = result.Header;
            Text = _subtitle.ToText(new AdvancedSubStationAlpha());
        }
    }

    [RelayCommand]
    private void Ok()
    {
        // Pick up any hand edits made in the source view before saving.
        UpdateSubtitleFromText();

        OkPressed = true;
        Se.Settings.Tools.BatchConvert.AssaUseSourceStylesIfPossible = UseSourceStylesIfPossible;
        Se.Settings.Tools.BatchConvert.AssaHeader = _subtitle.Header ?? string.Empty;
        Se.Settings.Tools.BatchConvert.AssaFooter = _subtitle.Footer ?? string.Empty;
        Se.SaveSettings();
        Window?.Close();
    }

    /// <summary>
    /// Read the header/footer back from the source view, so edits made directly in the
    /// text editor are kept and not overwritten by the last dialog result.
    /// </summary>
    private void UpdateSubtitleFromText()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        var sub = new Subtitle();
        new AdvancedSubStationAlpha().LoadSubtitle(sub, Text.SplitToLines(), string.Empty);
        if (string.IsNullOrEmpty(sub.Header))
        {
            return; // not parsable as ASSA - keep what we have
        }

        _subtitle.Header = sub.Header;
        _subtitle.Footer = sub.Footer;
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private SyntaxTextEditorWrapper CreateAdvancedTextBoxWrapper(string text)
    {
        var editor = new SyntaxTextEditor
        {
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ShowLineNumbers = true,
            SourceHighlighter = new AssaSourceSyntaxHighlighting(),
            Text = text,
        };

        // Two way by hand: the ASSA property editors rebuild Text, and typing in the editor has to
        // come back the other way. The header is small, so mirroring it per keystroke is fine here.
        var updating = false;

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(Text) || updating)
            {
                return;
            }

            updating = true;
            try
            {
                editor.Text = Text ?? string.Empty;
            }
            finally
            {
                updating = false;
            }
        };

        editor.TextChanged += (_, _) =>
        {
            if (updating)
            {
                return;
            }

            updating = true;
            try
            {
                Text = editor.Text;
            }
            finally
            {
                updating = false;
            }
        };

        var textBoxBorder = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            Child = editor,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        return new SyntaxTextEditorWrapper(editor, textBoxBorder);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Loaded()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            await Task.Delay(50); // Slight delay to ensure control is ready

            SourceViewTextBox = CreateAdvancedTextBoxWrapper(Text);

            TextBoxContainer.Child = SourceViewTextBox.ContentControl;

            await Task.Delay(50); // Slight delay to ensure control is ready
            SourceViewTextBox.Focus();
            SourceViewTextBox.CaretIndex = 0;
        }, DispatcherPriority.Input);
    }
}