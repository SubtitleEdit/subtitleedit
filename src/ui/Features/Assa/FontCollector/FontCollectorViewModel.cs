using Avalonia.Controls;
using Avalonia.Input;
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

    public Window? Window { get; set; }

    private static readonly Regex FontNameTagRegex = new(@"\\fn(?<name>[^\\}]+)", RegexOptions.Compiled);
    private static readonly string[] FontFileExtensions = { ".ttf", ".otf", ".ttc", ".otc" };

    private readonly IFolderHelper _folderHelper;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public FontCollectorViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;
        FontItems = new ObservableCollection<FontCollectorItem>();
        StatusText = string.Empty;
    }

    public void Initialize(Subtitle subtitle)
    {
        CollectFontNames(subtitle);
        IsScanning = true;
        StatusText = Se.Language.Assa.FontCollectorScanning;

        var items = FontItems.ToList();
        _ = Task.Run(() => ScanSystemFonts(items, _cancellationTokenSource.Token));
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
        foreach (var folder in GetFontFolders().Where(Directory.Exists))
        {
            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(folder, "*.*", new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true,
                });
            }
            catch
            {
                continue;
            }

            foreach (var file in files)
            {
                if (FontFileExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                {
                    yield return file;
                }
            }
        }
    }

    private static List<string> GetFontFolders()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var folders = new List<string>();

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

        var files = FontItems.SelectMany(i => i.FoundFiles).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
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

        var copied = 0;
        try
        {
            foreach (var file in files)
            {
                File.Copy(file, Path.Combine(folder, Path.GetFileName(file)), overwrite: true);
                copied++;
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Font collector copy failed");
            await MessageBox.Show(Window, Se.Language.General.Error, exception.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        await MessageBox.Show(
            Window,
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
