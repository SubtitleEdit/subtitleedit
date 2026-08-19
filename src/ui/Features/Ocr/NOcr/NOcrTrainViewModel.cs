using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using System;
using System.Collections.ObjectModel;
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
}

public partial class NOcrTrainViewModel : ObservableObject
{
    public Window? Window { get; set; }

    public ObservableCollection<NOcrTrainFontItem> Fonts { get; }

    [ObservableProperty] private string _databaseName;
    [ObservableProperty] private string _charactersToTrain;
    [ObservableProperty] private string _mergedLetters;
    [ObservableProperty] private bool _trainBold;
    [ObservableProperty] private bool _trainItalic;
    [ObservableProperty] private int _fontSize;
    [ObservableProperty] private int _numberOfSegments;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _isTraining;
    [ObservableProperty] private string _trainButtonText;

    /// <summary>Set when a database was trained and saved; the caller should refresh its database list.</summary>
    public string? TrainedDatabaseName { get; private set; }

    private bool _abort;
    private readonly IFileHelper _fileHelper;

    public NOcrTrainViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        var ocr = Se.Settings.Ocr;
        var selectedFonts = (ocr.NOcrTrainFonts ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries);
        Fonts = new ObservableCollection<NOcrTrainFontItem>(
            FontHelper.GetSystemFonts().Select(name => new NOcrTrainFontItem(name, selectedFonts.Contains(name))));

        DatabaseName = string.Empty;
        CharactersToTrain = NOcrTrainer.DefaultTrainingCharacters;
        MergedLetters = ocr.NOcrTrainMergedLetters;
        TrainBold = ocr.NOcrTrainBold;
        TrainItalic = ocr.NOcrTrainItalic;
        FontSize = Math.Clamp(ocr.NOcrTrainFontSize, 20, 100);
        NumberOfSegments = Math.Clamp(ocr.NOcrTrainSegmentCount, 10, 500);
        StatusText = string.Empty;
        TrainButtonText = Se.Language.Ocr.StartTraining;
    }

    [RelayCommand]
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

        var databaseName = DatabaseName.Trim();
        if (string.IsNullOrEmpty(databaseName) || databaseName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return;
        }

        if (!Directory.Exists(Se.OcrFolder))
        {
            Directory.CreateDirectory(Se.OcrFolder);
        }

        var fileName = Path.Combine(Se.OcrFolder, databaseName + ".nocr");
        if (File.Exists(fileName) && TrainedDatabaseName != databaseName)
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

        var settings = new NOcrTrainerSettings
        {
            FontNames = fontNames,
            FontSize = FontSize,
            IncludeBold = TrainBold,
            IncludeItalic = TrainItalic,
            NumberOfLineSegments = NumberOfSegments,
            CharactersToTrain = CharactersToTrain,
            MergedLetterCombinations = MergedLetters,
        };

        _abort = false;
        IsTraining = true;
        TrainButtonText = Se.Language.Ocr.AbortTraining;

        await Task.Run(() =>
        {
            try
            {
                var db = new NOcrDb(fileName)
                {
                    OcrCharacters = new(),
                    OcrCharactersExpanded = new(),
                };
                var trainer = new NOcrTrainer();
                var learned = trainer.Train(settings, db, () => _abort, p =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        StatusText = string.Format(Se.Language.Ocr.TrainingXLearnedYSkipped,
                            p.CurrentFontName, p.CharactersLearned, p.CharactersSkipped);
                    });
                });

                db.Save();

                Dispatcher.UIThread.Post(() =>
                {
                    TrainedDatabaseName = databaseName;
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

        var seen = new System.Collections.Generic.HashSet<char>();
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
        _abort = true;
        SaveSettings(Fonts.Where(f => f.IsSelected).Select(f => f.Name).ToList());
        Close();
    }

    private void SaveSettings(System.Collections.Generic.List<string> fontNames)
    {
        var ocr = Se.Settings.Ocr;
        ocr.NOcrTrainFonts = string.Join(';', fontNames);
        ocr.NOcrTrainMergedLetters = MergedLetters;
        ocr.NOcrTrainFontSize = FontSize;
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
    }

    internal void OnClosing()
    {
        _abort = true;
    }
}
