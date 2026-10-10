using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public partial class NOcrTrainFontItem : ObservableObject
{
    public string Name { get; }
    [ObservableProperty] private bool _isSelected;

    public NOcrTrainFontItem(string name, bool isSelected)
    {
        Name = name;
        IsSelected = isSelected;
    }

    // A list row or combo box value is announced by ToString() unless its template is a bare
    // text block - without this a screen reader reads the class name (#12087).
    public override string ToString() => Name;
}

public partial class NOcrTrainViewModel : ObservableObject
{
    /// <summary>
    /// The usual faces of image subtitles. Training the shipped Latin.nocr on the first eight
    /// (regular, bold and italic at 30 and 40 px, on top of the previous database) measured best
    /// on real Blu-ray/DVD subtitles; the rest are their Linux/Windows stand-ins.
    /// </summary>
    public static readonly string[] SubtitleFontPreset =
    {
        "Arial", "Helvetica", "Verdana", "Tahoma", "Trebuchet MS", "Arial Narrow", "Helvetica Neue", "Times New Roman",
        "Liberation Sans", "DejaVu Sans", "Segoe UI",
    };

    public const string PreviewText = "Subtitle Edit 123";
    private const int PreviewFontSize = 30;

    public Window? Window { get; set; }

    /// <summary>All installed fonts.</summary>
    public ObservableCollection<NOcrTrainFontItem> Fonts { get; }

    /// <summary>The fonts matching <see cref="FontSearchText"/>, shown in the list.</summary>
    public ObservableCollection<NOcrTrainFontItem> FilteredFonts { get; } = new();

    /// <summary>"Start from" choices: an empty database, then the existing databases.</summary>
    public ObservableCollection<string> BaseDatabases { get; } = new();

    [ObservableProperty] private string _fontSearchText;
    [ObservableProperty] private string _selectedFontsText;
    [ObservableProperty] private NOcrTrainFontItem? _highlightedFont;
    [ObservableProperty] private Bitmap? _previewImage;
    [ObservableProperty] private string _previewFontName;
    [ObservableProperty] private string _databaseName;
    [ObservableProperty] private string? _selectedBaseDatabase;
    [ObservableProperty] private string _fontSizes;
    [ObservableProperty] private string _charactersToTrain;
    [ObservableProperty] private string _mergedLetters;
    [ObservableProperty] private bool _trainBold;
    [ObservableProperty] private bool _trainItalic;
    [ObservableProperty] private int _numberOfSegments;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _isTraining;
    [ObservableProperty] private bool _isNotTraining;
    [ObservableProperty] private string _trainButtonText;
    [ObservableProperty] private string _trainButtonIcon;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private bool _isProgressVisible;
    [ObservableProperty] private string _learnedText;
    [ObservableProperty] private string _skippedText;

    /// <summary>Set when a database was trained and saved; the caller should refresh its database list.</summary>
    public string? TrainedDatabaseName { get; private set; }

    private volatile bool _abort;
    private volatile bool _closing;
    private readonly IFileHelper _fileHelper;

    public NOcrTrainViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        var ocr = Se.Settings.Ocr;
        var selectedFonts = (ocr.NOcrTrainFonts ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries);
        Fonts = new ObservableCollection<NOcrTrainFontItem>(
            FontHelper.GetSystemFonts().Select(name => new NOcrTrainFontItem(name, selectedFonts.Contains(name))));
        foreach (var font in Fonts)
        {
            font.PropertyChanged += (_, _) => UpdateSelectedFontsText();
        }

        Fonts.CollectionChanged += OnFontsChanged;

        BaseDatabases.Add(Se.Language.Ocr.EmptyDatabase);
        foreach (var name in NOcrDb.GetDatabases(Se.OcrFolder))
        {
            BaseDatabases.Add(name);
        }

