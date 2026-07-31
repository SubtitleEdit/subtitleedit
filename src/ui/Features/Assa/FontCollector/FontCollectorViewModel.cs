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
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _isScanning;
    [ObservableProperty] private ObservableCollection<string> _installedFontNames;
    [ObservableProperty] private string? _selectedInstalledFontName;
    [ObservableProperty] private ObservableCollection<CollectedFont> _collectedFonts;
    [ObservableProperty] private CollectedFont? _selectedCollectedFont;
    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private Bitmap? _fontPreview;

    public Window? Window { get; set; }

    private static readonly Regex FontNameTagRegex = new(@"\\fn(?<name>[^\\}]+)", RegexOptions.Compiled);

    private readonly IFolderHelper _folderHelper;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public FontCollectorViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;
        FontItems = new ObservableCollection<FontCollectorItem>();
        InstalledFontNames = new ObservableCollection<string>();
        CollectedFonts = new ObservableCollection<CollectedFont>();
        StatusText = string.Empty;
    }

    public void Initialize(Subtitle subtitle)
    {
        CollectFontNames(subtitle);
        IsScanning = true;
        StatusText = Se.Language.Assa.FontCollectorScanning;

        var items = FontItems.ToList();
        _ = Task.Run(() => ScanSystemFonts(items, _cancellationTokenSource.Token));
        _ = Task.Run(LoadFontLists);
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

    partial void OnSelectedInstalledFontNameChanged(string? value) => UpdateFontPreview();

    partial void OnSelectedCollectedFontChanged(CollectedFont? value) => UpdateFontPreview();

    /// <summary>
    /// Renders a sample-text preview of the font selected in the installed/collected tab.
    /// A collected font need not be installed, so it is rendered from its file.
    /// </summary>
    private void UpdateFontPreview()
    {
        string? fontName = null;
        SKTypeface? skTypeface = null;

        if (SelectedTabIndex == 1 && !string.IsNullOrEmpty(SelectedInstalledFontName))
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
    private void CollectFontNames(Subtitle subtitle)
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

            foreach (Match match in FontNameTagRegex.Matches(paragraph.Text))
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
    /// Scans the platform font directories and matches each font file's family/face names
    /// against the collected font names. Skia has no file-path API, so files are read
    /// directly; both the typographic family name and the Win32/GDI face name (what libass
    /// matches) are checked.
    /// </summary>
    private void ScanSystemFonts(List<FontCollectorItem> items, CancellationToken cancellationToken)
    {
        try
        {
            var wanted = items.ToDictionary(i => i.FontName, i => i, StringComparer.OrdinalIgnoreCase);

            // item.FoundFiles is only mutated in posted UI updates, so it cannot be used for
            // duplicate checks on this thread - the family and face name of a regular font are
            // usually identical, and both would queue an Add before either has run.
            var found = items.ToDictionary(i => i, i => new HashSet<string>(StringComparer.OrdinalIgnoreCase));

            foreach (var fontFile in EnumerateFontFiles())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                // A collection (.ttc/.otc) holds several faces; plain files have one at index 0.
                for (var index = 0; index < 30; index++)
                {
                    using var typeface = SKTypeface.FromFile(fontFile, index);
                    if (typeface == null)
                    {
                        break;
                    }

                    var names = new[] { typeface.FamilyName, FontHelper.GetLibAssaFontName(typeface) };
                    foreach (var name in names)
                    {
                        if (!string.IsNullOrEmpty(name) &&
                            wanted.TryGetValue(name, out var item) &&
                            found[item].Add(fontFile))
                        {
                            var file = fontFile;
                            Dispatcher.UIThread.Post(() =>
                            {
                                item.FoundFiles.Add(file);
                                item.UpdateFileDisplay();
                                item.Status = Se.Language.Assa.FontCollectorFound;
                            });
                        }
                    }
                }
            }
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
                    if (item.FoundFiles.Count == 0)
                    {
                        item.Status = Se.Language.Assa.FontCollectorNotFound;
                    }
                }

                IsScanning = false;
                var foundCount = FontItems.Count(i => i.FoundFiles.Count > 0);
                StatusText = string.Format(Se.Language.Assa.FontCollectorXOfYFontsFound, foundCount, FontItems.Count);
            });
        }
    }

    private static IEnumerable<string> EnumerateFontFiles()
    {
        return GetFontFolders().SelectMany(FontHelper.EnumerateFontFiles);
    }

    private static List<string> GetFontFolders()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // SE's own font collection is scanned first, so a collected font counts as
        // found even when it is not installed on the system.
        var folders = new List<string> { Se.FontsFolder };

        if (OperatingSystem.IsWindows())
        {
            folders.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts"));
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            folders.Add(Path.Combine(localAppData, "Microsoft", "Windows", "Fonts"));
        }
        else if (OperatingSystem.IsMacOS())
        {
            folders.Add("/System/Library/Fonts");
            folders.Add("/Library/Fonts");
            folders.Add(Path.Combine(home, "Library", "Fonts"));
        }
        else
        {
            folders.Add("/usr/share/fonts");
            folders.Add("/usr/local/share/fonts");
            folders.Add(Path.Combine(home, ".fonts"));
            folders.Add(Path.Combine(home, ".local", "share", "fonts"));
        }

        return folders;
    }

    [RelayCommand]
    private async Task CopyFontsToFolder()
    {
        if (Window == null)
        {
            return;
        }

        var files = GetFoundFontFiles();
        if (files.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.Assa.FontCollectorTitle, Se.Language.Assa.FontCollectorNoFontsToCopy, MessageBoxButtons.OK);
            return;
        }

        var folder = await _folderHelper.PickFolderAsync(Window, Se.Language.Assa.FontCollectorCopyFontsToFolder);
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        await CopyFontsTo(files, folder);
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
        if (files.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.Assa.FontCollectorTitle, Se.Language.Assa.FontCollectorNoFontsToCopy, MessageBoxButtons.OK);
            return;
        }

        await CopyFontsTo(files, Se.FontsFolder);
        _ = Task.Run(LoadFontLists); // the "Collected fonts" tab just gained fonts
    }

    private List<string> GetFoundFontFiles()
    {
        return FontItems.SelectMany(i => i.FoundFiles).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private async Task CopyFontsTo(List<string> files, string folder)
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
