using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

public partial class EditCustomTextFormatViewModel : ObservableObject
{
    [ObservableProperty] private CustomFormatItem? _selectedCustomFormat;
    [ObservableProperty] private string _previewText;
    [ObservableProperty] private string _title;

    public List<string> NewLineList { get; } = new() { "{newline}", "{tab}", "{cr}", "{lf}", "||" };
    public List<string> TimeCodeList { get; } = new() { "hh:mm:ss,zzz", "ff" };
    public List<string> HeaderFooterTags { get; } = new() { "{title}", "{#lines}", "{tab}", "{media-file-name}", "{media-file-name-full}", "{media-file-name-with-ext}" };
    public List<string> ParagraphTags { get; } = new()
    {
        "{text-length-br1}",
        "{start}",
        "{end}",
        "{number}",
        "{duration}",
        "{gap}",
        "{text}",
        "{original-text}",
        "{actor}",
        "{actor-colon-space}",
        "{actor-upper-brackets-space}",
        "{tab}",
        "{text-line-1}",
        "{text-line-2}",
        "{cps-period}",
        "{cps-comma}",
        "{text-length}",
        "{text-length-br0}",
        "{text-length-br1}",
        "{text-length-br2}",
        "{bookmark}",
    };

    private List<SubtitleLineViewModel> _subtitles;
    private string _subtitleTitle;
    private string? _videoFileName;
    private readonly System.Timers.Timer _previewTimer;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public TextBox TextBoxHeader { get; set; }
    public TextBox TextBoxFooter { get; set; }
    public TextBox TextBoxParagraph { get; set; }

    public EditCustomTextFormatViewModel()
    {
        Title = string.Empty;
        PreviewText = string.Empty;
        TextBoxHeader = new TextBox();
        TextBoxParagraph = new TextBox();
        TextBoxFooter = new TextBox();
        _subtitles = new List<SubtitleLineViewModel>();
        _subtitleTitle = string.Empty;

        _previewTimer = new System.Timers.Timer(500);
        _previewTimer.Elapsed += (sender, args) =>
        {
            if (SelectedCustomFormat == null)
            {
                PreviewText = string.Empty;
                return;
            }

            PreviewText = CustomTextFormatter.GenerateCustomText(SelectedCustomFormat, _subtitles, _subtitleTitle, _videoFileName ?? string.Empty);
        };
    }


    [RelayCommand]
    private void InsertHeaderTag(string tag)
    {
        if (TextBoxHeader.Text == null)
        {
            return;
        }

        var index = TextBoxHeader.CaretIndex;
        TextBoxHeader.Text = TextBoxHeader.Text.Insert(index, tag);
    }

    [RelayCommand]
    private void InsertParagraphTag(string tag)
    {
        if (TextBoxParagraph.Text == null)
        {
            return;
        }

        var index = TextBoxParagraph.CaretIndex;
        TextBoxParagraph.Text = TextBoxParagraph.Text.Insert(index, tag);
    }

    [RelayCommand]
    private void InsertFooterTag(string tag)
    {
        if (TextBoxFooter.Text == null)
        {
            return;
        }

        var index = TextBoxFooter.CaretIndex;
        TextBoxFooter.Text = TextBoxFooter.Text.Insert(index, tag);
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (SelectedCustomFormat == null || Window == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedCustomFormat.Name))
        {
            var message = Se.Language.File.Export.PleaseEnterNameForTheCustomFormat;
            await MessageBox.Show(Window, Se.Language.General.Error, message);
            return;
        }

        _previewTimer.Stop();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _previewTimer.Stop();
        Window?.Close();
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(CustomFormatItem selected, string title, List<SubtitleLineViewModel> subtitles, string videoFileName)
    {
        SelectedCustomFormat = selected;
        Title = title;
        _subtitles = subtitles.Take(50).ToList();
        _videoFileName = videoFileName;
        _previewTimer.Start();
    }
}