        FontSearchText = string.Empty;
        SelectedFontsText = string.Empty;
        PreviewFontName = string.Empty;
        DatabaseName = string.Empty;
        SelectedBaseDatabase = !string.IsNullOrEmpty(ocr.NOcrTrainBaseDatabase) && BaseDatabases.Contains(ocr.NOcrTrainBaseDatabase)
            ? ocr.NOcrTrainBaseDatabase
            : BaseDatabases[0];
        FontSizes = string.IsNullOrWhiteSpace(ocr.NOcrTrainFontSizes) ? "30, 40" : ocr.NOcrTrainFontSizes;
        CharactersToTrain = NOcrTrainer.DefaultTrainingCharacters;
        MergedLetters = ocr.NOcrTrainMergedLetters ?? string.Empty;
        TrainBold = ocr.NOcrTrainBold;
        TrainItalic = ocr.NOcrTrainItalic;
        NumberOfSegments = Math.Clamp(ocr.NOcrTrainSegmentCount, 10, 500);
        StatusText = string.Empty;
        LearnedText = string.Empty;
        SkippedText = string.Empty;
        IsNotTraining = true;
        TrainButtonText = Se.Language.Ocr.StartTraining;
        TrainButtonIcon = IconNames.Play;

        ApplyFontFilter();
        UpdateSelectedFontsText();
        HighlightedFont = Fonts.FirstOrDefault(f => f.IsSelected) ?? Fonts.FirstOrDefault();
    }

    private void OnFontsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (NOcrTrainFontItem font in e.NewItems)
            {
                font.PropertyChanged += (_, _) => UpdateSelectedFontsText();
            }
        }

        ApplyFontFilter();
        UpdateSelectedFontsText();
        if (HighlightedFont == null || !Fonts.Contains(HighlightedFont))
        {
            HighlightedFont = Fonts.FirstOrDefault(f => f.IsSelected) ?? Fonts.FirstOrDefault();
        }
    }

    partial void OnFontSearchTextChanged(string value) => ApplyFontFilter();

    partial void OnHighlightedFontChanged(NOcrTrainFontItem? value) => UpdatePreview();

    partial void OnTrainBoldChanged(bool value) => UpdatePreview();

    partial void OnTrainItalicChanged(bool value) => UpdatePreview();

    partial void OnIsTrainingChanged(bool value) => IsNotTraining = !value;

    private void ApplyFontFilter()
    {
        var search = FontSearchText?.Trim() ?? string.Empty;
        FilteredFonts.Clear();
        foreach (var font in Fonts)
        {
            if (search.Length == 0 || font.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                FilteredFonts.Add(font);
            }
        }
    }

    private void UpdateSelectedFontsText()
    {
        SelectedFontsText = string.Format(Se.Language.Ocr.XFontsSelected, Fonts.Count(f => f.IsSelected));
    }

    /// <summary>
    /// Renders the sample text like the trainer renders characters (white fill, black outline)
    /// on a dark backdrop, once per style that will be trained.
    /// </summary>
    private void UpdatePreview()
    {
        var font = HighlightedFont;
        PreviewFontName = font?.Name ?? string.Empty;
        if (font == null)
        {
            PreviewImage = null;
            return;
        }

        var styles = new List<(bool Bold, bool Italic)> { (false, false) };
        if (TrainBold)
        {
            styles.Add((true, false));
        }

        if (TrainItalic)
        {
            styles.Add((false, true));
        }

        var rendered = new List<SKBitmap>();
        try
        {
            foreach (var (bold, italic) in styles)
            {
                var bitmap = NOcrTrainer.RenderCharacterImage(PreviewText, font.Name, PreviewFontSize, bold, italic);
                if (bitmap != null)
                {
                    rendered.Add(bitmap);
                }
            }

            if (rendered.Count == 0)
            {
                PreviewImage = null;
                return;
            }

            const int padding = 4;
            using var preview = new SKBitmap(rendered.Max(b => b.Width) + padding * 2, rendered.Sum(b => b.Height) + padding * 2);
            using (var canvas = new SKCanvas(preview))
            {
                canvas.Clear(SKColors.Transparent);
                var y = padding;
                foreach (var bitmap in rendered)
                {
                    canvas.DrawBitmap(bitmap, padding, y);
                    y += bitmap.Height;
                }
            }

            PreviewImage = preview.ToAvaloniaBitmap();
        }
        catch (Exception exception)
        {
            Se.LogError(exception, $"nOCR train preview failed for font '{font.Name}'");
            PreviewImage = null;
        }
        finally
        {
            foreach (var bitmap in rendered)
            {
                bitmap.Dispose();
            }
        }
    }

    /// <summary>Parses "30, 40" into distinct sizes in range; anything else is ignored.</summary>
    internal static List<int> ParseFontSizes(string? text)
    {
        var sizes = new List<int>();
        foreach (var part in (text ?? string.Empty).Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var size) &&
                size is >= 10 and <= 200 && !sizes.Contains(size))
            {
                sizes.Add(size);
            }
        }

        return sizes;
    }

    [RelayCommand]
    private void SelectSubtitleFonts()
    {
        foreach (var font in Fonts)
        {
            font.IsSelected = SubtitleFontPreset.Contains(font.Name, StringComparer.OrdinalIgnoreCase);
        }

        FontSearchText = string.Empty;
        HighlightedFont = Fonts.FirstOrDefault(f => f.IsSelected) ?? HighlightedFont;
    }

    [RelayCommand]
    private void ClearFonts()
    {
        foreach (var font in Fonts)
        {
            font.IsSelected = false;
        }
    }

    [RelayCommand]
    private void ResetCharacters()
    {
        CharactersToTrain = NOcrTrainer.DefaultTrainingCharacters;
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task StartOrAbortTraining()
    {
        if (IsTraining)
        {
            _abort = true;
            return;
        }

        var fontNames = Fonts.Where(f => f.IsSelected).Select(f => f.Name).ToList();
        if (fontNames.Count == 0)
        {
            await MessageBox.Show(Window!, Se.Language.Ocr.TrainNOcrDatabase, Se.Language.Ocr.SelectAtLeastOneFont,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var fontSizes = ParseFontSizes(FontSizes);
        if (fontSizes.Count == 0)
        {
            await MessageBox.Show(Window!, Se.Language.Ocr.TrainNOcrDatabase, Se.Language.Ocr.FontSizesHint,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var databaseName = DatabaseName.Trim();
        if (string.IsNullOrEmpty(databaseName) || databaseName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            await MessageBox.Show(Window!, Se.Language.Ocr.TrainNOcrDatabase, Se.Language.Ocr.EnterDatabaseName,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!Directory.Exists(Se.OcrFolder))
        {
            Directory.CreateDirectory(Se.OcrFolder);
        }

        var baseDatabase = SelectedBaseDatabase == Se.Language.Ocr.EmptyDatabase ? null : SelectedBaseDatabase;
        var fileName = Path.Combine(Se.OcrFolder, databaseName + ".nocr");
        if (File.Exists(fileName) && TrainedDatabaseName != databaseName && baseDatabase != databaseName)
        {
            var answer = await MessageBox.Show(Window!, Se.Language.General.FileAlreadyExists,
                string.Format(Se.Language.General.FileXAlreadyExists, fileName),
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        SaveSettings(fontNames);

        var characters = CharactersToTrain;
        var mergedLetters = MergedLetters;
        var bold = TrainBold;
        var italic = TrainItalic;
        var segments = NumberOfSegments;
        var stepsPerSize = characters.Where(c => !char.IsWhiteSpace(c)).Distinct().Count() +
                           mergedLetters.Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().Count();
        var totalSteps = Math.Max(1, fontNames.Count * fontSizes.Count * stepsPerSize);

        _abort = false;
        IsTraining = true;
        IsProgressVisible = true;
        ProgressValue = 0;
        LearnedText = string.Empty;
        SkippedText = string.Empty;
        TrainButtonText = Se.Language.Ocr.AbortTraining;
        TrainButtonIcon = IconNames.Stop;

        await Task.Run(() =>
        {
            try
            {
                var baseFileName = baseDatabase == null ? null : Path.Combine(Se.OcrFolder, baseDatabase + ".nocr");
                var db = baseFileName != null && File.Exists(baseFileName)
                    ? new NOcrDb(new NOcrDb(baseFileName), fileName)
                    : new NOcrDb(fileName) { OcrCharacters = new(), OcrCharactersExpanded = new() };

                var trainer = new NOcrTrainer();
                var learned = 0;
                var skipped = 0;
                var stepsDone = 0;
                foreach (var size in fontSizes)
                {
                    if (_abort)
                    {
                        break;
                    }

                    var settings = new NOcrTrainerSettings
                    {
                        FontNames = fontNames,
                        FontSize = size,
                        IncludeBold = bold,
                        IncludeItalic = italic,
                        NumberOfLineSegments = segments,
                        CharactersToTrain = characters,
                        MergedLetterCombinations = mergedLetters,
                    };

                    var learnedBefore = learned;
                    var skippedBefore = skipped;
                    trainer.Train(settings, db, () => _abort, p =>
                    {
                        stepsDone++;
                        learned = learnedBefore + p.CharactersLearned;
                        skipped = skippedBefore + p.CharactersSkipped;
                        var percent = Math.Min(100, stepsDone * 100.0 / totalSteps);
                        var learnedNow = learned;
                        var skippedNow = skipped;
                        Dispatcher.UIThread.Post(() =>
                        {
                            ProgressValue = percent;
                            StatusText = string.Format(Se.Language.Ocr.TrainingFontXSizeYCharacterZ, p.CurrentFontName, size, p.CurrentText);
                            LearnedText = string.Format(Se.Language.Ocr.XLearned, learnedNow);
                            SkippedText = string.Format(Se.Language.Ocr.XSkipped, skippedNow);
                        });
                    });
                }

                if (!SaveTrainedDatabase(db, databaseName))
                {
                    return;
                }

                var aborted = _abort;
                Dispatcher.UIThread.Post(() =>
                {
                    if (!aborted)
                    {
                        ProgressValue = 100;
                    }

                    StatusText = string.Format(Se.Language.Ocr.TrainingDoneXLearned, learned);
                });
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "nOCR training failed");
                Dispatcher.UIThread.Post(() => { StatusText = exception.Message; });
            }
        });

        IsTraining = false;
        TrainButtonText = Se.Language.Ocr.StartTraining;
        TrainButtonIcon = IconNames.Play;
    }

    /// <summary>
    /// Saves the trained database unless the window is closing (Done/Escape/close button), so a
    /// half-trained database never overwrites an existing one. Stop keeps what was learned so far.
    /// The name is set here (not via a posted callback) so the caller sees it as soon as the dialog returns.
    /// </summary>
    internal bool SaveTrainedDatabase(NOcrDb db, string databaseName)
    {
        if (_closing)
        {
            return false;
        }

        db.Save();
        TrainedDatabaseName = databaseName;
        return true;
    }

    [RelayCommand]
    private async Task ImportCharactersFromFile()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle, includeVideoFiles: false);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        var seen = new HashSet<char>();
        var sb = new StringBuilder();
        foreach (var paragraph in subtitle.Paragraphs)
        {
            foreach (var ch in HtmlUtil.RemoveHtmlTags(paragraph.Text, true))
            {
                if (!char.IsWhiteSpace(ch) && seen.Add(ch))
                {
                    sb.Append(ch);
                }
            }
        }

        if (sb.Length > 0)
        {
            CharactersToTrain = sb.ToString();
        }
    }

    [RelayCommand]
    private void Done()
    {
        _closing = true;
        _abort = true;
        SaveSettings(Fonts.Where(f => f.IsSelected).Select(f => f.Name).ToList());
        Close();
    }

    private void SaveSettings(List<string> fontNames)
    {
        var ocr = Se.Settings.Ocr;
        ocr.NOcrTrainFonts = string.Join(';', fontNames);
        ocr.NOcrTrainMergedLetters = MergedLetters;
        ocr.NOcrTrainFontSizes = FontSizes;
        ocr.NOcrTrainBaseDatabase = SelectedBaseDatabase == Se.Language.Ocr.EmptyDatabase ? string.Empty : SelectedBaseDatabase ?? string.Empty;
        ocr.NOcrTrainSegmentCount = NumberOfSegments;
        ocr.NOcrTrainBold = TrainBold;
        ocr.NOcrTrainItalic = TrainItalic;
        Se.SaveSettings();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Done();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/ocr", "nocr-nikse-ocr");
        }
    }

    internal void OnClosing()
    {
        _closing = true;
        _abort = true;
    }
}
