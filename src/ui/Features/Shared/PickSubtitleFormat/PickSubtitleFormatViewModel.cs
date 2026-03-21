using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

namespace Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;

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

    internal void Initialize(SubtitleFormat? subtitleFormat, Subtitle subtitle)
    {
        SelectedSubtitleFormatName = subtitleFormat?.FriendlyName;

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
                ObservableCollectionExtensions.AddRange(SubtitleFormatNames, _allSubtitleFormatNames);
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
                PreviewText = text;
                
                // Limit preview to first 1000 characters for better display
                if (text.Length > 1000)
                {
                    text = text.Substring(0, 1000) + "\n\n... (truncated)";
                    PreviewText = text;
                }

                // Create TextEditor with syntax highlighting
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

                // Override the built-in link color with our softer pastel color
                textEditor.TextArea.TextView.LinkTextForegroundBrush = UiUtil.MakeLinkForeground();

                // Add syntax highlighting for subtitle source formats
                var lineTransformer = GetLineTransformer(text, format);
                if (lineTransformer != null)
                {
                    textEditor.TextArea.TextView.LineTransformers.Add(lineTransformer);
                }

                PreviewContainer.Child = textEditor;
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