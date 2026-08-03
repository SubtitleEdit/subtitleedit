using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa.FontCollector;

public partial class FontCollectorViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<FontCollectorItem> _fontItems;
    [ObservableProperty] private FontCollectorItem? _selectedFontItem;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _isScanning;
    [ObservableProperty] private bool _isDeleteCollectedFontVisible;
    [ObservableProperty] private ObservableCollection<string> _installedFontNames;
    [ObservableProperty] private string? _selectedInstalledFontName;
    [ObservableProperty] private ObservableCollection<CollectedFont> _collectedFonts;
    [ObservableProperty] private CollectedFont? _selectedCollectedFont;
    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private Bitmap? _fontPreview;

    public Window? Window { get; set; }

    /// <summary>Set when "Embed fonts in subtitle" ran - the caller applies it to the real subtitle.</summary>
    public string? UpdatedFooter { get; private set; }

    private Subtitle _subtitle;
    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public FontCollectorViewModel(IFolderHelper folderHelper, IFileHelper fileHelper)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        FontItems = new ObservableCollection<FontCollectorItem>();
        InstalledFontNames = new ObservableCollection<string>();
        CollectedFonts = new ObservableCollection<CollectedFont>();
        StatusText = string.Empty;
        _subtitle = new Subtitle();
    }

    public void Initialize(Subtitle subtitle)
    {
        _subtitle = subtitle;
        CollectFontNames(subtitle);
        MatchEmbeddedFonts(subtitle.Footer);
        IsScanning = true;
        StatusText = Se.Language.Assa.FontCollectorScanning;

        var items = FontItems.ToList();
        _ = Task.Run(() => ScanSystemFonts(items, _cancellationTokenSource.Token));
        _ = Task.Run(LoadFontLists);
    }

    /// <summary>
    /// Marks the needed fonts that the subtitle itself carries as [Fonts] attachments,
    /// so an embedded font counts as available and can be previewed from its bytes.
    /// </summary>
    internal void MatchEmbeddedFonts(string? footer)
    {
        foreach (var (fileName, bytes) in AssaFontEmbedder.GetEmbeddedFonts(footer))
        {
            try
            {
                using var data = SKData.CreateCopy(bytes);
                for (var index = 0; index < 30; index++)
                {
                    using var typeface = SKTypeface.FromData(data, index);
                    if (typeface == null)
                    {
                        break;
                    }

                    foreach (var name in new[] { typeface.FamilyName, FontHelper.GetLibAssaFontName(typeface) })
                    {
                        var item = FontItems.FirstOrDefault(i =>
                            i.FontName.Equals(name, StringComparison.OrdinalIgnoreCase) && i.EmbeddedFontBytes == null);
                        if (item != null)
                        {
                            item.EmbeddedFontBytes = bytes;
                            item.EmbeddedFileName = fileName;
                            item.Status = Se.Language.Assa.FontCollectorEmbedded;
                            item.FileDisplay = fileName;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Font collector could not read embedded font " + fileName);
            }
        }
    }

    /// <summary>Fills the "Installed fonts" and "Collected fonts" tabs.</summary>
    private void LoadFontLists()
    {
        try
        {
            var installed = FontHelper.GetLibAssaFonts();
            var collected = FontHelper.GetFontsFolderFonts();
            Dispatcher.UIThread.Post(() =>
            {
                InstalledFontNames.Clear();
                InstalledFontNames.AddRange(installed);
                CollectedFonts.Clear();
                CollectedFonts.AddRange(collected);
            });
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Font collector font list load failed");
        }
    }

    partial void OnSelectedTabIndexChanged(int value) => UpdateFontPreview();

    partial void OnSelectedFontItemChanged(FontCollectorItem? value) => UpdateFontPreview();

    partial void OnSelectedInstalledFontNameChanged(string? value) => UpdateFontPreview();

    partial void OnSelectedCollectedFontChanged(CollectedFont? value) => UpdateFontPreview();

    /// <summary>
    /// Renders a sample-text preview of the font selected in the active tab.
    /// A found/collected font need not be installed, so it is rendered from its file.
    /// </summary>
    private void UpdateFontPreview()
    {
        string? fontName = null;
        SKTypeface? skTypeface = null;

        if (SelectedTabIndex == 0 && SelectedFontItem != null)
        {
            fontName = SelectedFontItem.FontName;
            if (SelectedFontItem.FoundFiles.Count > 0)
            {
                skTypeface = LoadTypefaceForName(SelectedFontItem.FoundFiles[0], fontName);
            }
            else if (SelectedFontItem.EmbeddedFontBytes != null)
            {
                skTypeface = LoadTypefaceForNameFromBytes(SelectedFontItem.EmbeddedFontBytes, fontName);
            }
            else
            {
                fontName = null; // not found anywhere - no preview rather than a misleading fallback font
            }
        }
        else if (SelectedTabIndex == 1 && !string.IsNullOrEmpty(SelectedInstalledFontName))
        {
            fontName = SelectedInstalledFontName;
            skTypeface = SKTypeface.FromFamilyName(FontHelper.GetSkiaFontNameFromLibAssaFontName(fontName));
        }
        else if (SelectedTabIndex == 2 && SelectedCollectedFont != null)
        {
            fontName = SelectedCollectedFont.Name;
            skTypeface = SKTypeface.FromFile(SelectedCollectedFont.FilePath, SelectedCollectedFont.FaceIndex);
        }

        if (fontName == null || skTypeface == null)
        {
            FontPreview?.Dispose();
            FontPreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
            return;
        }

        var imageInfo = new SKImageInfo(750, 150, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        using var font = new SKFont(skTypeface, 32);
        using var paint = new SKPaint
        {
            Color = SKColors.Orange,
            IsAntialias = true,
        };

        var y = 25f;
        foreach (var line in new[] { fontName, "I know the quick brown fox jumps over the lazy dog.", "0123456789" })
        {
            canvas.DrawText(line, 12, y, SKTextAlign.Left, font, paint);
            y += font.Size + 5;
        }

        using var skImage = surface.Snapshot();
        var skBitmap = SKBitmap.FromImage(skImage);
        FontPreview?.Dispose();
        FontPreview = skBitmap.ToAvaloniaBitmap();
    }

    /// <summary>
    /// Loads the face in <paramref name="fontFile"/> whose family/face name matches
    /// <paramref name="fontName"/> (a .ttc/.otc holds several); falls back to the first face.
    /// </summary>
    private static SKTypeface? LoadTypefaceForName(string fontFile, string fontName)
    {
        for (var index = 0; index < 30; index++)
        {
            var candidate = SKTypeface.FromFile(fontFile, index);
            if (candidate == null)
            {
                break;
            }

            if (fontName.Equals(candidate.FamilyName, StringComparison.OrdinalIgnoreCase) ||
                fontName.Equals(FontHelper.GetLibAssaFontName(candidate), StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }

            candidate.Dispose();
        }

        return SKTypeface.FromFile(fontFile, 0);
    }

    /// <summary>Like <see cref="LoadTypefaceForName"/>, but for an embedded font's bytes.</summary>
    private static SKTypeface? LoadTypefaceForNameFromBytes(byte[] bytes, string fontName)
    {
        using var data = SKData.CreateCopy(bytes);
        for (var index = 0; index < 30; index++)
        {
            var candidate = SKTypeface.FromData(data, index);
            if (candidate == null)
            {
                break;
            }

            if (fontName.Equals(candidate.FamilyName, StringComparison.OrdinalIgnoreCase) ||
                fontName.Equals(FontHelper.GetLibAssaFontName(candidate), StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }

            candidate.Dispose();
        }

        return SKTypeface.FromData(data, 0);
    }

    /// <summary>Deletes the selected collected font's file from SE's Fonts folder.</summary>
    [RelayCommand]
    private async Task DeleteCollectedFont()
    {
        var selected = SelectedCollectedFont;
        if (Window == null || selected == null)
        {
            return;
        }

        var answer = await MessageBox.Show(
            Window,
            Se.Language.General.Delete,
            string.Format(Se.Language.Assa.FontCollectorDeleteFontXPrompt, Path.GetFileName(selected.FilePath)),
            MessageBoxButtons.YesNo);
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            File.Delete(selected.FilePath);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Font collector delete failed");
            await MessageBox.Show(Window, Se.Language.General.Error, exception.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SelectedCollectedFont = null;
        _ = Task.Run(LoadFontLists);
    }

    /// <summary>
    /// Keeps <see cref="SelectedFontItem"/> in sync even if the SelectedItem binding
    /// does not fire for the multi-select grid, and refreshes the preview.
    /// </summary>
    internal void FontItemsGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is TableView { SelectedItem: FontCollectorItem item })
        {
            SelectedFontItem = item;
        }

        UpdateFontPreview();
    }

    internal void CollectedFontsContextMenuOpening(object? sender, EventArgs e)
    {
        IsDeleteCollectedFontVisible = SelectedCollectedFont != null;
    }

    internal void CollectedFontsGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && SelectedCollectedFont != null)
        {
            e.Handled = true;
            _ = DeleteCollectedFont();
        }
    }

    [RelayCommand]
    private async Task OpenSeFontsFolder()
    {
        if (Window == null)
        {
            return;
        }

        Directory.CreateDirectory(Se.FontsFolder);
        await _folderHelper.OpenFolder(Window, Se.FontsFolder);
    }

    /// <summary>
    /// Collects the font names an ASSA renderer would need: fonts of styles that are
    /// actually used by lines, plus inline <c>\fn</c> overrides.
    /// </summary>
    internal void CollectFontNames(Subtitle subtitle)
    {
        var usage = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        void AddUsage(string fontName, string usedIn)
        {
            fontName = fontName.Trim().TrimStart('@'); // "@" prefix = vertical variant of the same font
            if (fontName.Length == 0)
            {
                return;
            }

            if (!usage.TryGetValue(fontName, out var list))
            {
                list = new List<string>();
                usage[fontName] = list;
            }

            if (!list.Contains(usedIn))
            {
                list.Add(usedIn);
            }
        }

        var header = subtitle.Header ?? string.Empty;
        var usedStyleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var paragraph in subtitle.Paragraphs)
        {
            usedStyleNames.Add(string.IsNullOrEmpty(paragraph.Extra) ? "Default" : paragraph.Extra);

            foreach (Match match in AssaFontEmbedder.FontNameTagRegex.Matches(paragraph.Text))
            {
                AddUsage(match.Groups["name"].Value, string.Format(Se.Language.Assa.FontCollectorInlineLineX, paragraph.Number));
            }
        }

        if (header.Contains("[V4", StringComparison.Ordinal))
        {
            foreach (var styleName in AdvancedSubStationAlpha.GetStylesFromHeader(header))
            {
                if (!usedStyleNames.Contains(styleName))
                {
                    continue;
                }

                var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, header);
                AddUsage(style.FontName, string.Format(Se.Language.Assa.FontCollectorStyleX, styleName));
            }
        }

        foreach (var kvp in usage.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
        {
            FontItems.Add(new FontCollectorItem(kvp.Key, string.Join(", ", kvp.Value)));
        }
    }

    /// <summary>
    /// Scans the font folders (via <see cref="FontHelper.FindFontFiles"/>) and posts a UI
    /// update for every file whose family/face name matches a collected font name.
    /// </summary>
    private void ScanSystemFonts(List<FontCollectorItem> items, CancellationToken cancellationToken)
    {
        try
        {
            // One name per item, and FindFontFiles reports each (name, file) match only
            // once, so posted adds cannot double up on an item.
            var wanted = items.ToDictionary(i => i.FontName, i => i, StringComparer.OrdinalIgnoreCase);
            FontHelper.FindFontFiles(wanted.Keys, cancellationToken, (name, file) =>
            {
                var item = wanted[name];
                Dispatcher.UIThread.Post(() =>
                {
                    item.FoundFiles.Add(file);
                    item.UpdateFileDisplay();
                    item.Status = Se.Language.Assa.FontCollectorFound;
                    if (item == SelectedFontItem)
                    {
                        UpdateFontPreview(); // the selected row just became previewable
                    }
                });
            });
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Font collector scan failed");
        }
        finally
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var item in FontItems)
                {
                    if (item.FoundFiles.Count == 0 && item.EmbeddedFontBytes == null)
                    {
                        item.Status = Se.Language.Assa.FontCollectorNotFound;
                    }
                }

                IsScanning = false;
                var foundCount = FontItems.Count(i => i.FoundFiles.Count > 0 || i.EmbeddedFontBytes != null);
                StatusText = string.Format(Se.Language.Assa.FontCollectorXOfYFontsFound, foundCount, FontItems.Count);
                UpdateFontPreview(); // the selected row may only now have found files
            });
        }
    }

    /// <summary>
    /// Picks font files from disk and adds them to SE's Fonts folder collection.
    /// </summary>
    [RelayCommand]
    private async Task ImportFont()
    {
        if (Window == null)
        {
            return;
        }

        var fileNames = await _fileHelper.PickOpenFiles(
            Window,
            Se.Language.Assa.FontCollectorImportFontDotDotDot.TrimEnd('.'),
            Se.Language.General.Fonts,
            ["*.ttf", "*.otf", "*.ttc", "*.otc"],
            string.Empty,
            []);
        if (fileNames.Length == 0)
        {
            return;
        }

        await CopyFontsTo(fileNames.ToList(), Se.FontsFolder);
        _ = Task.Run(LoadFontLists);
    }

    /// <summary>
    /// Copies the found fonts into SE's own Fonts folder (<see cref="Se.FontsFolder"/>),
    /// building a collection that both the scan and the ASSA styles font list pick up.
    /// </summary>
    [RelayCommand]
    private async Task CopyFontsToSeFontsFolder()
    {
        if (Window == null)
        {
            return;
        }

        var files = GetFoundFontFiles();

        // Fonts only present as subtitle attachments have no file on disk - write their bytes.
        var embedded = FontItems
            .Where(i => i.FoundFiles.Count == 0 && i.EmbeddedFontBytes != null)
            .DistinctBy(i => i.EmbeddedFileName, StringComparer.OrdinalIgnoreCase)
            .Select(i => (i.EmbeddedFileName, i.EmbeddedFontBytes!))
            .ToList();

        if (files.Count == 0 && embedded.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.Assa.FontCollectorTitle, Se.Language.Assa.FontCollectorNoFontsToCopy, MessageBoxButtons.OK);
            return;
        }

        await CopyFontsTo(files, Se.FontsFolder, embedded);
        _ = Task.Run(LoadFontLists); // the "Collected fonts" tab just gained fonts
    }

    /// <summary>
    /// Copies the font selected in the "Installed fonts" tab into SE's Fonts folder.
    /// Installed fonts are known by name only, so the system font folders are searched
    /// for the file(s) carrying that family/face name.
    /// </summary>
    [RelayCommand]
    private async Task CopyInstalledFontToSeFontsFolder()
    {
        var fontName = SelectedInstalledFontName;
        if (Window == null || string.IsNullOrEmpty(fontName))
        {
            return;
        }

        var files = await Task.Run(() => FindSystemFontFiles(fontName));
        if (files.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.Assa.FontCollectorTitle, Se.Language.Assa.FontCollectorNoFontsToCopy, MessageBoxButtons.OK);
            return;
        }

        await CopyFontsTo(files, Se.FontsFolder);
        _ = Task.Run(LoadFontLists);
    }

    private static List<string> FindSystemFontFiles(string fontName)
    {
        // Skip(1): index 0 is Se.FontsFolder - already collected.
        return FontHelper.FindFontFiles([fontName], CancellationToken.None,
                folders: FontHelper.GetFontFolders().Skip(1))
            .Values.SelectMany(f => f).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private List<string> GetFoundFontFiles()
    {
        return FontItems.SelectMany(i => i.FoundFiles).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Embeds the needed fonts that were found on disk into the subtitle's [Fonts]
    /// attachment section, after a confirmation showing count, file names and sizes.
    /// The updated footer reaches the caller via <see cref="UpdatedFooter"/>.
    /// </summary>
    [RelayCommand]
    private async Task EmbedFontsInSubtitle()
    {
        if (Window == null)
        {
            return;
        }

        var embeddedFileNames = new HashSet<string>(
            AssaFontEmbedder.GetEmbeddedFonts(_subtitle.Footer).Select(f => f.FileName),
            StringComparer.OrdinalIgnoreCase);
        var files = GetFoundFontFiles()
            .Where(f => !embeddedFileNames.Contains(Path.GetFileName(f)))
            .ToList();
        if (files.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.Assa.FontCollectorTitle, Se.Language.Assa.FontCollectorNoFontsToEmbed, MessageBoxButtons.OK);
            return;
        }

        long totalBytes = 0;
        var fileLines = new List<string>();
        foreach (var file in files)
        {
            var length = new FileInfo(file).Length;
            totalBytes += length;
            fileLines.Add($"{Path.GetFileName(file)} ({Utilities.FormatBytesToDisplayFileSize(length)})");
        }

        const int maxListedFiles = 12;
        if (fileLines.Count > maxListedFiles)
        {
            var more = fileLines.Count - maxListedFiles;
            fileLines = fileLines.Take(maxListedFiles).ToList();
            fileLines.Add(string.Format(Se.Language.Assa.FontCollectorAndXMoreFonts, more));
        }

        var encodedBytes = (long)(totalBytes * 4.0 / 3.0); // the UU-style encoding stores 3 bytes as 4 characters
        var message = string.Format(
            Se.Language.Assa.FontCollectorEmbedXFontsSizeYZPrompt,
            files.Count,
            Utilities.FormatBytesToDisplayFileSize(totalBytes),
            Utilities.FormatBytesToDisplayFileSize(encodedBytes),
            string.Join(Environment.NewLine, fileLines));

        var answer = await MessageBox.Show(Window, Se.Language.Assa.FontCollectorTitle, message, MessageBoxButtons.YesNo);
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        var footer = _subtitle.Footer;
        var count = 0;
        foreach (var file in files)
        {
            try
            {
                footer = AssaFontEmbedder.AddFontToFooter(footer, file, await File.ReadAllBytesAsync(file));
                count++;
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Font collector could not embed " + file);
            }
        }

        _subtitle.Footer = footer;
        UpdatedFooter = footer;
        MatchEmbeddedFonts(footer); // the rows just became "Embedded"

        await MessageBox.Show(
            Window,
            Se.Language.Assa.FontCollectorTitle,
            string.Format(Se.Language.Assa.FontCollectorXFontFilesEmbedded, count),
            MessageBoxButtons.OK);
    }

    private async Task CopyFontsTo(List<string> files, string folder, List<(string FileName, byte[] Bytes)>? embeddedFonts = null)
    {
        var copied = 0;
        try
        {
            Directory.CreateDirectory(folder);
            foreach (var file in files)
            {
                var target = Path.Combine(folder, Path.GetFileName(file));
                if (string.Equals(Path.GetFullPath(file), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                {
                    continue; // already in the target folder
                }

                File.Copy(file, target, overwrite: true);
                copied++;
            }

            foreach (var (fileName, bytes) in embeddedFonts ?? [])
            {
                await File.WriteAllBytesAsync(Path.Combine(folder, fileName), bytes);
                copied++;
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Font collector copy failed");
            await MessageBox.Show(Window!, Se.Language.General.Error, exception.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        await MessageBox.Show(
            Window!,
            Se.Language.Assa.FontCollectorTitle,
            string.Format(Se.Language.Assa.FontCollectorXFontFilesCopiedToY, copied, folder),
            MessageBoxButtons.OK);
    }

    [RelayCommand]
    private void Close()
    {
        _cancellationTokenSource.Cancel();
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}
