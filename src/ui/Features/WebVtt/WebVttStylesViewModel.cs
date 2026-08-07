using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.WebVtt;

/// <summary>
/// Editor for the <c>STYLE</c> blocks of a WebVTT file - the cue classes referenced from the
/// text as <c>&lt;c.name&gt;</c>. The whole <c>STYLE</c> section of the header is rewritten from
/// <see cref="Styles"/> on OK/Apply, so the list order is the file order, not a view sort.
/// </summary>
public partial class WebVttStylesViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<WebVttStyleDisplay> _styles;
    [ObservableProperty] private ObservableCollection<string> _fonts;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStyleSelected))]
    private WebVttStyleDisplay? _selectedStyle;

    [ObservableProperty] private Bitmap? _imagePreview;
    [ObservableProperty] private string _cssBefore;
    [ObservableProperty] private string _cssAfter;
    [ObservableProperty] private string _duplicateStyleNames;
    [ObservableProperty] private bool _hasDuplicateStyleNames;
    [ObservableProperty] private bool _isApplyVisible;
    [ObservableProperty] private bool _isNameInvalid;

    public bool IsStyleSelected => SelectedStyle != null;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>The rewritten WebVTT header, valid after OK or Apply.</summary>
    public string Header { get; private set; }

    public TableView StyleGrid { get; set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private readonly System.Timers.Timer _timerUpdatePreview;
    private List<WebVttStyle> _originalStyles;
    private Subtitle _subtitle;
    private IApplyWebVttStyles? _applyWebVttStyles;
    private volatile bool _isClosing;

    public WebVttStylesViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;

        _title = string.Empty;
        _styles = new ObservableCollection<WebVttStyleDisplay>();
        _fonts = new ObservableCollection<string>();
        _cssBefore = string.Empty;
        _cssAfter = string.Empty;
        _duplicateStyleNames = string.Empty;
        _originalStyles = new List<WebVttStyle>();
        _subtitle = new Subtitle();

        Header = string.Empty;
        StyleGrid = new TableView();

        _timerUpdatePreview = new System.Timers.Timer(300);
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
    }

    public void Initialize(Subtitle subtitle, string fileName, string? selectedStyleName, IApplyWebVttStyles? applyWebVttStyles)
    {
        Title = string.Format(Se.Language.Assa.StylesTitleX, fileName);
        _subtitle = subtitle;
        _applyWebVttStyles = applyWebVttStyles;
        IsApplyVisible = applyWebVttStyles != null;
        Header = subtitle.Header ?? string.Empty;
        _originalStyles = WebVttHelper.GetStyles(Header);

        Styles.Clear();
        foreach (var style in WebVttHelper.GetStyles(Header))
        {
            var display = new WebVttStyleDisplay(style);
            display.PropertyChanged += StyleChanged;
            Styles.Add(display);

            if (!string.IsNullOrEmpty(display.FontName) && !Fonts.Contains(display.FontName))
            {
                Fonts.Insert(0, display.FontName);
            }
        }

        UpdateUsages();
        CheckDuplicateStyleNames();

        if (Styles.Count > 0)
        {
            SelectedStyle = Styles.FirstOrDefault(p =>
                                p.Name.Equals((selectedStyleName ?? string.Empty).TrimStart('.'), StringComparison.OrdinalIgnoreCase))
                            ?? Styles[0];
        }

        Task.Run(LoadFonts);

        _timerUpdatePreview.Start();
    }

    /// <summary>
    /// The font combo box binds to the selected style's font name; a font missing from the item
    /// list would make Avalonia clear the selection and null out the style's font (see #13101).
    /// </summary>
    partial void OnSelectedStyleChanging(WebVttStyleDisplay? value)
    {
        var fontName = value?.FontName;
        if (!string.IsNullOrEmpty(fontName) && !Fonts.Contains(fontName))
        {
            Fonts.Insert(0, fontName);
        }
    }

    partial void OnSelectedStyleChanged(WebVttStyleDisplay? value)
    {
        UpdateCssLabels();
        ValidateName();
    }

    private void LoadFonts()
    {
        var fonts = FontHelper.GetLibAssaFonts();

        Dispatcher.UIThread.Post(() =>
        {
            foreach (var font in fonts)
            {
                if (!Fonts.Contains(font))
                {
                    Fonts.Add(font);
                }
            }
        });
    }

    private void StyleChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WebVttStyleDisplay.UsageCount) ||
            e.PropertyName == nameof(WebVttStyleDisplay.IsSelected))
        {
            return;
        }

        if (e.PropertyName == nameof(WebVttStyleDisplay.Name))
        {
            CheckDuplicateStyleNames();
            ValidateName();
        }

        UpdateCssLabels();
    }

    private void UpdateCssLabels()
    {
        var style = SelectedStyle;
        if (style == null)
        {
            CssBefore = string.Empty;
            CssAfter = string.Empty;
            return;
        }

        var before = _originalStyles.FirstOrDefault(p => (p.Name ?? string.Empty).TrimStart('.') == style.Name);
        CssBefore = before == null ? string.Empty : Wrap(WebVttHelper.GetCssProperties(before));
        CssAfter = Wrap(style.Css);
    }

    private static string Wrap(string css) => css.Replace("; ", ";" + Environment.NewLine);

    /// <summary>
    /// A style name that is empty or already taken would silently overwrite another style when
    /// the header is rewritten, so it is flagged in the editor instead.
    /// </summary>
    private void ValidateName()
    {
        var style = SelectedStyle;
        IsNameInvalid = style != null &&
                        (string.IsNullOrWhiteSpace(style.Name) ||
                         Styles.Count(p => p.Name.Equals(style.Name, StringComparison.OrdinalIgnoreCase)) > 1);
    }

    private void CheckDuplicateStyleNames()
    {
        var duplicates = Styles
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        HasDuplicateStyleNames = duplicates.Count > 0;
        DuplicateStyleNames = duplicates.Count > 0
            ? string.Format(Se.Language.File.WebVtt.DuplicateStyleNamesX, string.Join(", ", duplicates))
            : string.Empty;
    }

    private void UpdateUsages()
    {
        foreach (var style in Styles)
        {
            style.UsageCount = CountUsages(_subtitle, style.Name);
        }
    }

    /// <summary>
    /// Counts the lines using a cue class. A class is referenced as <c>&lt;c.name&gt;</c> and
    /// several classes can be combined (<c>&lt;c.name.other&gt;</c>), so the name is matched
    /// followed by either the next class separator or the closing bracket.
    /// </summary>
    private static int CountUsages(Subtitle subtitle, string styleName)
    {
        if (string.IsNullOrEmpty(styleName))
        {
            return 0;
        }

        var count = 0;
        foreach (var p in subtitle.Paragraphs)
        {
            if (string.IsNullOrEmpty(p.Text))
            {
                continue;
            }

            if (p.Text.Contains("." + styleName + ".", StringComparison.Ordinal) ||
                p.Text.Contains("." + styleName + ">", StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }

    [RelayCommand]
    private void New()
    {
        var style = new WebVttStyleDisplay { Name = MakeUniqueName("new") };
        style.PropertyChanged += StyleChanged;
        Styles.Add(style);
        SelectedStyle = style;
        CheckDuplicateStyleNames();
        TableViewExtras.EnsureRowFullyVisible(StyleGrid, style);
    }

    [RelayCommand]
    private void Duplicate()
    {
        if (SelectedStyle == null)
        {
            return;
        }

        var style = new WebVttStyleDisplay(SelectedStyle) { Name = MakeUniqueName(SelectedStyle.Name) };
        style.PropertyChanged += StyleChanged;
        Styles.Add(style);
        SelectedStyle = style;
        CheckDuplicateStyleNames();
        TableViewExtras.EnsureRowFullyVisible(StyleGrid, style);
    }

    private string MakeUniqueName(string baseName)
    {
        // The name ends up in a cue class selector, where a space would split it into two classes.
        var name = string.IsNullOrWhiteSpace(baseName) ? "new" : baseName.Trim().Replace(" ", "-");
        var candidate = name;
        var count = 2;
        while (Styles.Any(p => p.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase)))
        {
            candidate = name + "-" + count;
            count++;
        }

        return candidate;
    }

    [RelayCommand]
    private void Remove()
    {
        if (SelectedStyle == null)
        {
            return;
        }

        var idx = Styles.IndexOf(SelectedStyle);
        SelectedStyle.PropertyChanged -= StyleChanged;
        Styles.RemoveAt(idx);
        SelectedStyle = Styles.Count == 0 ? null : Styles[Math.Min(idx, Styles.Count - 1)];
        CheckDuplicateStyleNames();
    }

    [RelayCommand]
    private void RemoveAll()
    {
        foreach (var style in Styles)
        {
            style.PropertyChanged -= StyleChanged;
        }

        Styles.Clear();
        SelectedStyle = null;
        CheckDuplicateStyleNames();
    }

    [RelayCommand]
    private async Task Import()
    {
        if (Window == null)
        {
            return;
        }

        var format = new WebVTT();
        var fileName = await _fileHelper.PickOpenFile(
            Window,
            Se.Language.File.WebVtt.OpenStyleFileTitle,
            format.Name,
            "*" + format.Extension,
            format.Name,
            "*.webvtt");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        List<WebVttStyle> importStyles;
        try
        {
            importStyles = WebVttHelper.GetStyles(FileUtil.ReadAllTextShared(fileName, Encoding.UTF8));
        }
        catch (Exception exception)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, exception.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (importStyles.Count == 0)
        {
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                Se.Language.File.WebVtt.NoStylesToImport,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var pickerResult = await _windowService.ShowDialogAsync<WebVttStylePickerWindow, WebVttStylePickerViewModel>(Window, vm =>
        {
            vm.Initialize(
                Se.Language.File.WebVtt.ImportStylesTitle,
                Se.Language.General.Import,
                importStyles.Select(p => new WebVttStyleDisplay(p) { IsSelected = true }).ToList());
        });

        if (!pickerResult.OkPressed)
        {
            return;
        }

        WebVttStyleDisplay? last = null;
        foreach (var style in pickerResult.CheckedStyles)
        {
            style.Name = MakeUniqueName(style.Name);
            style.IsSelected = false;
            style.PropertyChanged += StyleChanged;
            Styles.Add(style);
            last = style;

            if (!string.IsNullOrEmpty(style.FontName) && !Fonts.Contains(style.FontName))
            {
                Fonts.Insert(0, style.FontName);
            }
        }

        if (last != null)
        {
            SelectedStyle = last;
        }

        UpdateUsages();
        CheckDuplicateStyleNames();
    }

    [RelayCommand]
    private async Task Export()
    {
        if (Window == null || Styles.Count == 0)
        {
            return;
        }

        var pickerResult = await _windowService.ShowDialogAsync<WebVttStylePickerWindow, WebVttStylePickerViewModel>(Window, vm =>
        {
            vm.Initialize(
                Se.Language.File.WebVtt.ExportStylesTitle,
                Se.Language.General.Export,
                Styles.Select(p => new WebVttStyleDisplay(p) { IsSelected = true }).ToList());
        });

        var selected = pickerResult.CheckedStyles;
        if (!pickerResult.OkPressed || selected.Count == 0)
        {
            return;
        }

        // A cue selector is keyed by name, so two styles with the same name cannot both be
        // exported - the second would simply shadow the first when the file is read back.
        var duplicates = selected
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicates.Count > 0)
        {
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                string.Format(Se.Language.File.WebVtt.DuplicateStyleNamesX, string.Join(", ", duplicates)),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(
            Window,
            ".vtt",
            "my_styles.vtt",
            Se.Language.File.WebVtt.ExportStylesTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("WEBVTT");
        sb.AppendLine();
        sb.AppendLine("STYLE");
        foreach (var style in selected)
        {
            sb.AppendLine(ToRawStyle(style));
        }

        try
        {
            await System.IO.File.WriteAllTextAsync(fileName, sb.ToString().TrimEnd() + Environment.NewLine, Encoding.UTF8);
        }
        catch (Exception exception)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, exception.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string ToRawStyle(WebVttStyleDisplay style)
    {
        return "::cue(." + style.Name.TrimStart('.') + ") { " + style.Css + " }";
    }

    [RelayCommand]
    private void MoveUp() => TableViewExtras.MoveSelectedRows(StyleGrid, Styles, ListMoveDirection.Up);

    [RelayCommand]
    private void MoveDown() => TableViewExtras.MoveSelectedRows(StyleGrid, Styles, ListMoveDirection.Down);

    [RelayCommand]
    private void MoveToTop() => TableViewExtras.MoveSelectedRows(StyleGrid, Styles, ListMoveDirection.Top);

    [RelayCommand]
    private void MoveToBottom() => TableViewExtras.MoveSelectedRows(StyleGrid, Styles, ListMoveDirection.Bottom);

    /// <summary>
    /// Rewrites the <c>STYLE</c> section of the header from <see cref="Styles"/>, keeping every
    /// other header line (the <c>WEBVTT</c> line, <c>NOTE</c> blocks, ...) as it was.
    /// </summary>
    private void SetHeader()
    {
        var header = _subtitle.Header ?? string.Empty;
        if (!header.StartsWith("WEBVTT", StringComparison.Ordinal))
        {
            header = "WEBVTT";
        }

        var styleOn = false;
        var sb = new StringBuilder();
        foreach (var line in header.SplitToLines())
        {
            if (line.Trim().Equals("STYLE", StringComparison.OrdinalIgnoreCase))
            {
                styleOn = true;
            }
            else if (line.Trim().Length == 0 && styleOn)
            {
                styleOn = false;
            }
            else if (!styleOn)
            {
                sb.AppendLine(line);
            }
        }

        var result = new StringBuilder(sb.ToString().Trim());
        if (Styles.Count > 0)
        {
            result.AppendLine();
            result.AppendLine();
            result.AppendLine("STYLE");
            foreach (var style in Styles)
            {
                result.AppendLine(ToRawStyle(style));
            }
        }

        Header = result.ToString().TrimEnd() + Environment.NewLine;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        SetHeader();
        Close();
    }

    [RelayCommand]
    private void Apply()
    {
        OkPressed = true;
        SetHeader();
        _applyWebVttStyles?.ApplyWebVttStyles(this);
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => Window?.Close());
    }

    private void TimerUpdatePreviewElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_isClosing)
        {
            return;
        }

        _timerUpdatePreview.Stop();
        UpdatePreview();

        // OnClosingCleanup may have disposed the timer while this handler ran, and Start() on a
        // disposed timer throws from a thread-pool thread, taking the app down (#12739).
        if (!_isClosing)
        {
            _timerUpdatePreview.Start();
        }
    }

    private void UpdatePreview()
    {
        var style = SelectedStyle;
        if (style == null)
        {
            ImagePreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
            return;
        }

        var fontSize = style.FontSize > 0 ? (float)style.FontSize * 2.5f : 60f;
        var fontName = string.IsNullOrWhiteSpace(style.FontName)
            ? SKTypeface.Default.FamilyName
            : FontHelper.GetSkiaFontNameFromLibAssaFontName(style.FontName);

        // WebVTT has no outline; the background color is the cue box behind the whole cue.
        var bitmap = TextToImageGenerator.GenerateImageWithPadding(
            "This is a test",
            fontName,
            fontSize,
            style.Bold,
            style.UseColor ? style.Color.ToSKColor() : SKColors.White,
            SKColors.Transparent,
            style.UseShadow ? style.ShadowColor.ToSKColor() : SKColors.Transparent,
            style.UseBackgroundColor ? style.BackgroundColor.ToSKColor() : SKColors.Transparent,
            0,
            style.UseShadow ? (float)style.ShadowWidth : 0,
            isItalic: style.Italic,
            isUnderline: style.Underline,
            isStrikeout: style.Strikeout);

        var frame = TextToImageGenerator.ComposeOnPreviewFrame(bitmap, 2, 0, 0, 20);
        ImagePreview = frame.ToAvaloniaBitmap();
    }

    public void OnClosingCleanup()
    {
        _isClosing = true;
        _timerUpdatePreview.StopAndDispose(TimerUpdatePreviewElapsed);
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/webvtt-styles");
        }
    }

    internal void StylesMoveKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.Control)
        {
            return;
        }

        if (e.Key == Key.Up)
        {
            e.Handled = true;
            MoveUp();
        }
        else if (e.Key == Key.Down)
        {
            e.Handled = true;
            MoveDown();
        }
    }
}
