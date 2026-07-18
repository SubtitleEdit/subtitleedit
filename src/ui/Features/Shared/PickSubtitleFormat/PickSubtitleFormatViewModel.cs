using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;

/// <summary>
/// A format offered by the picker that is not a real <see cref="SubtitleFormat"/> (e.g. batch-convert
/// image outputs like Blu-ray sup). <paramref name="PreviewText"/> null means "no text preview".
/// </summary>
public sealed record PickSubtitleFormatExtraFormat(string Name, string? PreviewText);

public partial class PickSubtitleFormatViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private ObservableCollection<string> _subtitleFormatNames;
    [ObservableProperty] private string? _selectedSubtitleFormatName;
    [ObservableProperty] private string _previewText;

    public Window? Window { get; set; }
    public Border PreviewContainer { get; set; }

    public bool OkPressed { get; private set; }
    
    private readonly List<string> _allSubtitleFormatNames;
    private Subtitle _sampleSubtitle;

    // Extra (non-SubtitleFormat) entries, e.g. batch-convert image/text outputs. Value is the preview
    // text, or null for formats with no text preview (image-based).
    private readonly Dictionary<string, string?> _extraFormatPreviews = new();

    public PickSubtitleFormatViewModel()
    {
       _allSubtitleFormatNames = SubtitleFormatHelper.GetSubtitleFormatsWithFavoritesAtTop()
           .Select(x => x.FriendlyName)
           .ToList();
        
        SubtitleFormatNames = new ObservableCollection<string>(_allSubtitleFormatNames);
        SearchText = string.Empty;
        PreviewText = string.Empty;
        PreviewContainer = new Border();
        
        // Create a sample subtitle for preview
        _sampleSubtitle = new Subtitle();
        _sampleSubtitle.Paragraphs.Add(new Paragraph("Hello, World!", 1000, 3000));
        _sampleSubtitle.Paragraphs.Add(new Paragraph("This is a sample subtitle.", 3500, 6000));
    }

    internal void Initialize(SubtitleFormat? subtitleFormat, Subtitle subtitle,
        IReadOnlyList<PickSubtitleFormatExtraFormat>? extraFormats = null, string? selectedFormatName = null)
    {
        if (extraFormats != null)
        {
            foreach (var extra in extraFormats)
            {
                _extraFormatPreviews[extra.Name] = extra.PreviewText;
                if (!_allSubtitleFormatNames.Contains(extra.Name))
                {
                    _allSubtitleFormatNames.Add(extra.Name);
                    SubtitleFormatNames.Add(extra.Name);
                }
            }
        }

        SelectedSubtitleFormatName = selectedFormatName ?? subtitleFormat?.FriendlyName;

        if (subtitle.Paragraphs.Count > 1)
        {
            _sampleSubtitle = new Subtitle(subtitle);

            if (_sampleSubtitle.Footer != null && _sampleSubtitle.Footer.Length > 100)
            {
                _sampleSubtitle.Footer = string.Empty;
            }

            while (_sampleSubtitle.Paragraphs.Count > 2)
            {
                _sampleSubtitle.Paragraphs.RemoveAt(2);
            }
        }

        UpdatePreview();
    }

    public SubtitleFormat? GetSelectedFormat()
    {
        if (string.IsNullOrEmpty(SelectedSubtitleFormatName))
        {
            return null;
        }

        return SubtitleFormat.AllSubtitleFormats.FirstOrDefault(x => x.FriendlyName == SelectedSubtitleFormatName);
    }

    private void UpdateSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText) && SubtitleFormatNames.Count == _allSubtitleFormatNames.Count)
        {
            return;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            SubtitleFormatNames.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SubtitleFormatNames.AddRange(_allSubtitleFormatNames);
                return;
            }

            foreach (var name in _allSubtitleFormatNames)
            {
                if (name.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase) ||
                    name.Replace("-", string.Empty).Contains(SearchText, StringComparison.InvariantCultureIgnoreCase))
                {
                    SubtitleFormatNames.Add(name);
                }
            }

            if (SubtitleFormatNames.Count > 0)
            {
                SelectedSubtitleFormatName = SubtitleFormatNames[0];
            }
        });
    }

    private void UpdatePreview()
    {
        if (string.IsNullOrEmpty(SelectedSubtitleFormatName))
        {
            PreviewText = string.Empty;
            PreviewContainer.Child = null;
            return;
        }

        try
        {
            var format = SubtitleFormat.AllSubtitleFormats.FirstOrDefault(x => x.FriendlyName == SelectedSubtitleFormatName);
            if (format != null)
            {
                var text = format.ToText(_sampleSubtitle, "Sample");
                ShowTextPreview(text, format);
            }
            else if (_extraFormatPreviews.TryGetValue(SelectedSubtitleFormatName, out var extraPreview))
            {
                // Batch-only format (e.g. image output): show its supplied preview text, or a
                // placeholder for formats that have no text representation.
                if (string.IsNullOrEmpty(extraPreview))
                {
                    ShowPlaceholderPreview(Se.Language.Tools.PickSubtitleFormatImageBasedNoPreview);
                }
                else
                {
                    ShowTextPreview(extraPreview, null);
                }
            }
            else
            {
                PreviewText = string.Empty;
                PreviewContainer.Child = null;
            }
        }
        catch (Exception ex)
        {
            PreviewText = $"Error generating preview:\n{ex.Message}";
            var textBox = new TextBox
            {
                Text = PreviewText,
                IsReadOnly = true,
                AcceptsReturn = true,
            };
            PreviewContainer.Child = textBox;
        }
    }

    // Renders read-only source text (optionally syntax-highlighted for a known SubtitleFormat).
    private void ShowTextPreview(string text, SubtitleFormat? format)
    {
        // Limit preview to first 1000 characters for better display
        if (text.Length > 1000)
        {
            text = text.Substring(0, 1000) + "\n\n... (truncated)";
        }

        PreviewText = text;

        var textEditor = new TextEditor
        {
            Text = text,
            IsReadOnly = true,
            ShowLineNumbers = true,
            WordWrap = false,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            FontFamily = new FontFamily("Courier New, Consolas, monospace"),
            FontSize = 12,
        };

        textEditor.TextArea.TextView.LinkTextForegroundBrush = UiUtil.MakeLinkForeground();

        var lineTransformer = format != null ? GetLineTransformer(text, format) : null;
        if (lineTransformer != null)
        {
            textEditor.TextArea.TextView.LineTransformers.Add(lineTransformer);
        }

        PreviewContainer.Child = textEditor;
    }

    // Shows an informational note for formats with no text preview (image-based outputs).
    private void ShowPlaceholderPreview(string message)
    {
        PreviewText = message;
        PreviewContainer.Child = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Opacity = 0.7,
        };
    }

    private static DocumentColorizingTransformer? GetLineTransformer(string text, SubtitleFormat subtitleFormat)
    {
        // SubRip (.srt) and WebVTT (.vtt) use similar time code formats
        if (subtitleFormat is SubRip ||
            subtitleFormat is WebVTT ||
            subtitleFormat is WebVTTFileWithLineNumber)
        {
            return new SubRipSourceSyntaxHighlighting();
        }

        // Advanced SubStation Alpha (.ass) and SubStation Alpha (.ssa) formats
        if (subtitleFormat is AdvancedSubStationAlpha || subtitleFormat is SubStationAlpha)
        {
            return new AssaSourceSyntaxHighlighting();
        }

        // XML-based formats (e.g., TTML, Netflix DFXP, etc.)
        if (subtitleFormat.Extension == ".xml" ||
            subtitleFormat.AlternateExtensions.Contains(".xml") ||
            text.Contains("<?xml version=") ||
            subtitleFormat is Sami ||
            subtitleFormat is SamiModern ||
            subtitleFormat is SamiYouTube ||
            subtitleFormat is SamiAvDicPlayer)
        {
            return new XmlSourceSyntaxHighlighting();
        }

        // Json-based formats
        if (subtitleFormat.Extension == ".json" ||
            subtitleFormat.AlternateExtensions.Contains(".json"))
        {
            return new JsonSourceSyntaxHighlighting();
        }

        // No syntax highlighting for other formats
        return null;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Enter && SelectedSubtitleFormatName != null)
        {
            e.Handled = true;
            Ok();
        }
    }

    public void SearchTextChanged()
    {
        UpdateSearch();
    }
    
    public void SelectedSubtitleFormatNameChanged()
    {
        UpdatePreview();
    }
}