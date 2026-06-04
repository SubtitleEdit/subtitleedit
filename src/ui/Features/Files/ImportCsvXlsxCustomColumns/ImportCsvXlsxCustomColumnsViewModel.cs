using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.ImportCsvXlsxCustomColumns;

public partial class ImportCsvXlsxCustomColumnsViewModel : ObservableObject
{
    [ObservableProperty] private string _filePath;
    [ObservableProperty] private string _separatorDisplay;
    [ObservableProperty] private ObservableCollection<CsvColumnDefinition> _columns;
    [ObservableProperty] private ObservableCollection<CsvRowItem> _rows;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _previewSubtitles;
    [ObservableProperty] private string _previewCount;
    [ObservableProperty] private bool _isOkEnabled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> ResultSubtitles { get; private set; } = new();

    public event EventHandler? ColumnsRebuilt;

    private readonly IFileHelper _fileHelper;

    private static readonly HashSet<string> StartHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "start", "start time", "in", "begin", "starttime", "start_time", "startmillis", "start_millis",
        "startms", "start_ms", "startmilliseconds", "start_milliseconds", "from", "fromtime",
        "tc-in", "tc in", "show", "start tc", "start-tc", "tc start", "tc-start",
    };

    private static readonly HashSet<string> EndHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "end", "end time", "out", "stop", "endtime", "end_time", "endmillis", "end_millis",
        "endms", "end_ms", "endmilliseconds", "end_milliseconds", "to", "totime",
        "tc-out", "tc out", "hide", "end tc", "end-tc", "tc end", "tc-end",
    };

    private static readonly HashSet<string> DurationHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "duration", "durationms", "dur",
    };

    private static readonly HashSet<string> TextHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "text", "content", "value", "caption", "sentence", "dialog", "dialogue",
    };

    private static readonly HashSet<string> CharacterHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "speaker", "voice", "character", "role", "actor", "rolle", "character name", "sprecher",
    };

    public ImportCsvXlsxCustomColumnsViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        FilePath = string.Empty;
        SeparatorDisplay = string.Empty;
        Columns = new ObservableCollection<CsvColumnDefinition>();
        Rows = new ObservableCollection<CsvRowItem>();
        PreviewSubtitles = new ObservableCollection<SubtitleLineViewModel>();
        PreviewCount = string.Empty;
    }

    [RelayCommand]
    private async Task PickFile()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(
            Window,
            Se.Language.General.OpenFile,
            Se.Language.File.Import.CsvXlsxFilterTitle,
            "*.csv;*.xlsx;*.ods;*.tsv;*.txt");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        LoadFile(fileName);
    }

    [RelayCommand]
    private void Ok()
    {
        ResultSubtitles = PreviewSubtitles.ToList();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    private void LoadFile(string fileName)
    {
        FilePath = fileName;

        List<string>? lines;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext == ".xlsx")
        {
            lines = new UnknownFormatImporterXlsx().ReadLinesFromFile(fileName);
        }
        else if (ext == ".ods")
        {
            lines = new UnknownFormatImporterOds().ReadLinesFromFile(fileName);
        }
        else
        {
            try
            {
                lines = File.ReadAllLines(fileName, LanguageAutoDetect.GetEncodingFromFile(fileName)).ToList();
            }
            catch
            {
                lines = null;
            }
        }

        if (lines == null || lines.Count == 0)
        {
            ClearFileState();
            return;
        }

        // XLSX/ODS readers emit tab-separated rows; CSV/TSV/TXT need separator detection.
        var separator = ext is ".xlsx" or ".ods" ? '\t' : DetectSeparator(lines);
        SeparatorDisplay = string.Format(Se.Language.File.Import.DetectedSeparatorX, FormatSeparator(separator));

        var parsed = CsvUtil.CsvSplitLines(lines, separator);
        if (parsed.Count == 0)
        {
            ClearFileState();
            return;
        }

        var columnCount = parsed.Max(r => r.Count);
        if (columnCount == 0)
        {
            ClearFileState();
            return;
        }

        UnsubscribeColumns();

        var firstRow = parsed[0];
        var headerLooksLikeHeader = LooksLikeHeader(firstRow);
        var headerNames = new List<string>();
        for (var i = 0; i < columnCount; i++)
        {
            if (headerLooksLikeHeader && i < firstRow.Count && !string.IsNullOrWhiteSpace(firstRow[i]))
            {
                headerNames.Add(firstRow[i].Trim());
            }
            else
            {
                headerNames.Add($"{Se.Language.General.Column} {i + 1}");
            }
        }

        var newColumns = new List<CsvColumnDefinition>();
        for (var i = 0; i < columnCount; i++)
        {
            var role = headerLooksLikeHeader ? GuessRoleFromHeader(headerNames[i]) : CsvColumnRole.None;
            newColumns.Add(new CsvColumnDefinition(i, headerNames[i], role));
        }

        EnsureSingleAssignment(newColumns);

        // Subscribe after the initial role assignment so auto-detection doesn't trigger preview rebuilds.
        foreach (var c in newColumns)
        {
            c.RoleChanged += OnColumnRoleChanged;
        }

        Columns.Clear();
        foreach (var c in newColumns)
        {
            Columns.Add(c);
        }

        Rows.Clear();
        var dataRows = headerLooksLikeHeader ? parsed.Skip(1) : parsed;
        foreach (var row in dataRows)
        {
            var cells = new List<string>(columnCount);
            for (var i = 0; i < columnCount; i++)
            {
                cells.Add(i < row.Count ? row[i] : string.Empty);
            }
            Rows.Add(new CsvRowItem(cells));
        }

        ColumnsRebuilt?.Invoke(this, EventArgs.Empty);
        RebuildPreview();
    }

    private void OnColumnRoleChanged(object? sender, EventArgs e)
    {
        if (sender is CsvColumnDefinition changed && changed.Role != CsvColumnRole.None)
        {
            // Each role (other than None) is unique — clear it from any other column.
            foreach (var c in Columns)
            {
                if (!ReferenceEquals(c, changed) && c.Role == changed.Role)
                {
                    c.RoleChanged -= OnColumnRoleChanged;
                    c.Role = CsvColumnRole.None;
                    c.RoleChanged += OnColumnRoleChanged;
                }
            }
        }

        RebuildPreview();
    }

    private void RebuildPreview()
    {
        PreviewSubtitles.Clear();

        var startCol = Columns.FirstOrDefault(c => c.Role == CsvColumnRole.Start);
        var endCol = Columns.FirstOrDefault(c => c.Role == CsvColumnRole.End);
        var durationCol = Columns.FirstOrDefault(c => c.Role == CsvColumnRole.Duration);
        var textCol = Columns.FirstOrDefault(c => c.Role == CsvColumnRole.Text);
        var characterCol = Columns.FirstOrDefault(c => c.Role == CsvColumnRole.Character);

        IsOkEnabled = textCol != null && Rows.Count > 0;
        if (textCol == null)
        {
            PreviewCount = string.Empty;
            return;
        }

        var startSamples = startCol == null ? null : Rows.Select(r => Cell(r, startCol.Index)).ToList();
        var endSamples = endCol == null ? null : Rows.Select(r => Cell(r, endCol.Index)).ToList();
        var durationSamples = durationCol == null ? null : Rows.Select(r => Cell(r, durationCol.Index)).ToList();

        var startIsFrames = startSamples != null && DetectIsFrames(startSamples);
        var endIsFrames = endSamples != null && DetectIsFrames(endSamples);
        var durationIsFrames = durationSamples != null && DetectIsFrames(durationSamples);

        var startIsMs = startCol != null && IsMillisecondsHeader(startCol.HeaderName);
        var endIsMs = endCol != null && IsMillisecondsHeader(endCol.HeaderName);
        var durationIsMs = durationCol != null && IsMillisecondsHeader(durationCol.HeaderName);

        var number = 1;
        foreach (var row in Rows)
        {
            var text = Cell(row, textCol.Index);
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            var startMs = ParseTimeCell(startCol == null ? null : Cell(row, startCol.Index), startIsFrames, startIsMs);
            var endMs = ParseTimeCell(endCol == null ? null : Cell(row, endCol.Index), endIsFrames, endIsMs);
            var durationMs = ParseTimeCell(durationCol == null ? null : Cell(row, durationCol.Index), durationIsFrames, durationIsMs);

            if (durationMs > 0 && endMs <= 0)
            {
                endMs = startMs + durationMs;
            }

            var actor = characterCol == null ? string.Empty : Cell(row, characterCol.Index).Trim();

            var line = new SubtitleLineViewModel
            {
                Number = number++,
                Text = text.Trim(),
                Actor = actor,
                StartTime = TimeSpan.FromMilliseconds(startMs),
                EndTime = TimeSpan.FromMilliseconds(endMs),
                Duration = TimeSpan.FromMilliseconds(Math.Max(0, endMs - startMs)),
            };
            PreviewSubtitles.Add(line);
        }

        PreviewCount = string.Format(Se.Language.File.Import.NumberOfSubtitlesX, PreviewSubtitles.Count);
    }

    private static double ParseTimeCell(string? cell, bool isFrames, bool isMilliseconds)
    {
        if (string.IsNullOrWhiteSpace(cell))
        {
            return 0;
        }

        var trimmed = cell.Trim();
        if (isFrames)
        {
            return TimeCode.ParseHHMMSSFFToMilliseconds(trimmed);
        }

        if (isMilliseconds && double.TryParse(trimmed, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var ms))
        {
            return ms;
        }

        return TimeCode.ParseToMilliseconds(trimmed);
    }

    private static string Cell(CsvRowItem row, int index)
    {
        if (index < 0 || index >= row.Cells.Count)
        {
            return string.Empty;
        }
        return row.Cells[index] ?? string.Empty;
    }

    private static bool DetectIsFrames(List<string> values)
    {
        if (values.Count == 0)
        {
            return false;
        }

        foreach (var s in values)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            var parts = s.Split(TimeCode.TimeSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4 || parts[3].Trim().Length != 2)
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsMillisecondsHeader(string header)
    {
        var h = header.Replace(" ", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
        return h.EndsWith("ms") || h.EndsWith("millis") || h.EndsWith("milliseconds");
    }

    private static char DetectSeparator(List<string> lines)
    {
        const char defaultSeparator = ',';
        char[] candidates = { defaultSeparator, ';', '\t', '|' };

        var nonEmpty = lines.Where(l => !string.IsNullOrWhiteSpace(l)).Take(20).ToList();
        if (nonEmpty.Count < 2)
        {
            return defaultSeparator;
        }

        char best = defaultSeparator;
        var bestScore = 0;
        foreach (var sep in candidates)
        {
            var firstCount = CountTopLevel(nonEmpty[0], sep);
            if (firstCount == 0)
            {
                continue;
            }

            var consistent = nonEmpty.Skip(1).Take(5).Count(l => CountTopLevel(l, sep) == firstCount);
            var score = consistent * 10 + firstCount;
            if (score > bestScore)
            {
                bestScore = score;
                best = sep;
            }
        }
        return best;
    }

    private static int CountTopLevel(string line, char separator)
    {
        var count = 0;
        var inQuotes = false;
        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (ch == separator && !inQuotes)
            {
                count++;
            }
        }
        return count;
    }

    private static string FormatSeparator(char separator) => separator switch
    {
        '\t' => Se.Language.File.Import.SeparatorTab,
        ' ' => Se.Language.File.Import.SeparatorSpace,
        _ => separator.ToString(),
    };

    private static bool LooksLikeHeader(List<string> firstRow)
    {
        if (firstRow.Count == 0)
        {
            return false;
        }

        // Header rows typically have non-empty cells that aren't time codes or pure numbers.
        var lettered = 0;
        foreach (var cell in firstRow)
        {
            if (string.IsNullOrWhiteSpace(cell))
            {
                continue;
            }

            var t = cell.Trim();
            if (double.TryParse(t, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
            {
                continue;
            }

            // Time code patterns (HH:MM:SS:FF, HH:MM:SS.mmm, etc.) — not a header.
            if (t.Contains(':') && t.Length >= 5 && char.IsDigit(t[0]))
            {
                continue;
            }

            if (t.Any(char.IsLetter))
            {
                lettered++;
            }
        }
        return lettered >= 2;
    }

    private static CsvColumnRole GuessRoleFromHeader(string headerName)
    {
        var name = headerName.Trim();
        if (StartHeaderNames.Contains(name))
        {
            return CsvColumnRole.Start;
        }
        if (EndHeaderNames.Contains(name))
        {
            return CsvColumnRole.End;
        }
        if (DurationHeaderNames.Contains(name))
        {
            return CsvColumnRole.Duration;
        }
        if (TextHeaderNames.Contains(name))
        {
            return CsvColumnRole.Text;
        }
        if (CharacterHeaderNames.Contains(name))
        {
            return CsvColumnRole.Character;
        }

        // Loose ms-suffixed matches (e.g., "startMs", "endMillis").
        var compact = name.Replace(" ", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
        if (compact.StartsWith("start") || compact.StartsWith("from"))
        {
            return CsvColumnRole.Start;
        }
        if (compact.StartsWith("end") || compact.StartsWith("to") || compact.StartsWith("stop"))
        {
            return CsvColumnRole.End;
        }
        if (compact.StartsWith("duration") || compact.StartsWith("dur"))
        {
            return CsvColumnRole.Duration;
        }
        return CsvColumnRole.None;
    }

    private static void EnsureSingleAssignment(List<CsvColumnDefinition> columns)
    {
        var seen = new HashSet<CsvColumnRole>();
        foreach (var c in columns)
        {
            if (c.Role == CsvColumnRole.None)
            {
                continue;
            }

            if (!seen.Add(c.Role))
            {
                c.Role = CsvColumnRole.None;
            }
        }
    }

    private void UnsubscribeColumns()
    {
        foreach (var c in Columns)
        {
            c.RoleChanged -= OnColumnRoleChanged;
        }
    }

    private void ClearFileState()
    {
        UnsubscribeColumns();
        Columns.Clear();
        Rows.Clear();
        PreviewSubtitles.Clear();
        PreviewCount = string.Empty;
        SeparatorDisplay = string.Empty;
        IsOkEnabled = false;
        ColumnsRebuilt?.Invoke(this, EventArgs.Empty);
    }
}
