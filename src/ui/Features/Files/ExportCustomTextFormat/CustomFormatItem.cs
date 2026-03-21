using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

public partial class CustomFormatItem : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _extension;
    [ObservableProperty] private string _formatHeader;
    [ObservableProperty] private string _formatParagraph;
    [ObservableProperty] private string _formatFooter;
    [ObservableProperty] private string _formatTimeCode;
    [ObservableProperty] private string? _formatNewLine;

    public CustomFormatItem()
    {
        Name = string.Empty;
        Extension = string.Empty;
        FormatHeader = string.Empty;
        FormatParagraph = string.Empty;
        FormatFooter = string.Empty;
        FormatTimeCode = string.Empty;
        FormatNewLine = null;
    }

    public CustomFormatItem(string name, string extension, string formatHeader, string formatParagraph, string formatFooter, string formatTimeCode, string? formatNewLine)
    {
        Name = name;
        Extension = extension;
        FormatHeader = formatHeader;
        FormatParagraph = formatParagraph;
        FormatFooter = formatFooter;
        FormatTimeCode = formatTimeCode;
        FormatNewLine = formatNewLine;
    }

    public CustomFormatItem(SeExportCustomFormatItem customFormat)
    {
        Name = customFormat.Name;
        Extension = customFormat.Extension;
        FormatHeader = customFormat.FormatHeader;
        FormatParagraph = customFormat.FormatParagraph;
        FormatFooter = customFormat.FormatFooter;
        FormatTimeCode = customFormat.FormatTimeCode;
        FormatNewLine = customFormat.FormatNewLine;
    }

    public override string ToString()
    {
        return Name;
    }

    internal SeExportCustomFormatItem ToCustomFormat()
    {
        return new SeExportCustomFormatItem
        {
            Name = Name,
            Extension = Extension,
            FormatHeader = FormatHeader,
            FormatParagraph = FormatParagraph,
            FormatFooter = FormatFooter,
            FormatTimeCode = FormatTimeCode,
            FormatNewLine = FormatNewLine
        };
    }
}
